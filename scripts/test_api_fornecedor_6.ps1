# =====================================================
# SCRIPT DE TESTE: VERIFICAR API FORNECEDOR ID 6
# Data: $(Get-Date)
# Descrição: Testa especificamente o fornecedor ID 6 para debug
# =====================================================

Write-Host "=== TESTANDO FORNECEDOR ID 6 ===" -ForegroundColor Green

# Configurações
$baseUrl = "https://localhost:7001" # Ajuste conforme necessário
$fornecedorId = 6

Write-Host "`n=== 1. TESTANDO ENDPOINT GET BY ID ===" -ForegroundColor Yellow

try {
    $url = "$baseUrl/api/fornecedores/$fornecedorId"
    Write-Host "Chamando: $url" -ForegroundColor Cyan
    
    $response = Invoke-RestMethod -Uri $url -Method GET -ContentType "application/json"
    
    Write-Host "✅ Resposta recebida com sucesso" -ForegroundColor Green
    
    # Mostrar dados detalhados
    Write-Host "`n--- DADOS DO FORNECEDOR ---" -ForegroundColor Cyan
    Write-Host "ID: $($response.id)" -ForegroundColor White
    Write-Host "Nome: $($response.nome)" -ForegroundColor White
    Write-Host "CNPJ: $($response.cnpjFormatado)" -ForegroundColor White
    Write-Host "Logradouro: $($response.logradouro)" -ForegroundColor White
    Write-Host "Bairro: '$($response.bairro)'" -ForegroundColor $(if ($response.bairro) { "Green" } else { "Red" })
    Write-Host "CEP: $($response.cep)" -ForegroundColor White
    Write-Host "Complemento: $($response.complemento)" -ForegroundColor White
    
    Write-Host "`n--- DADOS GEOGRÁFICOS ---" -ForegroundColor Cyan
    Write-Host "UF ID: $($response.ufId)" -ForegroundColor White
    Write-Host "UF Nome: '$($response.ufNome)'" -ForegroundColor $(if ($response.ufNome) { "Green" } else { "Red" })
    Write-Host "UF Código: '$($response.ufCodigo)'" -ForegroundColor $(if ($response.ufCodigo) { "Green" } else { "Red" })
    Write-Host "Município ID: $($response.municipioId)" -ForegroundColor White
    Write-Host "Município Nome: '$($response.municipioNome)'" -ForegroundColor $(if ($response.municipioNome) { "Green" } else { "Red" })
    
    Write-Host "`n--- OUTROS DADOS ---" -ForegroundColor Cyan
    Write-Host "Telefone: $($response.telefone)" -ForegroundColor White
    Write-Host "Email: $($response.email)" -ForegroundColor White
    Write-Host "Ativo: $($response.ativo)" -ForegroundColor White
    Write-Host "Data Criação: $($response.dataCriacao)" -ForegroundColor White
    Write-Host "Data Atualização: $($response.dataAtualizacao)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Erro ao chamar API:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 2. TESTANDO ENDPOINT DEBUG BAIRRO ===" -ForegroundColor Yellow

try {
    $debugUrl = "$baseUrl/api/fornecedores/debug-bairro/$fornecedorId"
    Write-Host "Chamando: $debugUrl" -ForegroundColor Cyan
    
    $debugResponse = Invoke-RestMethod -Uri $debugUrl -Method GET -ContentType "application/json"
    
    Write-Host "✅ Debug executado com sucesso" -ForegroundColor Green
    
    # Mostrar dados de debug
    Write-Host "`n--- DEBUG BAIRRO ---" -ForegroundColor Cyan
    Write-Host "Fornecedor ID: $($debugResponse.fornecedorId)" -ForegroundColor White
    Write-Host "Nome: $($debugResponse.nome)" -ForegroundColor White
    Write-Host "Bairro: '$($debugResponse.bairro)'" -ForegroundColor $(if ($debugResponse.bairro) { "Green" } else { "Red" })
    Write-Host "Bairro Is Null: $($debugResponse.bairroIsNull)" -ForegroundColor $(if ($debugResponse.bairroIsNull) { "Red" } else { "Green" })
    Write-Host "Bairro Is Empty: $($debugResponse.bairroIsEmpty)" -ForegroundColor $(if ($debugResponse.bairroIsEmpty) { "Red" } else { "Green" })
    Write-Host "Bairro Length: $($debugResponse.bairroLength)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Erro ao chamar API de debug:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 3. TESTANDO ATUALIZAÇÃO DO BAIRRO ===" -ForegroundColor Yellow

try {
    $updateUrl = "$baseUrl/api/fornecedores/$fornecedorId"
    
    # Dados para atualização (apenas o bairro)
    $updateData = @{
        nome = "Lindemberg Cortez Gomes Araujo"
        inscricaoEstadual = "11111111"
        logradouro = "Rua Lauro Victor de Barros Junior"
        bairro = "Tambauzinho"
        ufId = 2
        municipioId = 10
        cep = "58037755"
        complemento = "AP 702"
        telefone = "83991247126"
        email = "lindemberg@gmail.com"
        moedaPadrao = 0
        pedidoMinimo = $null
        tokenLincros = $null
    } | ConvertTo-Json -Depth 10
    
    Write-Host "Atualizando fornecedor com bairro..." -ForegroundColor Cyan
    Write-Host $updateData -ForegroundColor White
    
    $updateResponse = Invoke-RestMethod -Uri $updateUrl -Method PUT -Body $updateData -ContentType "application/json"
    
    Write-Host "✅ Fornecedor atualizado com sucesso!" -ForegroundColor Green
    Write-Host "Bairro atualizado: '$($updateResponse.bairro)'" -ForegroundColor $(if ($updateResponse.bairro) { "Green" } else { "Red" })
    
} catch {
    Write-Host "❌ Erro ao atualizar fornecedor:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 4. VERIFICAÇÃO FINAL ===" -ForegroundColor Yellow

try {
    $finalUrl = "$baseUrl/api/fornecedores/$fornecedorId"
    Write-Host "Verificação final: $finalUrl" -ForegroundColor Cyan
    
    $finalResponse = Invoke-RestMethod -Uri $finalUrl -Method GET -ContentType "application/json"
    
    Write-Host "✅ Verificação final concluída" -ForegroundColor Green
    Write-Host "Bairro final: '$($finalResponse.bairro)'" -ForegroundColor $(if ($finalResponse.bairro) { "Green" } else { "Red" })
    Write-Host "UF Nome: '$($finalResponse.ufNome)'" -ForegroundColor $(if ($finalResponse.ufNome) { "Green" } else { "Red" })
    Write-Host "Município Nome: '$($finalResponse.municipioNome)'" -ForegroundColor $(if ($finalResponse.municipioNome) { "Green" } else { "Red" })
    
} catch {
    Write-Host "❌ Erro na verificação final:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== RESUMO ===" -ForegroundColor Green
Write-Host "Execute este script para verificar se:" -ForegroundColor White
Write-Host "1. A API está retornando os dados corretos" -ForegroundColor White
Write-Host "2. O bairro está sendo mapeado corretamente" -ForegroundColor White
Write-Host "3. Os dados geográficos estão completos" -ForegroundColor White
Write-Host "4. A atualização está funcionando" -ForegroundColor White

Write-Host "`nTeste concluído!" -ForegroundColor Green