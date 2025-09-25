# Script de teste para validar a implementação do módulo Produtor
# Testa a API e verifica se os novos campos estão funcionando

param(
    [string]$ApiUrl = "https://localhost:7001",
    [string]$TestEmail = "produtor.teste@agriis.com",
    [string]$TestPassword = "Teste123!",
    [switch]$SkipHttpsValidation
)

Write-Host "=== Testando Implementação do Módulo Produtor ===" -ForegroundColor Green

# Desabilitar validação SSL se solicitado (apenas para desenvolvimento)
if ($SkipHttpsValidation) {
    Write-Host "⚠️  Desabilitando validação HTTPS (apenas para desenvolvimento)" -ForegroundColor Yellow
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $PSDefaultParameterValues['Invoke-RestMethod:SkipCertificateCheck'] = $true
        $PSDefaultParameterValues['Invoke-WebRequest:SkipCertificateCheck'] = $true
    }
}

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    $uri = "$ApiUrl$Endpoint"
    $params = @{
        Uri = $uri
        Method = $Method
        Headers = $Headers
        ContentType = "application/json"
    }
    
    if ($Body) {
        $params.Body = $Body | ConvertTo-Json -Depth 10
    }
    
    try {
        $response = Invoke-RestMethod @params
        return @{ Success = $true; Data = $response }
    } catch {
        $errorDetails = $_.Exception.Message
        if ($_.Exception.Response) {
            try {
                $errorStream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($errorStream)
                $errorBody = $reader.ReadToEnd()
                $errorDetails += " - Response: $errorBody"
            } catch {
                # Ignore errors reading response body
            }
        }
        return @{ Success = $false; Error = $errorDetails }
    }
}

# Teste 1: Verificar se a API está rodando
Write-Host "🔍 Teste 1: Verificando se a API está acessível..." -ForegroundColor Cyan
$healthCheck = Invoke-ApiRequest -Method "GET" -Endpoint "/"

if ($healthCheck.Success) {
    Write-Host "✅ API está acessível" -ForegroundColor Green
    Write-Host "   Resposta: $($healthCheck.Data | ConvertTo-Json -Compress)" -ForegroundColor Gray
} else {
    Write-Host "❌ API não está acessível: $($healthCheck.Error)" -ForegroundColor Red
    Write-Host "📋 Verifique se a API está rodando em: $ApiUrl" -ForegroundColor Yellow
    exit 1
}

# Teste 2: Listar produtores existentes
Write-Host ""
Write-Host "🔍 Teste 2: Listando produtores existentes..." -ForegroundColor Cyan
$listProdutores = Invoke-ApiRequest -Method "GET" -Endpoint "/api/produtores"

if ($listProdutores.Success) {
    Write-Host "✅ Endpoint de listagem funcionando" -ForegroundColor Green
    $totalProdutores = if ($listProdutores.Data.items) { $listProdutores.Data.items.Count } else { 0 }
    Write-Host "   Total de produtores: $totalProdutores" -ForegroundColor Gray
} else {
    Write-Host "❌ Erro ao listar produtores: $($listProdutores.Error)" -ForegroundColor Red
}

# Teste 3: Criar um produtor completo com os novos campos
Write-Host ""
Write-Host "🔍 Teste 3: Criando produtor completo com novos campos..." -ForegroundColor Cyan

$novoProdutorCompleto = @{
    nome = "Produtor Teste Completo"
    cpfCnpj = "12345678901"
    tipoCliente = "PF"
    telefone1 = "(11) 99999-1111"
    telefone2 = "(11) 99999-2222"
    telefone3 = "(11) 99999-3333"
    email = $TestEmail
    areaPlantio = 100.5
    culturas = @(1, 2)
    usuarioMaster = @{
        nome = "Usuario Master Teste"
        email = $TestEmail
        senha = $TestPassword
        telefone = "(11) 99999-0000"
        cpf = "98765432100"
    }
}

$createCompleto = Invoke-ApiRequest -Method "POST" -Endpoint "/api/produtores/completo" -Body $novoProdutorCompleto

