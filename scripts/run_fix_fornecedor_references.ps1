# Script PowerShell para executar o fix das referências do Fornecedor
# Execute este script a partir da pasta nova_api

param(
    [string]$ConnectionString = "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432"
)

Write-Host "Executando script de correção das referências do Fornecedor..." -ForegroundColor Green

try {
    # Verificar se o psql está disponível
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        Write-Host "ERRO: psql não encontrado. Instale o PostgreSQL client." -ForegroundColor Red
        exit 1
    }

    # Executar o script SQL
    $scriptPath = Join-Path $PSScriptRoot "fix_fornecedor_references.sql"
    
    Write-Host "Executando script SQL: $scriptPath" -ForegroundColor Yellow
    
    # Converter connection string para formato psql
    $connParts = $ConnectionString -split ";"
    $host = ($connParts | Where-Object { $_ -like "Host=*" }) -replace "Host=", ""
    $database = ($connParts | Where-Object { $_ -like "Database=*" }) -replace "Database=", ""
    $username = ($connParts | Where-Object { $_ -like "Username=*" }) -replace "Username=", ""
    $password = ($connParts | Where-Object { $_ -like "Password=*" }) -replace "Password=", ""
    $port = ($connParts | Where-Object { $_ -like "Port=*" }) -replace "Port=", ""
    
    # Definir variável de ambiente para senha
    $env:PGPASSWORD = $password
    
    # Executar psql
    & psql -h $host -p $port -U $username -d $database -f $scriptPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Script executado com sucesso!" -ForegroundColor Green
        Write-Host "Agora você pode testar a API novamente." -ForegroundColor Cyan
    } else {
        Write-Host "ERRO: Falha ao executar o script SQL." -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de ambiente da senha
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "Concluído!" -ForegroundColor Green