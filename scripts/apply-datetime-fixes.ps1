# Script PowerShell para aplicar todas as correções de DateTimeOffset
# Execução: .\apply-datetime-fixes.ps1

Write-Host "=== Aplicando Correções de DateTimeOffset ===" -ForegroundColor Green

# 1. Executar correção do banco de dados
Write-Host "1. Executando correção do banco de dados..." -ForegroundColor Yellow
try {
    # Assumindo que as variáveis de conexão estão configuradas
    $connectionString = $env:AGRIIS_CONNECTION_STRING
    if (-not $connectionString) {
        $connectionString = "Host=localhost;Database=agriis_db;Username=postgres;Password=postgres"
        Write-Host "Usando connection string padrão. Configure AGRIIS_CONNECTION_STRING se necessário." -ForegroundColor Yellow
    }
    
    # Executar script SQL
    psql $connectionString -f "fix-datetime-columns-to-timestamptz.sql"
    Write-Host "✅ Correção do banco aplicada com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao aplicar correção do banco: $_" -ForegroundColor Red
    exit 1
}

# 2. Compilar o projeto para verificar se não há erros
Write-Host "2. Compilando projeto para verificar correções..." -ForegroundColor Yellow
try {
    Set-Location "../"
    dotnet build --no-restore
    Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro na compilação: $_" -ForegroundColor Red
    exit 1
}

# 3. Executar testes de validação
Write-Host "3. Executando testes de validação..." -ForegroundColor Yellow
try {
    Set-Location "scripts"
    psql $connectionString -f "test-datetime-fixes.sql"
    psql $connectionString -f "test-produtores-produtos-datetime.sql"
    Write-Host "✅ Testes de validação executados!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro nos testes: $_" -ForegroundColor Red
}

# 4. Gerar nova migração se necessário
Write-Host "4. Verificando se nova migração é necessária..." -ForegroundColor Yellow
try {
    Set-Location "../"
    $migrationCheck = dotnet ef migrations has-pending-model-changes --project src/Agriis.Api
    if ($migrationCheck -match "changes") {
        Write-Host "Gerando nova migração..." -ForegroundColor Yellow
        $migrationName = "FixDateTimeOffsetColumns_$(Get-Date -Format 'yyyyMMddHHmmss')"
        dotnet ef migrations add $migrationName --project src/Agriis.Api
        Write-Host "✅ Migração $migrationName criada!" -ForegroundColor Green
    } else {
        Write-Host "✅ Nenhuma migração adicional necessária!" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️ Aviso: Não foi possível verificar/gerar migrações: $_" -ForegroundColor Yellow
}

Write-Host "=== Correções Aplicadas com Sucesso! ===" -ForegroundColor Green
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Teste a API acessando os endpoints de Fornecedor, Produtor e Produto" -ForegroundColor White
Write-Host "2. Verifique se não há mais erros de DateTimeOffset" -ForegroundColor White
Write-Host "3. Execute testes automatizados se disponíveis" -ForegroundColor White
Write-Host "4. Monitore logs para outros módulos que possam ter o mesmo problema" -ForegroundColor White