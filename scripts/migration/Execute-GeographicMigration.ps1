# =====================================================
# Geographic Tables Unification - PowerShell Execution Script
# =====================================================
# This script executes the complete geographic migration process
# =====================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = ".\backups",
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackup = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose = $false
)

# Import required modules
Import-Module SqlServer -ErrorAction SilentlyContinue

# Configuration
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogFile = Join-Path $ScriptPath "migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$DeploymentId = "GEOGRAPHIC_MIGRATION_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Logging function
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "[$Timestamp] [$Level] $Message"
    
    Write-Host $LogMessage
    Add-Content -Path $LogFile -Value $LogMessage
    
    if ($Verbose) {
        Write-Verbose $LogMessage
    }
}

# Error handling function
function Handle-Error {
    param(
        [string]$ErrorMessage,
        [string]$Step
    )
    
    Write-Log "ERROR in step '$Step': $ErrorMessage" "ERROR"
    Write-Host "Migration failed at step: $Step" -ForegroundColor Red
    Write-Host "Check log file: $LogFile" -ForegroundColor Yellow
    
    # Attempt rollback if not in dry run mode
    if (-not $DryRun) {
        Write-Host "Attempting automatic rollback..." -ForegroundColor Yellow
        try {
            Execute-Rollback
        }
        catch {
            Write-Log "Rollback failed: $($_.Exception.Message)" "ERROR"
            Write-Host "Manual rollback may be required!" -ForegroundColor Red
        }
    }
    
    exit 1
}

# Execute SQL script function
function Execute-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$StepName
    )
    
    Write-Log "Executing step: $StepName"
    Write-Log "Script: $ScriptPath"
    
    if ($DryRun) {
        Write-Log "DRY RUN: Would execute $ScriptPath" "INFO"
        return
    }
    
    try {
        if (Test-Path $ScriptPath) {
            $ScriptContent = Get-Content $ScriptPath -Raw
            
            # Replace deployment ID placeholder
            $ScriptContent = $ScriptContent -replace ':deployment_id', "'$DeploymentId'"
            
            # Execute the script
            Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $ScriptContent -QueryTimeout 300
            
            Write-Log "Successfully completed: $StepName" "SUCCESS"
        }
        else {
            throw "Script file not found: $ScriptPath"
        }
    }
    catch {
        Handle-Error $_.Exception.Message $StepName
    }
}

# Backup database function
function Create-DatabaseBackup {
    if ($SkipBackup) {
        Write-Log "Skipping database backup as requested" "WARNING"
        return
    }
    
    Write-Log "Creating database backup..."
    
    if (-not (Test-Path $BackupPath)) {
        New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    }
    
    $BackupFile = Join-Path $BackupPath "agriis_pre_migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
    
    try {
        # Extract database name from connection string
        $DbName = ($ConnectionString -split ';' | Where-Object { $_ -like 'Database=*' } | ForEach-Object { $_.Split('=')[1] })
        
        if ($DryRun) {
            Write-Log "DRY RUN: Would create backup at $BackupFile" "INFO"
        }
        else {
            $BackupQuery = "BACKUP DATABASE [$DbName] TO DISK = '$BackupFile'"
            Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $BackupQuery -QueryTimeout 600
            Write-Log "Database backup created: $BackupFile" "SUCCESS"
        }
    }
    catch {
        Write-Log "Backup failed: $($_.Exception.Message)" "ERROR"
        Write-Host "Backup failed, but continuing with migration..." -ForegroundColor Yellow
    }
}

# Pre-migration validation
function Test-PreMigrationConditions {
    Write-Log "Performing pre-migration validation..."
    
    try {
        # Test database connection
        $TestQuery = "SELECT 1 as test"
        $Result = Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $TestQuery
        
        if (-not $Result) {
            throw "Database connection test failed"
        }
        
        # Check if required tables exist
        $TableCheckQuery = @"
            SELECT COUNT(*) as table_count 
            FROM information_schema.tables 
            WHERE table_name IN ('estados', 'municipios', 'estados_referencia', 'municipios_referencia')
"@
        
        $TableCount = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $TableCheckQuery).table_count
        
        if ($TableCount -lt 4) {
            throw "Required tables not found. Expected 4 tables, found $TableCount"
        }
        
        # Check for existing migration
        $MigrationCheckQuery = @"
            SELECT COUNT(*) as log_count 
            FROM information_schema.tables 
            WHERE table_name = 'migration_log'
"@
        
        $LogTableExists = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $MigrationCheckQuery).log_count -gt 0
        
        if ($LogTableExists) {
            $ExistingMigrationQuery = "SELECT COUNT(*) as migration_count FROM migration_log WHERE step = 'main_transaction' AND status = 'completed'"
            $ExistingMigration = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $ExistingMigrationQuery).migration_count
            
            if ($ExistingMigration -gt 0) {
                Write-Log "Previous migration detected. This may be a re-run." "WARNING"
                
                if (-not $DryRun) {
                    $Continue = Read-Host "Continue with migration? (y/N)"
                    if ($Continue -ne 'y' -and $Continue -ne 'Y') {
                        Write-Log "Migration cancelled by user"
                        exit 0
                    }
                }
            }
        }
        
        Write-Log "Pre-migration validation passed" "SUCCESS"
    }
    catch {
        Handle-Error $_.Exception.Message "Pre-migration validation"
    }
}

