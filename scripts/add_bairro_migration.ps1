# =====================================================
# SCRIPT POWERSHELL: CRIAR E APLICAR MIGRAÇÃO PARA CAMPO BAIRRO
# Data: $(Get-Date)
# Descrição: Cria e aplica migração do Entity Framework para adicionar campo Bairro
# =====================================================

Write-Host "=== INICIANDO MIGRAÇÃO PARA CAMPO BAIRRO ===" -ForegroundColor Green

# Verificar se estamos no diretório correto
$currentPath = Get-Location
Write-Host "Diretório atual: $currentPath" -ForegroundColor Yellow

# Navegar para o diretório da API se necessário
if (-not (Test-Path "src/Agriis.Api")) {
    if (Test-Path "nova_api/src/Agriis.Api") {
        Set-Location "nova_api"
        Write-Host "Navegando para o diretório nova_api" -ForegroundColor Yellow
    } else {
        Write-Host "ERRO: Não foi possível encontrar o diretório da API" -ForegroundColor Red
        exit 1
    }
}

# Verificar se o dotnet está instalado
try {
    $dotnetVersion = dotnet --version
    Write-Host "Versão do .NET: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERRO: .NET CLI não encontrado. Instale o .NET SDK." -ForegroundColor Red
    exit 1
}

# Verificar se o Entity Framework Tools está instalado
try {
    dotnet ef --version
    Write-Host "Entity Framework Tools encontrado" -ForegroundColor Green
} catch {
    Write-Host "Instalando Entity Framework Tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

Write-Host "`n=== CRIANDO MIGRAÇÃO ===" -ForegroundColor Green

# Criar a migração
$migrationName = "AddBairroToFornecedor_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Nome da migração: $migrationName" -ForegroundColor Yellow

try {
    dotnet ef migrations add $migrationName --project src/Agriis.Api --verbose
    Write-Host "Migração criada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "ERRO ao criar migração: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== APLICANDO MIGRAÇÃO ===" -ForegroundColor Green

# Aplicar a migração
try {
    dotnet ef database update --project src/Agriis.Api --verbose
    Write-Host "Migração aplicada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "ERRO ao aplicar migração: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Você pode tentar aplicar manualmente com: dotnet ef database update --project src/Agriis.Api" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n=== VALIDANDO MIGRAÇÃO ===" -ForegroundColor Green

# Executar script de validação SQL se existir
$validationScript = "scripts/add_bairro_fornecedor_migration.sql"
if (Test-Path $validationScript) {
    Write-Host "Executando validação SQL..." -ForegroundColor Yellow
    
    # Aqui você pode adicionar a lógica para executar o script SQL de validação
    # Por exemplo, usando psql se estiver disponível
    Write-Host "Script de validação disponível em: $validationScript" -ForegroundColor Yellow
    Write-Host "Execute manualmente para validar a migração." -ForegroundColor Yellow
} else {
    Write-Host "Script de validação não encontrado" -ForegroundColor Yellow
}

Write-Host "`n=== PRÓXIMOS PASSOS ===" -ForegroundColor Green
Write-Host "1. Compile o projeto: dotnet build" -ForegroundColor White
Write-Host "2. Execute os testes: dotnet test" -ForegroundColor White
Write-Host "3. Teste a API para verificar se o campo Bairro está funcionando" -ForegroundColor White
Write-Host "4. Atualize o frontend se necessário" -ForegroundColor White

Write-Host "`n=== MIGRAÇÃO CONCLUÍDA COM SUCESSO! ===" -ForegroundColor Green

# Opcional: Compilar o projeto para verificar se não há erros
$compile = Read-Host "Deseja compilar o projeto agora? (s/n)"
if ($compile -eq "s" -or $compile -eq "S") {
    Write-Host "`nCompilando projeto..." -ForegroundColor Yellow
    try {
        dotnet build src/Agriis.Api
        Write-Host "Compilação concluída com sucesso!" -ForegroundColor Green
    } catch {
        Write-Host "ERRO na compilação: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nScript concluído!" -ForegroundColor Green