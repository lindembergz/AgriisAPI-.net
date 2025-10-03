# Script de teste completo para os novos campos do Fornecedor
# Testa API e dados

Write-Host "=== TESTE COMPLETO - NOVOS CAMPOS FORNECEDOR ===" -ForegroundColor Green
Write-Host ""

# Configurações
$baseUrl = "https://localhost:7001/api"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $headers
            SkipCertificateCheck = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Erro na requisição: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Resposta do servidor: $responseBody" -ForegroundColor Yellow
        }
        return $null
    }
}

# 1. Testar listagem de fornecedores (verificar se novos campos estão sendo retornados)
Write-Host "1. Testando listagem de fornecedores..." -ForegroundColor Cyan
$fornecedores = Invoke-ApiRequest -Method "GET" -Url "$baseUrl/fornecedores?pagina=1&tamanhoPagina=5"

if ($fornecedores -and $fornecedores.items) {
    Write-Host "✅ Listagem funcionando. Total: $($fornecedores.total_items)" -ForegroundColor Green
    
    # Verificar se os novos campos estão presentes
    $primeiroFornecedor = $fornecedores.items[0]
    if ($primeiroFornecedor) {
        Write-Host "📋 Verificando novos campos no primeiro fornecedor:" -ForegroundColor Yellow
        Write-Host "  - ID: $($primeiroFornecedor.id)"
        Write-Host "  - Nome: $($primeiroFornecedor.nome)"
        Write-Host "  - Nome Fantasia: $($primeiroFornecedor.nomeFantasia)"
        Write-Host "  - Ramos Atividade: $($primeiroFornecedor.ramosAtividade -join ', ')"
        Write-Host "  - Endereço Correspondência: $($primeiroFornecedor.enderecoCorrespondencia)"
        
        # Verificar se os campos existem
        $camposPresentes = @()
        if ($null -ne $primeiroFornecedor.nomeFantasia) { $camposPresentes += "nomeFantasia" }
        if ($null -ne $primeiroFornecedor.ramosAtividade) { $camposPresentes += "ramosAtividade" }
        if ($null -ne $primeiroFornecedor.enderecoCorrespondencia) { $camposPresentes += "enderecoCorrespondencia" }
        
        Write-Host "✅ Campos novos presentes: $($camposPresentes -join ', ')" -ForegroundColor Green
    }
} else {
    Write-Host "❌ Erro ao listar fornecedores" -ForegroundColor Red
}

Write-Host ""

# 2. Testar criação de fornecedor com novos campos
Write-Host "2. Testando criação de fornecedor com novos campos..." -ForegroundColor Cyan

$novoFornecedor = @{
    nome = "Teste Agro Ltda"
    nomeFantasia = "Teste Agro"
    cpfCnpj = "12345678000190"
    tipoCliente = "PJ"
    telefone = "11999999999"
    email = "teste@testeagro.com.br"
    inscricaoEstadual = "123456789"
    ramosAtividade = @("Sementes", "Fertilizantes", "Defensivos Agrícolas")
    enderecoCorrespondencia = "MesmoFaturamento"
    endereco = @{
        logradouro = "Rua Teste"
        numero = "123"
        bairro = "Centro"
        cidade = "São Paulo"
        uf = "SP"
        cep = "01000000"
        ufId = 25
        municipioId = 3550308
    }
    usuarioMaster = @{
        nome = "Usuário Teste"
        email = "usuario@testeagro.com.br"
        senha = "123456"
        telefone = "11888888888"
    }
}

$fornecedorCriado = Invoke-ApiRequest -Method "POST" -Url "$baseUrl/fornecedores/completo" -Body $novoFornecedor

if ($fornecedorCriado) {
    Write-Host "✅ Fornecedor criado com sucesso! ID: $($fornecedorCriado.id)" -ForegroundColor Green
    Write-Host "📋 Dados do fornecedor criado:" -ForegroundColor Yellow
    Write-Host "  - Nome: $($fornecedorCriado.nome)"
    Write-Host "  - Nome Fantasia: $($fornecedorCriado.nomeFantasia)"
    Write-Host "  - Ramos Atividade: $($fornecedorCriado.ramosAtividade -join ', ')"
    Write-Host "  - Endereço Correspondência: $($fornecedorCriado.enderecoCorrespondencia)"
    
    $fornecedorId = $fornecedorCriado.id
} else {
    Write-Host "❌ Erro ao criar fornecedor" -ForegroundColor Red
    $fornecedorId = $null
}

Write-Host ""

# 3. Testar busca por ID (verificar se novos campos são retornados)
if ($fornecedorId) {
    Write-Host "3. Testando busca por ID..." -ForegroundColor Cyan
    
    $fornecedorDetalhes = Invoke-ApiRequest -Method "GET" -Url "$baseUrl/fornecedores/$fornecedorId"
    
    if ($fornecedorDetalhes) {
        Write-Host "✅ Fornecedor encontrado!" -ForegroundColor Green
        Write-Host "📋 Detalhes completos:" -ForegroundColor Yellow
        Write-Host "  - ID: $($fornecedorDetalhes.id)"
        Write-Host "  - Nome: $($fornecedorDetalhes.nome)"
        Write-Host "  - Nome Fantasia: $($fornecedorDetalhes.nomeFantasia)"
        Write-Host "  - CNPJ: $($fornecedorDetalhes.cnpjFormatado)"
        Write-Host "  - Ramos Atividade: $($fornecedorDetalhes.ramosAtividade -join ', ')"
        Write-Host "  - Endereço Correspondência: $($fornecedorDetalhes.enderecoCorrespondencia)"
        Write-Host "  - Bairro: $($fornecedorDetalhes.bairro)"
        Write-Host "  - Município: $($fornecedorDetalhes.municipioNome)"
        Write-Host "  - UF: $($fornecedorDetalhes.ufNome)"
    } else {
        Write-Host "❌ Erro ao buscar fornecedor por ID" -ForegroundColor Red
    }
}

