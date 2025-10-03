#!/usr/bin/env pwsh

# Script para testar a correção do carregamento do usuário Master

Write-Host "🧪 Testando correção do usuário Master..." -ForegroundColor Cyan

# Simular dados da API (baseado na resposta real que você mostrou)
$apiResponse = @{
    id = 6
    nome = "Lindemberg Cortez Gomes Araujo"
    cnpj = "68055398000105"
    cnpjFormatado = "68.055.398/0001-05"
    inscricaoEstadual = "11111111"
    logradouro = "Rua Lauro Victor de Barros Junior"
    ufId = 2
    ufNome = "SAO PAULO"
    ufCodigo = "SP"
    municipioId = 10
    municipioNome = "São Paulo"
    bairro = "JARDIM OCEANIA"
    cep = "58037755"
    complemento = "AP 702"
    latitude = $null
    longitude = $null
    telefone = "83991247126"
    email = "lindemberg@gmail.com"
    logoUrl = $null
    moedaPadrao = 0
    moedaPadraoNome = "Desconhecido"
    pedidoMinimo = $null
    tokenLincros = $null
    ativo = $true
    dadosAdicionais = $null
    dataCriacao = "2025-09-25T16:16:22.57007"
    dataAtualizacao = $null
    usuarios = @(
        @{
            id = 1
            usuarioId = 3
            usuarioNome = "Lindemberg Cortez Gomes"
            usuarioEmail = "lindemberg.cortez@gmail.com"
            fornecedorId = 6
            fornecedorNome = "Lindemberg Cortez Gomes Araujo"
            role = 3
            roleNome = "Administrador"
            ativo = $true
            dataInicio = "2025-09-25T01:16:23.666229Z"
            dataFim = $null
            dataCriacao = "2025-09-25T16:16:23.711117"
            dataAtualizacao = $null
            territorios = @()
        }
    )
}

Write-Host "📊 Dados simulados da API:" -ForegroundColor Yellow
Write-Host "  - Fornecedor: $($apiResponse.nome)" -ForegroundColor White
Write-Host "  - Usuários: $($apiResponse.usuarios.Count)" -ForegroundColor White

# Simular a lógica de busca do usuário Master
$usuarioMaster = $apiResponse.usuarios | Where-Object { $_.role -eq 3 -or $_.roleNome -eq "Administrador" } | Select-Object -First 1

if ($usuarioMaster) {
    Write-Host "✅ Usuário Master encontrado:" -ForegroundColor Green
    Write-Host "  - Nome: $($usuarioMaster.usuarioNome)" -ForegroundColor White
    Write-Host "  - Email: $($usuarioMaster.usuarioEmail)" -ForegroundColor White
    Write-Host "  - Role: $($usuarioMaster.roleNome) (ID: $($usuarioMaster.role))" -ForegroundColor White
    Write-Host "  - Ativo: $($usuarioMaster.ativo)" -ForegroundColor White
    
    # Simular o que seria populado no formulário
    $formData = @{
        nome = $usuarioMaster.usuarioNome
        email = $usuarioMaster.usuarioEmail
        senha = "" # Não retornamos a senha por segurança
        telefone = "" # Telefone não está na estrutura atual
    }
    
    Write-Host "📝 Dados que seriam populados no formulário:" -ForegroundColor Cyan
    $formData.GetEnumerator() | ForEach-Object {
        Write-Host "  - $($_.Key): '$($_.Value)'" -ForegroundColor White
    }
}
else {
    Write-Host "❌ Usuário Master NÃO encontrado!" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 Análise da correção:" -ForegroundColor Magenta
Write-Host "  1. ✅ A API está retornando os dados do usuário na propriedade 'usuarios'" -ForegroundColor Green
Write-Host "  2. ✅ O usuário Master tem role = 3 (Administrador)" -ForegroundColor Green
Write-Host "  3. ✅ A correção no frontend deve buscar o usuário com role = 3" -ForegroundColor Green
Write-Host "  4. ✅ Os dados usuarioNome e usuarioEmail estão disponíveis" -ForegroundColor Green
Write-Host "  5. ⚠️  Telefone não está disponível na estrutura atual do UsuarioFornecedorDto" -ForegroundColor Yellow

Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Blue
Write-Host "  1. Testar a correção no frontend" -ForegroundColor White
Write-Host "  2. Verificar se os dados aparecem na aba 'Usuário Master'" -ForegroundColor White
Write-Host "  3. Considerar adicionar telefone ao UsuarioFornecedorDto se necessário" -ForegroundColor White

Write-Host ""
Write-Host "✅ Teste concluído!" -ForegroundColor Green