# =====================================================
# SCRIPT DE TESTE: VERIFICAR API DE FORNECEDORES COM BAIRRO
# Data: $(Get-Date)
# Descrição: Testa se a API está retornando os dados de bairro corretamente
# =====================================================

Write-Host "=== TESTANDO API DE FORNECEDORES - CAMPO BAIRRO ===" -ForegroundColor Green

# Configurações
$baseUrl = "https://localhost:7001" # Ajuste conforme necessário
$apiUrl = "$baseUrl/api/fornecedores"

Write-Host "`n=== 1. TESTANDO ENDPOINT DE LISTAGEM ===" -ForegroundColor Yellow

try {
    # Testar endpoint de listagem paginada
    $listUrl = "$apiUrl?Pagina=1&TamanhoPagina=5"
    Write-Host "Chamando: $listUrl" -ForegroundColor Cyan
    
    $response = Invoke-RestMethod -Uri $listUrl -Method GET -ContentType "application/json"
    
    Write-Host "✅ Resposta recebida com sucesso" -ForegroundColor Green
    Write-Host "Total de fornecedores: $($response.total_items)" -ForegroundColor White
    Write-Host "Fornecedores na página: $($response.items.Count)" -ForegroundColor White
    
    # Verificar se os fornecedores têm dados de bairro
    foreach ($fornecedor in $response.items) {
        Write-Host "`n--- Fornecedor ID: $($fornecedor.id) ---" -ForegroundColor Cyan
        Write-Host "Nome: $($fornecedor.nome)" -ForegroundColor White
        Write-Host "Bairro: $($fornecedor.bairro)" -ForegroundColor $(if ($fornecedor.bairro) { "Green" } else { "Red" })
        Write-Host "UF Nome: $($fornecedor.ufNome)" -ForegroundColor $(if ($fornecedor.ufNome) { "Green" } else { "Red" })
        Write-Host "UF Código: $($fornecedor.ufCodigo)" -ForegroundColor $(if ($fornecedor.ufCodigo) { "Green" } else { "Red" })
        Write-Host "Município: $($fornecedor.municipioNome)" -ForegroundColor $(if ($fornecedor.municipioNome) { "Green" } else { "Red" })
        Write-Host "Logradouro: $($fornecedor.logradouro)" -ForegroundColor White
        Write-Host "CEP: $($fornecedor.cep)" -ForegroundColor White
    }
    
} catch {
    Write-Host "❌ Erro ao chamar API de listagem:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 2. TESTANDO ENDPOINT DE DETALHES ===" -ForegroundColor Yellow

try {
    # Pegar o primeiro fornecedor da lista para testar detalhes
    if ($response -and $response.items -and $response.items.Count -gt 0) {
        $primeiroFornecedor = $response.items[0]
        $detailUrl = "$apiUrl/$($primeiroFornecedor.id)"
        
        Write-Host "Chamando: $detailUrl" -ForegroundColor Cyan
        
        $detailResponse = Invoke-RestMethod -Uri $detailUrl -Method GET -ContentType "application/json"
        
        Write-Host "✅ Detalhes recebidos com sucesso" -ForegroundColor Green
        Write-Host "`n--- Detalhes do Fornecedor ---" -ForegroundColor Cyan
        Write-Host "ID: $($detailResponse.id)" -ForegroundColor White
        Write-Host "Nome: $($detailResponse.nome)" -ForegroundColor White
        Write-Host "CNPJ: $($detailResponse.cnpjFormatado)" -ForegroundColor White
        Write-Host "Bairro: $($detailResponse.bairro)" -ForegroundColor $(if ($detailResponse.bairro) { "Green" } else { "Red" })
        Write-Host "Logradouro: $($detailResponse.logradouro)" -ForegroundColor White
        Write-Host "UF ID: $($detailResponse.ufId)" -ForegroundColor White
        Write-Host "UF Nome: $($detailResponse.ufNome)" -ForegroundColor $(if ($detailResponse.ufNome) { "Green" } else { "Red" })
        Write-Host "UF Código: $($detailResponse.ufCodigo)" -ForegroundColor $(if ($detailResponse.ufCodigo) { "Green" } else { "Red" })
        Write-Host "Município ID: $($detailResponse.municipioId)" -ForegroundColor White
        Write-Host "Município Nome: $($detailResponse.municipioNome)" -ForegroundColor $(if ($detailResponse.municipioNome) { "Green" } else { "Red" })
        Write-Host "CEP: $($detailResponse.cep)" -ForegroundColor White
        Write-Host "Complemento: $($detailResponse.complemento)" -ForegroundColor White
    }
    
} catch {
    Write-Host "❌ Erro ao chamar API de detalhes:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 3. TESTANDO ENDPOINT DE TESTE GEOGRÁFICO ===" -ForegroundColor Yellow

try {
    $testUrl = "$apiUrl/teste-dados-geograficos"
    Write-Host "Chamando: $testUrl" -ForegroundColor Cyan
    
    $testResponse = Invoke-RestMethod -Uri $testUrl -Method GET -ContentType "application/json"
    
    Write-Host "✅ Teste geográfico executado com sucesso" -ForegroundColor Green
    Write-Host "Total de fornecedores: $($testResponse.TotalFornecedores)" -ForegroundColor White
    
    foreach ($fornecedor in $testResponse.Fornecedores) {
        Write-Host "`n--- Fornecedor Teste ID: $($fornecedor.Id) ---" -ForegroundColor Cyan
        Write-Host "Nome: $($fornecedor.Nome)" -ForegroundColor White
        Write-Host "Bairro: $($fornecedor.Bairro)" -ForegroundColor $(if ($fornecedor.Bairro) { "Green" } else { "Red" })
        Write-Host "UF ID: $($fornecedor.UfId)" -ForegroundColor White
        Write-Host "UF Nome: $($fornecedor.UfNome)" -ForegroundColor $(if ($fornecedor.UfNome) { "Green" } else { "Red" })
        Write-Host "UF Código: $($fornecedor.UfCodigo)" -ForegroundColor $(if ($fornecedor.UfCodigo) { "Green" } else { "Red" })
        Write-Host "Município ID: $($fornecedor.MunicipioId)" -ForegroundColor White
        Write-Host "Município Nome: $($fornecedor.MunicipioNome)" -ForegroundColor $(if ($fornecedor.MunicipioNome) { "Green" } else { "Red" })
        Write-Host "Dados Completos: $($fornecedor.DadosGeograficosCompletos)" -ForegroundColor $(if ($fornecedor.DadosGeograficosCompletos) { "Green" } else { "Red" })
    }
    
} catch {
    Write-Host "❌ Erro ao chamar API de teste geográfico:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== 4. CRIANDO FORNECEDOR DE TESTE COM BAIRRO ===" -ForegroundColor Yellow

try {
    $novoFornecedor = @{
        nome = "Fornecedor Teste Bairro $(Get-Date -Format 'HHmmss')"
        cnpj = "12345678000195"
        inscricaoEstadual = "123456789"
        logradouro = "Rua das Flores, 123"
        bairro = "Centro"
        ufId = 1
        municipioId = 1
        cep = "78000-000"
        complemento = "Sala 101"
        telefone = "(65) 3333-4444"
        email = "teste@fornecedor.com"
        moedaPadrao = 0
    } | ConvertTo-Json -Depth 10
    
    Write-Host "Criando fornecedor com dados:" -ForegroundColor Cyan
    Write-Host $novoFornecedor -ForegroundColor White
    
    $createResponse = Invoke-RestMethod -Uri $apiUrl -Method POST -Body $novoFornecedor -ContentType "application/json"
    
    Write-Host "✅ Fornecedor criado com sucesso!" -ForegroundColor Green
    Write-Host "ID: $($createResponse.id)" -ForegroundColor White
    Write-Host "Nome: $($createResponse.nome)" -ForegroundColor White
    Write-Host "Bairro: $($createResponse.bairro)" -ForegroundColor $(if ($createResponse.bairro) { "Green" } else { "Red" })
    
} catch {
    Write-Host "❌ Erro ao criar fornecedor de teste:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`n=== RESUMO DOS TESTES ===" -ForegroundColor Green
Write-Host "1. ✅ Endpoint de listagem testado" -ForegroundColor White
Write-Host "2. ✅ Endpoint de detalhes testado" -ForegroundColor White
Write-Host "3. ✅ Endpoint de teste geográfico testado" -ForegroundColor White
Write-Host "4. ✅ Criação de fornecedor testada" -ForegroundColor White

Write-Host "`n=== PRÓXIMOS PASSOS ===" -ForegroundColor Yellow
Write-Host "1. Verifique se os campos Bairro, UF e Município estão sendo retornados" -ForegroundColor White
Write-Host "2. Se algum campo estiver vazio, verifique a migração do banco" -ForegroundColor White
Write-Host "3. Se a API estiver OK, verifique o frontend" -ForegroundColor White
Write-Host "4. Teste a interface web para confirmar a exibição" -ForegroundColor White

Write-Host "`nTeste concluído!" -ForegroundColor Green