# Script para criar uma migração do Entity Framework para a tabela ProdutosCulturas
# Execute este script na pasta nova_api

Write-Host "=== Criando Migração para ProdutosCulturas ===" -ForegroundColor Green

# Verifica se estamos na pasta correta
if (-not (Test-Path "Agriis.sln")) {
    Write-Host "ERRO: Execute este script na pasta nova_api (onde está o Agriis.sln)" -ForegroundColor Red
    exit 1
}

# Verifica se o dotnet está disponível
try {
    $null = Get-Command dotnet -ErrorAction Stop
    Write-Host "✅ .NET CLI encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ ERRO: dotnet CLI não encontrado!" -ForegroundColor Red
    exit 1
}

# Verifica se o Entity Framework tools está instalado
Write-Host "Verificando Entity Framework tools..." -ForegroundColor Yellow
$efTools = dotnet tool list --global | Select-String "dotnet-ef"

if (-not $efTools) {
    Write-Host "Entity Framework tools não encontrado. Instalando..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Erro ao instalar Entity Framework tools" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Entity Framework tools instalado" -ForegroundColor Green
} else {
    Write-Host "✅ Entity Framework tools já instalado" -ForegroundColor Green
}

# Cria a migração
Write-Host "Criando migração para ProdutosCulturas..." -ForegroundColor Yellow

try {
    dotnet ef migrations add CriarTabelaProdutosCulturas --project src/Agriis.Api --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migração criada com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Próximos passos:" -ForegroundColor Yellow
        Write-Host "1. Revise a migração gerada em src/Agriis.Api/Migrations/" -ForegroundColor White
        Write-Host "2. Execute: dotnet ef database update --project src/Agriis.Api" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "❌ Erro ao criar a migração" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao executar dotnet ef: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "=== Fim da Criação da Migração ===" -ForegroundColor Green