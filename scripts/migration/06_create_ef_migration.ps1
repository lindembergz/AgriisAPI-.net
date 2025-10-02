# =====================================================
# Geographic Tables Unification - Entity Framework Migration
# =====================================================
# This script creates the Entity Framework migration for the unified schema
# =====================================================

Write-Host "Creating Entity Framework migration for geographic tables unification..." -ForegroundColor Green

# Set the working directory to the API project
$apiProjectPath = "src/Agriis.Api"
Set-Location $apiProjectPath

try {
    # Create the migration
    Write-Host "Creating migration 'UnifyGeographicTables'..." -ForegroundColor Yellow
    dotnet ef migrations add UnifyGeographicTables --context AgriisDbContext --output-dir Migrations

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration created successfully!" -ForegroundColor Green
        
        # Display the migration files created
        Write-Host "`nMigration files created:" -ForegroundColor Cyan
        Get-ChildItem "Migrations" -Filter "*UnifyGeographicTables*" | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor White
        }
        
        Write-Host "`nNext steps:" -ForegroundColor Yellow
        Write-Host "1. Review the generated migration files" -ForegroundColor White
        Write-Host "2. Run the database migration scripts first (01-05)" -ForegroundColor White
        Write-Host "3. Then apply the EF migration: dotnet ef database update" -ForegroundColor White
        
    } else {
        Write-Host "Failed to create migration. Check the error messages above." -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "Error creating migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nEntity Framework migration creation completed!" -ForegroundColor Green