#!/usr/bin/env pwsh

# Script para testar a implementação completa do campo Bairro
# Testa criação, leitura e atualização de fornecedor com bairro

Write-Host "🧪 Testando implementação completa do campo Bairro" -ForegroundColor Green
Write-Host "=" * 60

$baseUrl = "https://localhost:7001/api"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Method = $Method
            Uri = $Uri
            Headers = $headers
        }
        
        # Ignorar erros de certificado SSL para desenvolvimento
        if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
            $certCallback = @"
                using System;
                using System.Net;
                using System.Net.Security;
                using System.Security.Cryptography.X509Certificates;
                public class ServerCertificateValidationCallback
                {
                    public static void Ignore()
                    {
                        if(ServicePointManager.ServerCertificateValidationCallback ==null)
                        {
                            ServicePointManager.ServerCertificateValidationCallback += 
                                delegate
                                (
                                    Object obj, 
                                    X509Certificate certificate, 
                                    X509Chain chain, 
                                    SslPolicyErrors errors
                                )
                                {
                                    return true;
                                };
                        }
                    }
                }
"@
            Add-Type $certCallback
        }
        [ServerCertificateValidationCallback]::Ignore()
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "❌ Erro na requisição: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Resposta do servidor: $responseBody" -ForegroundColor Yellow
        }
        throw
    }
}

# Teste 1: Criar fornecedor com bairro
Write-Host "`n1️⃣ Testando criação de fornecedor com bairro..." -ForegroundColor Cyan

$novoFornecedor = @{
    nome = "Fornecedor Teste Bairro $(Get-Date -Format 'HHmmss')"
    cpfCnpj = "12345678901234"
    tipoCliente = 1
    telefone = "84999887766"
    email = "teste.bairro@exemplo.com"
    logradouro = "Rua das Flores, 123"
    numero = "123"
    complemento = "Sala 101"
    bairro = "Centro Histórico"
    cidade = "Natal"
    uf = "RN"
    cep = "59000000"
    ufId = 20
    municipioId = 1706
    latitude = -5.7945
    longitude = -35.2110
    usuarioMaster = @{
        nome = "Usuario Master Teste"
        email = "master.teste@exemplo.com"
        senha = "senha123456"
        telefone = "84988776655"
    }
}

try {
    $fornecedorCriado = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/fornecedores" -Body $novoFornecedor
    Write-Host "✅ Fornecedor criado com sucesso!" -ForegroundColor Green
    Write-Host "   ID: $($fornecedorCriado.id)" -ForegroundColor White
    Write-Host "   Nome: $($fornecedorCriado.nome)" -ForegroundColor White
    Write-Host "   Bairro: $($fornecedorCriado.bairro)" -ForegroundColor Yellow
    
    $fornecedorId = $fornecedorCriado.id
}
catch {
    Write-Host "❌ Falha ao criar fornecedor" -ForegroundColor Red
    exit 1
}

# Teste 2: Buscar fornecedor e verificar bairro
Write-Host "`n2️⃣ Testando busca de fornecedor..." -ForegroundColor Cyan