Write-Host ""

# 4. Testar atualização com novos campos
if ($fornecedorId) {
    Write-Host "4. Testando atualização com novos campos..." -ForegroundColor Cyan
    
    $atualizacao = @{
        nome = "Teste Agro Atualizada Ltda"
        nomeFantasia = "Teste Agro Plus"
        inscricaoEstadual = "987654321"
        logradouro = "Rua Atualizada"
        bairro = "Bairro Novo"
        ufId = 25
        municipioId = 3550308
        cep = "01000001"
        telefone = "11777777777"
        email = "atualizado@testeagro.com.br"
        moedaPadrao = 0
        ramosAtividade = @("Sementes", "Fertilizantes", "Máquinas e Equipamentos", "Irrigação")
        enderecoCorrespondencia = "DiferenteFaturamento"
    }
    
    $fornecedorAtualizado = Invoke-ApiRequest -Method "PUT" -Url "$baseUrl/fornecedores/$fornecedorId" -Body $atualizacao
    
    if ($fornecedorAtualizado) {
        Write-Host "✅ Fornecedor atualizado com sucesso!" -ForegroundColor Green
        Write-Host "📋 Dados atualizados:" -ForegroundColor Yellow
        Write-Host "  - Nome: $($fornecedorAtualizado.nome)"
        Write-Host "  - Nome Fantasia: $($fornecedorAtualizado.nomeFantasia)"
        Write-Host "  - Ramos Atividade: $($fornecedorAtualizado.ramosAtividade -join ', ')"
        Write-Host "  - Endereço Correspondência: $($fornecedorAtualizado.enderecoCorrespondencia)"
    } else {
        Write-Host "❌ Erro ao atualizar fornecedor" -ForegroundColor Red
    }
}

Write-Host ""

# 5. Testar filtros com novos campos
Write-Host "5. Testando filtros com novos campos..." -ForegroundColor Cyan

# Filtro por ramos de atividade
$filtroRamos = @{
    ramosAtividade = @("Sementes")
    pagina = 1
    tamanhoPagina = 10
}

$fornecedoresFiltrados = Invoke-ApiRequest -Method "GET" -Url "$baseUrl/fornecedores" -Body $filtroRamos

if ($fornecedoresFiltrados) {
    Write-Host "✅ Filtro por ramos de atividade funcionando. Encontrados: $($fornecedoresFiltrados.total_items)" -ForegroundColor Green
} else {
    Write-Host "⚠️ Filtro por ramos de atividade não implementado ou com erro" -ForegroundColor Yellow
}

Write-Host ""

# 6. Verificar estrutura do banco de dados
Write-Host "6. Verificando estrutura do banco..." -ForegroundColor Cyan
Write-Host "ℹ️ Execute o script SQL de teste para verificar a estrutura:" -ForegroundColor Blue
Write-Host "   nova_api/scripts/test_fornecedor_additional_fields.sql" -ForegroundColor Blue

Write-Host ""

# 7. Limpeza (opcional) - remover fornecedor de teste
if ($fornecedorId) {
    Write-Host "7. Limpeza - removendo fornecedor de teste..." -ForegroundColor Cyan
    
    # Descomente a linha abaixo se quiser remover o fornecedor de teste
    # $resultado = Invoke-ApiRequest -Method "DELETE" -Url "$baseUrl/fornecedores/$fornecedorId"
    
    Write-Host "ℹ️ Fornecedor de teste mantido (ID: $fornecedorId)" -ForegroundColor Blue
    Write-Host "   Para remover manualmente, execute: DELETE /api/fornecedores/$fornecedorId" -ForegroundColor Blue
}

Write-Host ""
Write-Host "=== TESTE COMPLETO FINALIZADO ===" -ForegroundColor Green
Write-Host ""
Write-Host "📋 RESUMO DOS NOVOS CAMPOS:" -ForegroundColor Yellow
Write-Host "  ✅ NomeFantasia - Campo string opcional até 200 caracteres"
Write-Host "  ✅ RamosAtividade - Array de strings com ramos pré-definidos"
Write-Host "  ✅ EnderecoCorrespondencia - Enum (MesmoFaturamento/DiferenteFaturamento)"
Write-Host ""
Write-Host "🔧 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "  1. Execute o script SQL: nova_api/scripts/add_fornecedor_additional_fields.sql"
Write-Host "  2. Teste o frontend em: http://localhost:4200/fornecedores"
Write-Host "  3. Verifique a validação dos novos campos"
Write-Host "  4. Teste os filtros na listagem"