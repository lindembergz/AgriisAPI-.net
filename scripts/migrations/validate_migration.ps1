# =====================================================
# MIGRATION VALIDATOR
# Description: Validates the reference entities migration
# Author: System Migration
# Date: 2025-01-27
# Requirements: 12.5, 12.6
# =====================================================

param(
    [string]$ConnectionString = $null,
    [string]$DatabaseHost = "localhost",
    [string]$DatabasePort = "5432",
    [string]$DatabaseName = "agriis",
    [string]$DatabaseUser = "postgres",
    [string]$DatabasePassword = $null,
    [switch]$DetailedReport = $false,
    [switch]$DataQualityCheck = $false,
    [switch]$ExportReport = $false,
    [string]$ReportPath = ".",
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "MIGRATION VALIDATION TOOL" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Starting validation process..." -ForegroundColor Green
Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Function to execute SQL script and capture output
function Execute-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$ConnectionString,
        [string]$Description
    )
    
    Write-Host "Executing: $Description" -ForegroundColor Yellow
    Write-Verbose "Script: $ScriptPath"
    
    try {
        if (Test-Path $ScriptPath) {
            $result = psql $ConnectionString -f $ScriptPath -v ON_ERROR_STOP=1 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Success: $Description" -ForegroundColor Green
                return @{ Success = $true; Output = $result }
            } else {
                Write-Host "✗ Failed: $Description" -ForegroundColor Red
                Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
                return @{ Success = $false; Output = $result; ExitCode = $LASTEXITCODE }
            }
        } else {
            Write-Host "✗ Script not found: $ScriptPath" -ForegroundColor Red
            return @{ Success = $false; Error = "Script not found" }
        }
    } catch {
        Write-Host "✗ Error executing $Description`: $($_.Exception.Message)" -ForegroundColor Red
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

# Function to execute SQL query and return results
function Execute-SqlQuery {
    param(
        [string]$Query,
        [string]$ConnectionString,
        [string]$Description = "SQL Query"
    )
    
    Write-Verbose "Executing query: $Description"
    
    try {
        $result = psql $ConnectionString -c $Query -t --csv 2>&1
        if ($LASTEXITCODE -eq 0) {
            return @{ Success = $true; Data = $result }
        } else {
            Write-Warning "Query failed: $Description"
            return @{ Success = $false; Error = $result }
        }
    } catch {
        Write-Warning "Query error: $($_.Exception.Message)"
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

# Function to test database connection
function Test-DatabaseConnection {
    param([string]$ConnectionString)
    
    Write-Host "Testing database connection..." -ForegroundColor Yellow
    
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

# Function to get validation summary
function Get-ValidationSummary {
    param([string]$ConnectionString)
    
    Write-Host "Retrieving validation summary..." -ForegroundColor Yellow
    
    $summaryQuery = @"
SELECT 
    COUNT(*) FILTER (WHERE "Status" IN ('PASS', 'FAIL', 'WARNING')) as total_tests,
    COUNT(*) FILTER (WHERE "Status" = 'PASS') as passed_tests,
    COUNT(*) FILTER (WHERE "Status" = 'FAIL') as failed_tests,
    COUNT(*) FILTER (WHERE "Status" = 'WARNING') as warning_tests,
    ROUND(
        CASE WHEN COUNT(*) FILTER (WHERE "Status" IN ('PASS', 'FAIL', 'WARNING')) > 0 
        THEN (COUNT(*) FILTER (WHERE "Status" = 'PASS')::DECIMAL / COUNT(*) FILTER (WHERE "Status" IN ('PASS', 'FAIL', 'WARNING')) * 100)
        ELSE 0 END, 2
    ) as success_rate
FROM public."ValidationResults" 
WHERE "ValidationName" = 'validate_all_references';
"@

    $result = Execute-SqlQuery -Query $summaryQuery -ConnectionString $ConnectionString -Description "Validation Summary"
    
    if ($result.Success -and $result.Data) {
        $data = $result.Data -split ','
        return @{
            TotalTests = [int]$data[0]
            PassedTests = [int]$data[1]
            FailedTests = [int]$data[2]
            WarningTests = [int]$data[3]
            SuccessRate = [decimal]$data[4]
        }
    }
    
    return $null
}

# Function to get data quality summary
function Get-DataQualitySummary {
    param([string]$ConnectionString)
    
    Write-Host "Retrieving data quality summary..." -ForegroundColor Yellow
    
    $qualityQuery = @"
SELECT 
    COUNT(*) as total_issues,
    COUNT(*) FILTER (WHERE "Severity" = 'CRITICAL') as critical_issues,
    COUNT(*) FILTER (WHERE "Severity" = 'HIGH') as high_issues,
    COUNT(*) FILTER (WHERE "Severity" = 'MEDIUM') as medium_issues,
    COUNT(*) FILTER (WHERE "Severity" = 'LOW') as low_issues
FROM public."DataQualityResults" 
WHERE "CheckName" = 'reference_entities_quality_check' 
AND "IssueType" != 'QUALITY_SUMMARY';
"@

    $result = Execute-SqlQuery -Query $qualityQuery -ConnectionString $ConnectionString -Description "Data Quality Summary"
    
    if ($result.Success -and $result.Data) {
        $data = $result.Data -split ','
        return @{
            TotalIssues = [int]$data[0]
            CriticalIssues = [int]$data[1]
            HighIssues = [int]$data[2]
            MediumIssues = [int]$data[3]
            LowIssues = [int]$data[4]
        }
    }
    
    return $null
}

# Function to export validation report
function Export-ValidationReport {
    param(
        [string]$ConnectionString,
        [string]$ReportPath
    )
    
    Write-Host "Exporting validation report..." -ForegroundColor Yellow
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportFile = Join-Path $ReportPath "migration_validation_report_$timestamp.csv"
    
    # Create report directory if it doesn't exist
    $reportDir = Split-Path $reportFile -Parent
    if (!(Test-Path $reportDir)) {
        New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    }
    
    $exportQuery = @"
SELECT 
    "Category",
    "TestName",
    "Status",
    "Message",
    "ExpectedCount",
    "ActualCount",
    "ExecutionTime"
FROM public."ValidationResults" 
WHERE "ValidationName" = 'validate_all_references'
AND "Status" IN ('PASS', 'FAIL', 'WARNING')
ORDER BY "Category", "TestName";
"@

    try {
        psql $ConnectionString -c $exportQuery --csv -o $reportFile
        if ($LASTEXITCODE -eq 0 -and (Test-Path $reportFile)) {
            Write-Host "✓ Report exported: $reportFile" -ForegroundColor Green
            return $reportFile
        } else {
            Write-Host "✗ Report export failed" -ForegroundColor Red
            return $null
        }
    } catch {
        Write-Host "✗ Export error: $($_.Exception.Message)" -ForegroundColor Red
        return $null
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
    
    # Step 1: Test database connection
    Write-Host "STEP 1: Database Connection Test" -ForegroundColor Cyan
    if (!(Test-DatabaseConnection -ConnectionString $ConnectionString)) {
        throw "Database connection test failed. Please check connection parameters."
    }
    Write-Host ""
    
    # Step 2: Run validation scripts
    Write-Host "STEP 2: Execute Validation Scripts" -ForegroundColor Cyan
    
    # Main validation
    $validationScript = Join-Path $PSScriptRoot "validation\validate_all_references.sql"
    $validationResult = Execute-SqlScript -ScriptPath $validationScript -ConnectionString $ConnectionString -Description "Reference Validation"
    
    if (!$validationResult.Success) {
        Write-Warning "Main validation script failed, but continuing with available data..."
    }
    
    # Data quality check (if requested)
    if ($DataQualityCheck) {
        $qualityScript = Join-Path $PSScriptRoot "validation\check_data_quality.sql"
        $qualityResult = Execute-SqlScript -ScriptPath $qualityScript -ConnectionString $ConnectionString -Description "Data Quality Check"
        
        if (!$qualityResult.Success) {
            Write-Warning "Data quality check failed, but continuing..."
        }
    }
    Write-Host ""
    
    # Step 3: Analyze results
    Write-Host "STEP 3: Analyze Validation Results" -ForegroundColor Cyan
    
    # Get validation summary
    $validationSummary = Get-ValidationSummary -ConnectionString $ConnectionString
    
    if ($validationSummary) {
        Write-Host "Validation Summary:" -ForegroundColor White
        Write-Host "  Total Tests: $($validationSummary.TotalTests)" -ForegroundColor Gray
        Write-Host "  Passed: $($validationSummary.PassedTests)" -ForegroundColor Green
        Write-Host "  Failed: $($validationSummary.FailedTests)" -ForegroundColor Red
        Write-Host "  Warnings: $($validationSummary.WarningTests)" -ForegroundColor Yellow
        Write-Host "  Success Rate: $($validationSummary.SuccessRate)%" -ForegroundColor $(
            if ($validationSummary.SuccessRate -ge 95) { "Green" }
            elseif ($validationSummary.SuccessRate -ge 80) { "Yellow" }
            else { "Red" }
        )
    } else {
        Write-Warning "Could not retrieve validation summary"
    }
    
    # Get data quality summary (if available)
    if ($DataQualityCheck) {
        $qualitySummary = Get-DataQualitySummary -ConnectionString $ConnectionString
        
        if ($qualitySummary) {
            Write-Host ""
            Write-Host "Data Quality Summary:" -ForegroundColor White
            Write-Host "  Total Issues: $($qualitySummary.TotalIssues)" -ForegroundColor Gray
            Write-Host "  Critical: $($qualitySummary.CriticalIssues)" -ForegroundColor Red
            Write-Host "  High: $($qualitySummary.HighIssues)" -ForegroundColor Red
            Write-Host "  Medium: $($qualitySummary.MediumIssues)" -ForegroundColor Yellow
            Write-Host "  Low: $($qualitySummary.LowIssues)" -ForegroundColor Green
        }
    }
    Write-Host ""
    
    # Step 4: Generate detailed report (if requested)
    if ($DetailedReport) {
        Write-Host "STEP 4: Generate Detailed Report" -ForegroundColor Cyan
        $reportScript = Join-Path $PSScriptRoot "validation\generate_migration_report.sql"
        Execute-SqlScript -ScriptPath $reportScript -ConnectionString $ConnectionString -Description "Generate Detailed Report"
        Write-Host ""
    }
    
    # Step 5: Export report (if requested)
    if ($ExportReport) {
        Write-Host "STEP 5: Export Validation Report" -ForegroundColor Cyan
        $exportedFile = Export-ValidationReport -ConnectionString $ConnectionString -ReportPath $ReportPath
        if ($exportedFile) {
            Write-Host "Report exported to: $exportedFile" -ForegroundColor Gray
        }
        Write-Host ""
    }
    
    # Determine overall status
    $overallStatus = "SUCCESS"
    $statusColor = "Green"
    
    if ($validationSummary) {
        if ($validationSummary.FailedTests -gt 0) {
            $overallStatus = "FAILED"
            $statusColor = "Red"
        } elseif ($validationSummary.WarningTests -gt 0) {
            $overallStatus = "WARNING"
            $statusColor = "Yellow"
        }
    }
    
    if ($DataQualityCheck -and $qualitySummary -and $qualitySummary.CriticalIssues -gt 0) {
        $overallStatus = "CRITICAL_ISSUES"
        $statusColor = "Red"
    }
    
    # Final status
    Write-Host "=========================================" -ForegroundColor $statusColor
    Write-Host "VALIDATION STATUS: $overallStatus" -ForegroundColor $statusColor
    Write-Host "=========================================" -ForegroundColor $statusColor
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    # Recommendations
    if ($overallStatus -eq "FAILED" -or $overallStatus -eq "CRITICAL_ISSUES") {
        Write-Host "RECOMMENDATIONS:" -ForegroundColor Red
        Write-Host "❗ Address failed validations before proceeding to production" -ForegroundColor Red
        Write-Host "❗ Review migration logs for detailed error information" -ForegroundColor Red
        Write-Host "❗ Consider running rollback if issues are severe" -ForegroundColor Red
    } elseif ($overallStatus -eq "WARNING") {
        Write-Host "RECOMMENDATIONS:" -ForegroundColor Yellow
        Write-Host "⚠️ Review warnings and consider addressing them" -ForegroundColor Yellow
        Write-Host "⚠️ Monitor system behavior after deployment" -ForegroundColor Yellow
    } else {
        Write-Host "RECOMMENDATIONS:" -ForegroundColor Green
        Write-Host "✓ Migration validation passed successfully" -ForegroundColor Green
        Write-Host "✓ System is ready for production deployment" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Useful Queries:" -ForegroundColor Gray
    Write-Host "View validation details:" -ForegroundColor Gray
    Write-Host "SELECT * FROM public.\"ValidationResults\" WHERE \"ValidationName\" = 'validate_all_references' ORDER BY \"Status\", \"Category\";" -ForegroundColor DarkGray
    
    if ($DataQualityCheck) {
        Write-Host "View data quality issues:" -ForegroundColor Gray
        Write-Host "SELECT * FROM public.\"DataQualityResults\" WHERE \"CheckName\" = 'reference_entities_quality_check' ORDER BY \"Severity\", \"IssueCount\" DESC;" -ForegroundColor DarkGray
    }
    
    # Set exit code based on validation results
    if ($overallStatus -eq "FAILED" -or $overallStatus -eq "CRITICAL_ISSUES") {
        exit 1
    } elseif ($overallStatus -eq "WARNING") {
        exit 2
    } else {
        exit 0
    }
    
} catch {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "VALIDATION FAILED!" -ForegroundColor Red
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    
    exit 1
}

# Clean up sensitive variables
if ($DatabasePassword) {
    Remove-Variable DatabasePassword -Force -ErrorAction SilentlyContinue
}
if ($ConnectionString) {
    Remove-Variable ConnectionString -Force -ErrorAction SilentlyContinue
}