try {
    $fornecedorBuscado = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/fornecedores/$fornecedorId"
    Write-Host "✅ Fornecedor encontrado!" -ForegroundColor Green
    Write-Host "   ID: $($fornecedorBuscado.id)" -ForegroundColor White
    Write-Host "   Nome: $($fornecedorBuscado.nome)" -ForegroundColor White
    Write-Host "   Bairro: $($fornecedorBuscado.bairro)" -ForegroundColor Yellow
    Write-Host "   UF: $($fornecedorBuscado.ufNome) ($($fornecedorBuscado.ufCodigo))" -ForegroundColor White
    Write-Host "   Município: $($fornecedorBuscado.municipioNome)" -ForegroundColor White
    
    # Verificar se o bairro foi salvo corretamente
    if ($fornecedorBuscado.bairro -eq "Centro Histórico") {
        Write-Host "✅ Campo Bairro salvo corretamente!" -ForegroundColor Green
    } else {
        Write-Host "❌ Campo Bairro não foi salvo corretamente. Esperado: 'Centro Histórico', Atual: '$($fornecedorBuscado.bairro)'" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Falha ao buscar fornecedor" -ForegroundColor Red
    exit 1
}

# Teste 3: Atualizar bairro do fornecedor
Write-Host "`n3️⃣ Testando atualização do bairro..." -ForegroundColor Cyan

$fornecedorAtualizado = @{
    nome = $fornecedorBuscado.nome
    cpfCnpj = $fornecedorBuscado.cpfCnpj
    tipoCliente = $fornecedorBuscado.tipoCliente
    telefone = $fornecedorBuscado.telefone
    email = $fornecedorBuscado.email
    logradouro = $fornecedorBuscado.logradouro
    numero = $fornecedorBuscado.numero
    complemento = $fornecedorBuscado.complemento
    bairro = "Bairro Atualizado Via API"  # Novo bairro
    cidade = $fornecedorBuscado.cidade
    uf = $fornecedorBuscado.ufCodigo
    cep = $fornecedorBuscado.cep
    ufId = $fornecedorBuscado.ufId
    municipioId = $fornecedorBuscado.municipioId
    latitude = $fornecedorBuscado.latitude
    longitude = $fornecedorBuscado.longitude
}

try {
    $fornecedorAtualizadoResponse = Invoke-ApiRequest -Method "PUT" -Uri "$baseUrl/fornecedores/$fornecedorId" -Body $fornecedorAtualizado
    Write-Host "✅ Fornecedor atualizado com sucesso!" -ForegroundColor Green
    
    # Buscar novamente para verificar a atualização
    $fornecedorVerificacao = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/fornecedores/$fornecedorId"
    Write-Host "   Bairro atualizado: $($fornecedorVerificacao.bairro)" -ForegroundColor Yellow
    
    if ($fornecedorVerificacao.bairro -eq "Bairro Atualizado Via API") {
        Write-Host "✅ Campo Bairro atualizado corretamente!" -ForegroundColor Green
    } else {
        Write-Host "❌ Campo Bairro não foi atualizado corretamente. Esperado: 'Bairro Atualizado Via API', Atual: '$($fornecedorVerificacao.bairro)'" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Falha ao atualizar fornecedor" -ForegroundColor Red
    exit 1
}

# Teste 4: Listar fornecedores e verificar bairro na listagem
Write-Host "`n4️⃣ Testando listagem de fornecedores..." -ForegroundColor Cyan

try {
    $fornecedores = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/fornecedores?pageSize=10"
    Write-Host "✅ Listagem obtida com sucesso!" -ForegroundColor Green
    Write-Host "   Total de fornecedores: $($fornecedores.totalItems)" -ForegroundColor White
    
    # Procurar nosso fornecedor na listagem
    $fornecedorNaLista = $fornecedores.items | Where-Object { $_.id -eq $fornecedorId }
    if ($fornecedorNaLista) {
        Write-Host "✅ Fornecedor encontrado na listagem!" -ForegroundColor Green
        Write-Host "   Bairro na listagem: $($fornecedorNaLista.bairro)" -ForegroundColor Yellow
        
        if ($fornecedorNaLista.bairro -eq "Bairro Atualizado Via API") {
            Write-Host "✅ Campo Bairro aparece corretamente na listagem!" -ForegroundColor Green
        } else {
            Write-Host "❌ Campo Bairro não aparece corretamente na listagem" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Fornecedor não encontrado na listagem" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Falha ao listar fornecedores" -ForegroundColor Red
}

# Teste 5: Verificar validação de bairro obrigatório
Write-Host "`n5️⃣ Testando validação de bairro obrigatório..." -ForegroundColor Cyan

$fornecedorSemBairro = @{
    nome = "Fornecedor Sem Bairro"
    cpfCnpj = "98765432109876"
    tipoCliente = 1
    telefone = "84999887766"
    email = "sem.bairro@exemplo.com"
    logradouro = "Rua Sem Bairro, 456"
    numero = "456"
    complemento = ""
    bairro = ""  # Bairro vazio
    cidade = "Natal"
    uf = "RN"
    cep = "59000000"
    ufId = 20
    municipioId = 1706
    usuarioMaster = @{
        nome = "Usuario Master Sem Bairro"
        email = "master.sem.bairro@exemplo.com"
        senha = "senha123456"
        telefone = "84988776655"
    }
}

try {
    $fornecedorSemBairroResponse = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/fornecedores" -Body $fornecedorSemBairro
    Write-Host "⚠️ Fornecedor criado mesmo sem bairro - validação pode estar desabilitada" -ForegroundColor Yellow
    Write-Host "   ID: $($fornecedorSemBairroResponse.id)" -ForegroundColor White
    Write-Host "   Bairro: '$($fornecedorSemBairroResponse.bairro)'" -ForegroundColor Yellow
}
catch {
    Write-Host "✅ Validação funcionando - fornecedor rejeitado por falta de bairro" -ForegroundColor Green
}

Write-Host "`n" + "=" * 60
Write-Host "🎉 Testes da implementação do campo Bairro concluídos!" -ForegroundColor Green
Write-Host "=" * 60

# Limpeza opcional - remover fornecedor de teste
Write-Host "`n🧹 Limpando dados de teste..." -ForegroundColor Cyan
try {
    Invoke-ApiRequest -Method "DELETE" -Uri "$baseUrl/fornecedores/$fornecedorId"
    Write-Host "✅ Fornecedor de teste removido" -ForegroundColor Green
}
catch {
    Write-Host "⚠️ Não foi possível remover o fornecedor de teste (ID: $fornecedorId)" -ForegroundColor Yellow
}

Write-Host "`n✨ Script concluído com sucesso!" -ForegroundColor Green