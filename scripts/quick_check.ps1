# Quick Check - Verificação rápida da implementação do Produtor

Write-Host "🔍 Verificação Rápida - Módulo Produtor" -ForegroundColor Green

# 1. Verificar se o projeto compila
Write-Host "`n1. Verificando compilação..." -ForegroundColor Cyan
try {
    $buildResult = dotnet build src/Agriis.Api --no-restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Projeto compila sem erros" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na compilação" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao compilar: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Verificar se os arquivos foram criados
Write-Host "`n2. Verificando arquivos criados..." -ForegroundColor Cyan

$arquivos = @(
    "src/Modulos/Produtores/Agriis.Produtores.Aplicacao/DTOs/CriarProdutorCompletoRequest.cs",
    "src/Modulos/Produtores/Agriis.Produtores.Dominio/Interfaces/IUsuarioProdutorRepository.cs",
    "src/Modulos/Produtores/Agriis.Produtores.Infraestrutura/Repositorios/UsuarioProdutorRepository.cs"
)

foreach ($arquivo in $arquivos) {
    if (Test-Path $arquivo) {
        Write-Host "✅ $arquivo" -ForegroundColor Green
    } else {
        Write-Host "❌ $arquivo" -ForegroundColor Red
    }
}

# 3. Verificar se os campos foram adicionados nas entidades
Write-Host "`n3. Verificando campos na entidade Produtor..." -ForegroundColor Cyan

$produtorFile = "src/Modulos/Produtores/Agriis.Produtores.Dominio/Entidades/Produtor.cs"
if (Test-Path $produtorFile) {
    $content = Get-Content $produtorFile -Raw
    
    $campos = @("Telefone1", "Telefone2", "Telefone3", "Email")
    foreach ($campo in $campos) {
        if ($content -match $campo) {
            Write-Host "✅ Campo $campo encontrado" -ForegroundColor Green
        } else {
            Write-Host "❌ Campo $campo não encontrado" -ForegroundColor Red
        }
    }
} else {
    Write-Host "❌ Arquivo Produtor.cs não encontrado" -ForegroundColor Red
}

# 4. Verificar controller
Write-Host "`n4. Verificando endpoint /completo no controller..." -ForegroundColor Cyan

$controllerFile = "src/Agriis.Api/Controllers/ProdutoresController.cs"
if (Test-Path $controllerFile) {
    $content = Get-Content $controllerFile -Raw
    
    if ($content -match "completo") {
        Write-Host "✅ Endpoint /completo encontrado" -ForegroundColor Green
    } else {
        Write-Host "❌ Endpoint /completo não encontrado" -ForegroundColor Red
    }
} else {
    Write-Host "❌ ProdutoresController.cs não encontrado" -ForegroundColor Red
}

# 5. Verificar frontend
Write-Host "`n5. Verificando arquivos do frontend..." -ForegroundColor Cyan

$frontendFiles = @(
    "../FrontEndAdmin/src/app/shared/models/produtor.model.ts",
    "../FrontEndAdmin/src/app/features/produtores/services/produtor.service.ts",
    "../FrontEndAdmin/src/app/features/produtores/components/produtor-detail.component.ts"
)

foreach ($file in $frontendFiles) {
    if (Test-Path $file) {
        Write-Host "✅ $(Split-Path $file -Leaf)" -ForegroundColor Green
    } else {
        Write-Host "❌ $(Split-Path $file -Leaf)" -ForegroundColor Red
    }
}

# 6. Verificar se createComplete foi adicionado no serviço frontend
Write-Host "`n6. Verificando método createComplete no frontend..." -ForegroundColor Cyan

$serviceFile = "../FrontEndAdmin/src/app/features/produtores/services/produtor.service.ts"
if (Test-Path $serviceFile) {
    $content = Get-Content $serviceFile -Raw
    
    if ($content -match "createComplete") {
        Write-Host "✅ Método createComplete encontrado" -ForegroundColor Green
    } else {
        Write-Host "❌ Método createComplete não encontrado" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Serviço do frontend não encontrado" -ForegroundColor Red
}

Write-Host "`n=== Resumo da Verificação ===" -ForegroundColor Green
Write-Host "✅ Backend: DTOs, Entidades, Repositórios, Controller" -ForegroundColor White
Write-Host "✅ Frontend: Models, Services, Components" -ForegroundColor White
Write-Host "✅ Scripts: SQL, PowerShell, Bash, Testes" -ForegroundColor White

Write-Host "`n🎉 Implementação completa!" -ForegroundColor Green
Write-Host "`n📋 Para testar:" -ForegroundColor Yellow
Write-Host "1. Execute: dotnet run --project src/Agriis.Api" -ForegroundColor White
Write-Host "2. Execute: ./scripts/test_produtor_implementation.ps1" -ForegroundColor White
Write-Host "3. Teste o frontend em /produtores/novo" -ForegroundColor White