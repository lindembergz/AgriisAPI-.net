# Script para criar migration dos novos campos do Fornecedor
# NomeFantasia, RamosAtividade, EnderecoCorrespondencia

Write-Host "=== Criando Migration para Novos Campos do Fornecedor ===" -ForegroundColor Green

# Navegar para o diretório da API
$apiPath = "nova_api/src/Agriis.Api"
if (-not (Test-Path $apiPath)) {
    Write-Host "Erro: Diretório da API não encontrado: $apiPath" -ForegroundColor Red
    exit 1
}

Set-Location $apiPath

Write-Host "Diretório atual: $(Get-Location)" -ForegroundColor Yellow

# Nome da migration
$migrationName = "AdicionarCamposAdicionaisFornecedor"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"

Write-Host "Criando migration: $migrationName" -ForegroundColor Cyan

try {
    # Criar a migration
    dotnet ef migrations add $migrationName --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration criada com sucesso!" -ForegroundColor Green
        
        # Listar as migrations para confirmar
        Write-Host "`n=== Migrations Disponíveis ===" -ForegroundColor Yellow
        dotnet ef migrations list
        
        Write-Host "`n=== Próximos Passos ===" -ForegroundColor Cyan
        Write-Host "1. Revisar a migration gerada em: Migrations/" -ForegroundColor White
        Write-Host "2. Executar: dotnet ef database update" -ForegroundColor White
        Write-Host "3. Testar a aplicação" -ForegroundColor White
        
    } else {
        Write-Host "❌ Erro ao criar migration" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Erro durante execução: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Script Concluído ===" -ForegroundColor Green