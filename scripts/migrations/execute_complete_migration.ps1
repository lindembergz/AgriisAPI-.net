# =====================================================
# COMPLETE MIGRATION EXECUTOR
# Description: Executes the complete reference entities migration process
# Author: System Migration
# Date: 2025-01-27
# Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6
# =====================================================

param(
    [string]$ConnectionString = $null,
    [string]$DatabaseHost = "localhost",
    [string]$DatabasePort = "5432",
    [string]$DatabaseName = "agriis",
    [string]$DatabaseUser = "postgres",
    [string]$DatabasePassword = $null,
    [switch]$SkipBackup = $false,
    [switch]$SkipValidation = $false,
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "AGRIIS REFERENCE ENTITIES MIGRATION" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Starting complete migration process..." -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Function to execute SQL script
function Execute-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$ConnectionString,
        [string]$Description
    )
    
    Write-Host "Executing: $Description" -ForegroundColor Yellow
    Write-Host "Script: $ScriptPath" -ForegroundColor Gray
    
    if ($DryRun) {
        Write-Host "[DRY RUN] Would execute: $ScriptPath" -ForegroundColor Magenta
        return $true
    }
    
    try {
        if (Test-Path $ScriptPath) {
            $result = psql $ConnectionString -f $ScriptPath -v ON_ERROR_STOP=1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Success: $Description" -ForegroundColor Green
                return $true
            } else {
                Write-Host "✗ Failed: $Description" -ForegroundColor Red
                Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "✗ Script not found: $ScriptPath" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ Error executing $Description`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to create database backup
function Create-DatabaseBackup {
    param(
        [string]$ConnectionString,
        [string]$BackupPath
    )
    
    Write-Host "Creating database backup..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "[DRY RUN] Would create backup at: $BackupPath" -ForegroundColor Magenta
        return $true
    }
    
    try {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupFile = "$BackupPath\agriis_backup_$timestamp.sql"
        
        # Create backup directory if it doesn't exist
        $backupDir = Split-Path $backupFile -Parent
        if (!(Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        }
        
        # Create backup using pg_dump
        $dumpResult = pg_dump $ConnectionString --no-owner --no-privileges --clean --if-exists -f $backupFile
        
        if ($LASTEXITCODE -eq 0 -and (Test-Path $backupFile)) {
            Write-Host "✓ Backup created: $backupFile" -ForegroundColor Green
            return $backupFile
        } else {
            Write-Host "✗ Backup failed" -ForegroundColor Red
            return $null
        }
    } catch {
        Write-Host "✗ Backup error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Function to validate prerequisites
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Yellow
    
    $allGood = $true
    
    # Check if psql is available
    try {
        $psqlVersion = psql --version
        Write-Host "✓ PostgreSQL client available: $($psqlVersion -split ' ' | Select-Object -First 3 -Join ' ')" -ForegroundColor Green
    } catch {
        Write-Host "✗ PostgreSQL client (psql) not found in PATH" -ForegroundColor Red
        $allGood = $false
    }
    
    # Check if pg_dump is available (for backup)
    if (!$SkipBackup) {
        try {
            $pgDumpVersion = pg_dump --version
            Write-Host "✓ pg_dump available: $($pgDumpVersion -split ' ' | Select-Object -First 3 -Join ' ')" -ForegroundColor Green
        } catch {
            Write-Host "✗ pg_dump not found in PATH" -ForegroundColor Red
            $allGood = $false
        }
    }
    
    # Check if migration scripts exist
    $requiredScripts = @(
        "01_populate_reference_tables.sql",
        "02_migrate_produto_references.sql",
        "03_migrate_fornecedor_references.sql"
    )
    
    foreach ($script in $requiredScripts) {
        $scriptPath = Join-Path $PSScriptRoot $script
        if (Test-Path $scriptPath) {
            Write-Host "✓ Script found: $script" -ForegroundColor Green
        } else {
            Write-Host "✗ Script missing: $script" -ForegroundColor Red
            $allGood = $false
        }
    }
    
    return $allGood
}

# Function to test database connection
function Test-DatabaseConnection {
    param([string]$ConnectionString)
    
    Write-Host "Testing database connection..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "[DRY RUN] Would test connection" -ForegroundColor Magenta
        return $true
    }
    
    try {
        $testResult = psql $ConnectionString -c "SELECT version();" -t
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database connection successful" -ForegroundColor Green
            Write-Verbose "Database version: $($testResult.Trim())"
            return $true
        } else {
            Write-Host "✗ Database connection failed" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ Connection error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main execution logic
try {
    # Build connection string if not provided
    if ([string]::IsNullOrEmpty($ConnectionString)) {
        if ([string]::IsNullOrEmpty($DatabasePassword)) {
            $DatabasePassword = Read-Host "Enter database password" -AsSecureString
            $DatabasePassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DatabasePassword))
        }
        
        $ConnectionString = "postgresql://$DatabaseUser`:$DatabasePassword@$DatabaseHost`:$DatabasePort/$DatabaseName"
    }
    
    # Step 1: Check prerequisites
    Write-Host "STEP 1: Prerequisites Check" -ForegroundColor Cyan
    if (!(Test-Prerequisites)) {
        throw "Prerequisites check failed. Please install required tools and ensure scripts are available."
    }
    Write-Host ""
    
    # Step 2: Test database connection
    Write-Host "STEP 2: Database Connection Test" -ForegroundColor Cyan
    if (!(Test-DatabaseConnection -ConnectionString $ConnectionString)) {
        throw "Database connection test failed. Please check connection parameters."
    }
    Write-Host ""
    
    # Step 3: Create backup
    $backupFile = $null
    if (!$SkipBackup) {
        Write-Host "STEP 3: Database Backup" -ForegroundColor Cyan
        $backupPath = Join-Path $PSScriptRoot "backups"
        $backupFile = Create-DatabaseBackup -ConnectionString $ConnectionString -BackupPath $backupPath
        if ($null -eq $backupFile -and !$DryRun) {
            $continue = Read-Host "Backup failed. Continue without backup? (y/N)"
            if ($continue -ne "y" -and $continue -ne "Y") {
                throw "Migration aborted due to backup failure."
            }
        }
        Write-Host ""
    }
    
    # Step 4: Execute migration scripts
    Write-Host "STEP 4: Migration Execution" -ForegroundColor Cyan
    
    $migrationScripts = @(
        @{
            Path = "01_populate_reference_tables.sql"
            Description = "Populate Reference Tables"
        },
        @{
            Path = "02_migrate_produto_references.sql"
            Description = "Migrate Produto References"
        },
        @{
            Path = "03_migrate_fornecedor_references.sql"
            Description = "Migrate Fornecedor References"
        }
    )
    
    $migrationSuccess = $true
    foreach ($migration in $migrationScripts) {
        $scriptPath = Join-Path $PSScriptRoot $migration.Path
        $success = Execute-SqlScript -ScriptPath $scriptPath -ConnectionString $ConnectionString -Description $migration.Description
        
        if (!$success) {
            $migrationSuccess = $false
            Write-Host "Migration failed at step: $($migration.Description)" -ForegroundColor Red
            break
        }
        
        Start-Sleep -Seconds 1  # Brief pause between migrations
    }
    
    if (!$migrationSuccess) {
        throw "Migration execution failed. Check logs for details."
    }
    Write-Host ""
    
    # Step 5: Validation
    if (!$SkipValidation) {
        Write-Host "STEP 5: Post-Migration Validation" -ForegroundColor Cyan
        
        $validationScripts = @(
            @{
                Path = "validation\validate_all_references.sql"
                Description = "Validate All References"
            },
            @{
                Path = "validation\check_data_quality.sql"
                Description = "Check Data Quality"
            }
        )
        
        foreach ($validation in $validationScripts) {
            $scriptPath = Join-Path $PSScriptRoot $validation.Path
            $success = Execute-SqlScript -ScriptPath $scriptPath -ConnectionString $ConnectionString -Description $validation.Description
            
            if (!$success) {
                Write-Host "⚠️ Validation failed: $($validation.Description)" -ForegroundColor Yellow
                Write-Host "Migration completed but validation issues found." -ForegroundColor Yellow
            }
        }
        Write-Host ""
    }
    
    # Step 6: Generate final report
    Write-Host "STEP 6: Generate Migration Report" -ForegroundColor Cyan
    $reportScript = Join-Path $PSScriptRoot "validation\generate_migration_report.sql"
    Execute-SqlScript -ScriptPath $reportScript -ConnectionString $ConnectionString -Description "Generate Migration Report"
    Write-Host ""
    
    # Success message
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "MIGRATION COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    
    if ($backupFile) {
        Write-Host "Backup created: $backupFile" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Review migration logs in the database" -ForegroundColor White
    Write-Host "2. Test application functionality" -ForegroundColor White
    Write-Host "3. Update application code to use new references" -ForegroundColor White
    Write-Host "4. Deploy frontend changes" -ForegroundColor White
    Write-Host ""
    Write-Host "Query migration logs:" -ForegroundColor Gray
    Write-Host "SELECT * FROM public.\"MigrationLogs\" ORDER BY \"ExecutionTime\" DESC;" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Query validation results:" -ForegroundColor Gray
    Write-Host "SELECT * FROM public.\"ValidationResults\" ORDER BY \"ExecutionTime\" DESC;" -ForegroundColor Gray
    
} catch {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "MIGRATION FAILED!" -ForegroundColor Red
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    
    if ($backupFile) {
        Write-Host ""
        Write-Host "Backup available for rollback: $backupFile" -ForegroundColor Yellow
        Write-Host "To rollback, run:" -ForegroundColor Yellow
        Write-Host "psql `"$ConnectionString`" -f `"$backupFile`"" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "For manual rollback, use:" -ForegroundColor Yellow
    Write-Host ".\rollback_all_migrations.ps1" -ForegroundColor Gray
    
    exit 1
}

# Clean up sensitive variables
if ($DatabasePassword) {
    Remove-Variable DatabasePassword -Force -ErrorAction SilentlyContinue
}
if ($ConnectionString) {
    Remove-Variable ConnectionString -Force -ErrorAction SilentlyContinue
}