if ($createCompleto.Success) {
    Write-Host "✅ Produtor completo criado com sucesso!" -ForegroundColor Green
    Write-Host "   ID: $($createCompleto.Data.id)" -ForegroundColor Gray
    Write-Host "   Nome: $($createCompleto.Data.nome)" -ForegroundColor Gray
    Write-Host "   Telefone1: $($createCompleto.Data.telefone1)" -ForegroundColor Gray
    Write-Host "   Telefone2: $($createCompleto.Data.telefone2)" -ForegroundColor Gray
    Write-Host "   Telefone3: $($createCompleto.Data.telefone3)" -ForegroundColor Gray
    Write-Host "   Email: $($createCompleto.Data.email)" -ForegroundColor Gray
    
    $produtorId = $createCompleto.Data.id
    
    # Teste 4: Buscar o produtor criado para verificar se os campos foram salvos
    Write-Host ""
    Write-Host "🔍 Teste 4: Verificando se os campos foram salvos corretamente..." -ForegroundColor Cyan
    
    $getProdutoById = Invoke-ApiRequest -Method "GET" -Endpoint "/api/produtores/$produtorId"
    
    if ($getProdutoById.Success) {
        Write-Host "✅ Produtor recuperado com sucesso!" -ForegroundColor Green
        
        # Verificar se os novos campos estão presentes
        $produtor = $getProdutoById.Data
        $camposOk = $true
        
        if (-not $produtor.telefone1) {
            Write-Host "❌ Campo telefone1 não encontrado ou vazio" -ForegroundColor Red
            $camposOk = $false
        }
        
        if (-not $produtor.telefone2) {
            Write-Host "❌ Campo telefone2 não encontrado ou vazio" -ForegroundColor Red
            $camposOk = $false
        }
        
        if (-not $produtor.telefone3) {
            Write-Host "❌ Campo telefone3 não encontrado ou vazio" -ForegroundColor Red
            $camposOk = $false
        }
        
        if (-not $produtor.email) {
            Write-Host "❌ Campo email não encontrado ou vazio" -ForegroundColor Red
            $camposOk = $false
        }
        
        if ($camposOk) {
            Write-Host "✅ Todos os novos campos estão funcionando!" -ForegroundColor Green
        }
        
    } else {
        Write-Host "❌ Erro ao buscar produtor: $($getProdutoById.Error)" -ForegroundColor Red
    }
    
} else {
    Write-Host "❌ Erro ao criar produtor completo: $($createCompleto.Error)" -ForegroundColor Red
    Write-Host "📋 Verifique se:" -ForegroundColor Yellow
    Write-Host "   - O banco de dados foi atualizado com os novos campos" -ForegroundColor White
    Write-Host "   - O serviço de usuários está funcionando" -ForegroundColor White
    Write-Host "   - As validações estão corretas" -ForegroundColor White
}

# Teste 5: Criar um produtor simples (método antigo)
Write-Host ""
Write-Host "🔍 Teste 5: Testando criação de produtor simples (compatibilidade)..." -ForegroundColor Cyan

$novoProdutorSimples = @{
    nome = "Produtor Teste Simples"
    cpf = "11122233344"
    telefone1 = "(11) 88888-1111"
    telefone2 = "(11) 88888-2222"
    telefone3 = "(11) 88888-3333"
    email = "produtor.simples@agriis.com"
    areaPlantio = 50.0
    culturas = @(1)
}

$createSimples = Invoke-ApiRequest -Method "POST" -Endpoint "/api/produtores" -Body $novoProdutorSimples

if ($createSimples.Success) {
    Write-Host "✅ Produtor simples criado com sucesso!" -ForegroundColor Green
    Write-Host "   ID: $($createSimples.Data.id)" -ForegroundColor Gray
} else {
    Write-Host "❌ Erro ao criar produtor simples: $($createSimples.Error)" -ForegroundColor Red
}

# Resumo dos testes
Write-Host ""
Write-Host "=== Resumo dos Testes ===" -ForegroundColor Green

$testResults = @(
    @{ Name = "API Acessível"; Status = $healthCheck.Success }
    @{ Name = "Listagem de Produtores"; Status = $listProdutores.Success }
    @{ Name = "Criação Completa"; Status = $createCompleto.Success }
    @{ Name = "Criação Simples"; Status = $createSimples.Success }
)

foreach ($test in $testResults) {
    $status = if ($test.Status) { "✅ PASSOU" } else { "❌ FALHOU" }
    $color = if ($test.Status) { "Green" } else { "Red" }
    Write-Host "$($test.Name): $status" -ForegroundColor $color
}

$totalPassed = ($testResults | Where-Object { $_.Status }).Count
$totalTests = $testResults.Count

Write-Host ""
Write-Host "📊 Resultado: $totalPassed/$totalTests testes passaram" -ForegroundColor $(if ($totalPassed -eq $totalTests) { "Green" } else { "Yellow" })

if ($totalPassed -eq $totalTests) {
    Write-Host "🎉 Todos os testes passaram! A implementação está funcionando corretamente." -ForegroundColor Green
} else {
    Write-Host "⚠️  Alguns testes falharam. Verifique os logs acima para mais detalhes." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 Para testar no frontend:" -ForegroundColor Cyan
Write-Host "1. Acesse o formulário de criação de produtores" -ForegroundColor White
Write-Host "2. Preencha os 3 campos de telefone" -ForegroundColor White
Write-Host "3. Preencha os dados do usuário master" -ForegroundColor White
Write-Host "4. Submeta o formulário" -ForegroundColor White
Write-Host "5. Verifique se o produtor e usuário foram criados" -ForegroundColor White