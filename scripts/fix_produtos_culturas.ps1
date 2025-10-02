# Script PowerShell para corrigir a tabela ProdutosCulturas faltante
# Execute este script na pasta nova_api

param(
    [string]$ConnectionString = $null
)

Write-Host "=== Correção da Tabela ProdutosCulturas ===" -ForegroundColor Green

# Se não foi fornecida uma connection string, tenta obter do appsettings
if (-not $ConnectionString) {
    Write-Host "Tentando obter connection string do appsettings.json..." -ForegroundColor Yellow
    
    $appsettingsPath = "src/Agriis.Api/appsettings.json"
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        $ConnectionString = $appsettings.ConnectionStrings.DefaultConnection
        Write-Host "Connection string encontrada no appsettings.json" -ForegroundColor Green
    } else {
        Write-Host "Arquivo appsettings.json não encontrado!" -ForegroundColor Red
        Write-Host "Por favor, forneça a connection string como parâmetro:" -ForegroundColor Yellow
        Write-Host ".\scripts\fix_produtos_culturas.ps1 -ConnectionString 'sua_connection_string'" -ForegroundColor Yellow
        exit 1
    }
}

# Verifica se o psql está disponível
try {
    $null = Get-Command psql -ErrorAction Stop
    Write-Host "PostgreSQL psql encontrado" -ForegroundColor Green
} catch {
    Write-Host "ERRO: psql não encontrado no PATH!" -ForegroundColor Red
    Write-Host "Instale o PostgreSQL client ou adicione ao PATH" -ForegroundColor Yellow
    exit 1
}

# Executa o script SQL
$sqlScript = "scripts/create_produtos_culturas_table.sql"

if (-not (Test-Path $sqlScript)) {
    Write-Host "ERRO: Script SQL não encontrado: $sqlScript" -ForegroundColor Red
    exit 1
}

Write-Host "Executando script SQL..." -ForegroundColor Yellow

try {
    # Executa o script usando psql
    $env:PGPASSWORD = ($ConnectionString -split "Password=")[1] -split ";")[0]
    $result = psql $ConnectionString -f $sqlScript -v ON_ERROR_STOP=1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tabela ProdutosCulturas criada com sucesso!" -ForegroundColor Green
        Write-Host "Agora você pode executar a aplicação novamente." -ForegroundColor Green
    } else {
        Write-Host "❌ Erro ao executar o script SQL" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao executar psql: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Tente executar manualmente:" -ForegroundColor Yellow
    Write-Host "psql 'sua_connection_string' -f $sqlScript" -ForegroundColor Yellow
}

Write-Host "=== Fim da Correção ===" -ForegroundColor Green