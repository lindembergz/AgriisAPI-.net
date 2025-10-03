# Script para testar os novos campos do Fornecedor
# NomeFantasia, RamosAtividade, EnderecoCorrespondencia

Write-Host "=== Testando Novos Campos do Fornecedor ===" -ForegroundColor Green

$baseUrl = "https://localhost:7001/api"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

# Dados de teste para criar um fornecedor com os novos campos
$fornecedorData = @{
    nome = "Empresa Teste Agro Ltda"
    nomeFantasia = "Teste Agro"
    cnpj = "12345678000190"
    inscricaoEstadual = "123456789"
    ramosAtividade = @("Sementes", "Fertilizantes", "Defensivos Agrícolas")
    enderecoCorrespondencia = "MesmoFaturamento"
    logradouro = "Rua das Flores, 123"
    bairro = "Centro"
    ufId = 1
    municipioId = 1
    cep = "12345-678"
    telefone = "(11) 99999-9999"
    email = "contato@testeagro.com.br"
    moedaPadrao = 0
    ativo = $true
} | ConvertTo-Json -Depth 3

Write-Host "📝 Dados do fornecedor de teste:" -ForegroundColor Cyan
Write-Host $fornecedorData -ForegroundColor White

try {
    Write-Host "`n🔍 1. Testando criação de fornecedor com novos campos..." -ForegroundColor Yellow
    
    # Criar fornecedor
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/fornecedores" -Method POST -Body $fornecedorData -Headers $headers -SkipCertificateCheck
    
    if ($createResponse) {
        Write-Host "✅ Fornecedor criado com sucesso!" -ForegroundColor Green
        Write-Host "   ID: $($createResponse.id)" -ForegroundColor White
        Write-Host "   Nome: $($createResponse.nome)" -ForegroundColor White
        Write-Host "   Nome Fantasia: $($createResponse.nomeFantasia)" -ForegroundColor White
        Write-Host "   Ramos de Atividade: $($createResponse.ramosAtividade -join ', ')" -ForegroundColor White
        Write-Host "   Endereço Correspondência: $($createResponse.enderecoCorrespondencia)" -ForegroundColor White
        
        $fornecedorId = $createResponse.id
        
        Write-Host "`n🔍 2. Testando busca do fornecedor criado..." -ForegroundColor Yellow
        
        # Buscar fornecedor por ID
        $getResponse = Invoke-RestMethod -Uri "$baseUrl/fornecedores/$fornecedorId" -Method GET -Headers $headers -SkipCertificateCheck
        
        if ($getResponse) {
            Write-Host "✅ Fornecedor encontrado!" -ForegroundColor Green
            Write-Host "   Nome Fantasia: $($getResponse.nomeFantasia)" -ForegroundColor White
            Write-Host "   Ramos de Atividade: $($getResponse.ramosAtividade -join ', ')" -ForegroundColor White
            Write-Host "   Endereço Correspondência: $($getResponse.enderecoCorrespondencia)" -ForegroundColor White
        }
        
        Write-Host "`n🔍 3. Testando atualização dos novos campos..." -ForegroundColor Yellow
        
        # Atualizar fornecedor
        $updateData = @{
            nome = $getResponse.nome
            nomeFantasia = "Teste Agro Atualizado"
            ramosAtividade = @("Sementes", "Máquinas e Equipamentos")
            enderecoCorrespondencia = "DiferenteFaturamento"
            inscricaoEstadual = $getResponse.inscricaoEstadual
            logradouro = $getResponse.logradouro
            ufId = $getResponse.ufId
            municipioId = $getResponse.municipioId
            cep = $getResponse.cep
            telefone = $getResponse.telefone
            email = $getResponse.email
            moedaPadrao = $getResponse.moedaPadrao
        } | ConvertTo-Json -Depth 3
        
        $updateResponse = Invoke-RestMethod -Uri "$baseUrl/fornecedores/$fornecedorId" -Method PUT -Body $updateData -Headers $headers -SkipCertificateCheck
        
        if ($updateResponse) {
            Write-Host "✅ Fornecedor atualizado com sucesso!" -ForegroundColor Green
            Write-Host "   Nome Fantasia: $($updateResponse.nomeFantasia)" -ForegroundColor White
            Write-Host "   Ramos de Atividade: $($updateResponse.ramosAtividade -join ', ')" -ForegroundColor White
            Write-Host "   Endereço Correspondência: $($updateResponse.enderecoCorrespondencia)" -ForegroundColor White
        }
        
        Write-Host "`n🔍 4. Testando listagem com filtros..." -ForegroundColor Yellow
        
        # Testar listagem com filtros
        $listResponse = Invoke-RestMethod -Uri "$baseUrl/fornecedores?pagina=1&tamanhoPagina=10" -Method GET -Headers $headers -SkipCertificateCheck
        
        if ($listResponse -and $listResponse.items) {
            Write-Host "✅ Listagem funcionando!" -ForegroundColor Green
            Write-Host "   Total de fornecedores: $($listResponse.total_items)" -ForegroundColor White
            
            $fornecedorNaLista = $listResponse.items | Where-Object { $_.id -eq $fornecedorId }
            if ($fornecedorNaLista) {
                Write-Host "   Fornecedor encontrado na listagem:" -ForegroundColor White
                Write-Host "     Nome Fantasia: $($fornecedorNaLista.nomeFantasia)" -ForegroundColor White
                Write-Host "     Ramos: $($fornecedorNaLista.ramosAtividade -join ', ')" -ForegroundColor White
            }
        }
        
        Write-Host "`n🧹 5. Limpando dados de teste..." -ForegroundColor Yellow
        
        # Limpar dados de teste (opcional)
        # Invoke-RestMethod -Uri "$baseUrl/fornecedores/$fornecedorId" -Method DELETE -Headers $headers -SkipCertificateCheck
        # Write-Host "✅ Fornecedor de teste removido" -ForegroundColor Green
        
    } else {
        Write-Host "❌ Falha ao criar fornecedor" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Erro durante o teste: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalhes: $($_.Exception)" -ForegroundColor Yellow
}

Write-Host "`n=== Teste Concluído ===" -ForegroundColor Green
Write-Host "Verifique se todos os novos campos estão funcionando corretamente:" -ForegroundColor White
Write-Host "- Nome Fantasia" -ForegroundColor White
Write-Host "- Ramos de Atividade (lista)" -ForegroundColor White
Write-Host "- Endereço de Correspondência" -ForegroundColor White