# Execute rollback
function Execute-Rollback {
    Write-Log "Executing rollback procedure..."
    
    $RollbackQuery = "SELECT * FROM rollback_geographic_migration('$DeploymentId')"
    
    try {
        $RollbackResult = Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $RollbackQuery
        
        foreach ($Row in $RollbackResult) {
            Write-Log "Rollback step: $($Row.step_name) - $($Row.status) - $($Row.message)"
        }
        
        Write-Log "Rollback completed" "SUCCESS"
    }
    catch {
        Write-Log "Rollback failed: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# Post-migration validation
function Test-PostMigrationConditions {
    Write-Log "Performing post-migration validation..."
    
    try {
        # Check final table counts
        $EstadosCount = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query "SELECT COUNT(*) as count FROM estados").count
        $MunicipiosCount = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query "SELECT COUNT(*) as count FROM municipios").count
        
        if ($EstadosCount -lt 27) {
            throw "Estados count validation failed: $EstadosCount (expected >= 27)"
        }
        
        if ($MunicipiosCount -lt 5000) {
            throw "Municipios count validation failed: $MunicipiosCount (expected >= 5000)"
        }
        
        # Check if reference tables were removed
        $RefTablesQuery = @"
            SELECT COUNT(*) as ref_count 
            FROM information_schema.tables 
            WHERE table_name IN ('estados_referencia', 'municipios_referencia')
"@
        
        $RefTablesCount = (Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $RefTablesQuery).ref_count
        
        if ($RefTablesCount -gt 0) {
            Write-Log "Reference tables still exist (this may be expected in some scenarios)" "WARNING"
        }
        
        Write-Log "Post-migration validation passed" "SUCCESS"
        Write-Log "Estados: $EstadosCount, Municipios: $MunicipiosCount"
    }
    catch {
        Handle-Error $_.Exception.Message "Post-migration validation"
    }
}

# Main execution function
function Start-Migration {
    Write-Log "Starting Geographic Tables Unification Migration"
    Write-Log "Deployment ID: $DeploymentId"
    Write-Log "Connection: $($ConnectionString -replace 'Password=[^;]*', 'Password=***')"
    Write-Log "Dry Run: $DryRun"
    
    # Step 1: Pre-migration validation
    Test-PreMigrationConditions
    
    # Step 2: Create backup
    Create-DatabaseBackup
    
    # Step 3: Execute migration scripts
    $MigrationSteps = @(
        @{ Script = "00_migration_setup.sql"; Name = "Migration Setup" },
        @{ Script = "01_backup_and_analysis.sql"; Name = "Backup and Analysis" },
        @{ Script = "01.5_pais_id_migration.sql"; Name = "PaisId Field Migration" },
        @{ Script = "02_estado_unification.sql"; Name = "Estado Unification" },
        @{ Script = "03_municipio_unification.sql"; Name = "Municipio Unification" },
        @{ Script = "04_foreign_key_updates.sql"; Name = "Foreign Key Updates" },
        @{ Script = "05_migration_transaction.sql"; Name = "Migration Transaction" },
        @{ Script = "06_schema_cleanup.sql"; Name = "Schema Cleanup" },
        @{ Script = "07_deployment_procedures.sql"; Name = "Deployment Procedures" }
    )
    
    foreach ($Step in $MigrationSteps) {
        $ScriptFile = Join-Path $ScriptPath $Step.Script
        Execute-SqlScript -ScriptPath $ScriptFile -StepName $Step.Name
    }
    
    # Step 4: Post-migration validation
    Test-PostMigrationConditions
    
    Write-Log "Migration completed successfully!" "SUCCESS"
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "Log file: $LogFile" -ForegroundColor Cyan
}

# Script execution
try {
    Start-Migration
}
catch {
    Write-Log "Migration failed: $($_.Exception.Message)" "ERROR"
    Write-Host "Migration failed! Check log file: $LogFile" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "GEOGRAPHIC MIGRATION COMPLETED" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run Entity Framework migrations" -ForegroundColor White
Write-Host "2. Execute integration tests" -ForegroundColor White
Write-Host "3. Validate application functionality" -ForegroundColor White
Write-Host "4. Monitor performance" -ForegroundColor White
Write-Host ""