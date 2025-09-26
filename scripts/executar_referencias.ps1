# Script PowerShell para criar tabelas de referências
Write-Host "========================================" -ForegroundColor Green
Write-Host " Criando Tabelas de Referencias" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Configurações do banco de dados
$DB_HOST = "localhost"
$DB_PORT = "5432"
$DB_NAME = "DBAgriis"
$DB_USER = "postgres"
$DB_PASSWORD = "RootPassword123"

Write-Host "Executando script SQL..." -ForegroundColor Yellow
Write-Host "Host: $DB_HOST`:$DB_PORT" -ForegroundColor Cyan
Write-Host "Database: $DB_NAME" -ForegroundColor Cyan
Write-Host "User: $DB_USER" -ForegroundColor Cyan
Write-Host ""

try {
    # Definir variável de ambiente para senha (evita prompt)
    $env:PGPASSWORD = $DB_PASSWORD
    
    # Executar o script SQL
    $scriptPath = Join-Path $PSScriptRoot "criar_tabelas_referencias.sql"
    
    if (Test-Path $scriptPath) {
        psql -h $DB_HOST -p $DB_PORT -d $DB_NAME -U $DB_USER -f $scriptPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Green
            Write-Host " Script executado com sucesso!" -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Red
            Write-Host " Erro ao executar o script!" -ForegroundColor Red
            Write-Host " Codigo de erro: $LASTEXITCODE" -ForegroundColor Red
            Write-Host "========================================" -ForegroundColor Red
        }
    } else {
        Write-Host "Arquivo SQL não encontrado: $scriptPath" -ForegroundColor Red
    }
} catch {
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Limpar variável de ambiente da senha
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host ""
Read-Host "Pressione Enter para continuar"