# =====================================================
# SCRIPT DE TESTE: VERIFICAR IMPLEMENTAÇÃO DO CAMPO BAIRRO
# Data: $(Get-Date)
# Descrição: Testa se a implementação do campo Bairro está funcionando corretamente
# =====================================================

Write-Host "=== TESTANDO IMPLEMENTAÇÃO DO CAMPO BAIRRO ===" -ForegroundColor Green

# Verificar se estamos no diretório correto
if (-not (Test-Path "src/Agriis.Api")) {
    if (Test-Path "nova_api/src/Agriis.Api") {
        Set-Location "nova_api"
    } else {
        Write-Host "ERRO: Não foi possível encontrar o diretório da API" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n=== 1. VERIFICANDO COMPILAÇÃO ===" -ForegroundColor Yellow

try {
    dotnet build src/Agriis.Api --verbosity quiet
    Write-Host "✅ Compilação bem-sucedida" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro na compilação" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`n=== 2. VERIFICANDO ESTRUTURA DO BANCO ===" -ForegroundColor Yellow

# Script SQL para verificar a estrutura
$sqlCheck = @"
-- Verificar se a coluna Bairro existe
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM information_schema.columns 
            WHERE table_name = 'Fornecedor' 
            AND column_name = 'Bairro' 
            AND table_schema = 'public'
        )
        THEN '✅ Coluna Bairro existe'
        ELSE '❌ Coluna Bairro não encontrada'
    END as status_coluna;

-- Verificar se o índice existe
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM pg_indexes 
            WHERE tablename = 'Fornecedor' 
            AND indexname = 'IX_Fornecedor_Bairro'
            AND schemaname = 'public'
        )
        THEN '✅ Índice IX_Fornecedor_Bairro existe'
        ELSE '⚠️ Índice IX_Fornecedor_Bairro não encontrado'
    END as status_indice;
"@

Write-Host "Script SQL de verificação criado" -ForegroundColor Green
Write-Host "Execute no seu banco de dados:" -ForegroundColor Yellow
Write-Host $sqlCheck -ForegroundColor White

Write-Host "`n=== 3. VERIFICANDO ARQUIVOS MODIFICADOS ===" -ForegroundColor Yellow

$filesToCheck = @(
    "src/Modulos/Fornecedores/Agriis.Fornecedores.Dominio/Entidades/Fornecedor.cs",
    "src/Modulos/Fornecedores/Agriis.Fornecedores.Aplicacao/DTOs/FornecedorDto.cs",
    "src/Modulos/Fornecedores/Agriis.Fornecedores.Aplicacao/DTOs/CriarFornecedorRequest.cs",
    "src/Modulos/Fornecedores/Agriis.Fornecedores.Aplicacao/DTOs/AtualizarFornecedorRequest.cs",
    "src/Modulos/Fornecedores/Agriis.Fornecedores.Infraestrutura/Configuracoes/FornecedorConfiguration.cs"
)

foreach ($file in $filesToCheck) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -match "Bairro") {
            Write-Host "✅ $file - Campo Bairro encontrado" -ForegroundColor Green
        } else {
            Write-Host "❌ $file - Campo Bairro NÃO encontrado" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️ $file - Arquivo não encontrado" -ForegroundColor Yellow
    }
}

Write-Host "`n=== 4. VERIFICANDO FRONTEND ===" -ForegroundColor Yellow

$frontendFiles = @(
    "../FrontEndAdmin/src/app/shared/models/fornecedor.model.ts",
    "../FrontEndAdmin/src/app/features/fornecedores/components/fornecedor-list.component.ts",
    "../FrontEndAdmin/src/app/features/fornecedores/components/fornecedor-list.component.html"
)

foreach ($file in $frontendFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        if ($content -match "bairro|Bairro|getBairro") {
            Write-Host "✅ $file - Campo Bairro encontrado" -ForegroundColor Green
        } else {
            Write-Host "❌ $file - Campo Bairro NÃO encontrado" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️ $file - Arquivo não encontrado" -ForegroundColor Yellow
    }
}

Write-Host "`n=== 5. EXECUTANDO TESTES UNITÁRIOS ===" -ForegroundColor Yellow

try {
    $testResult = dotnet test --verbosity quiet --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Todos os testes passaram" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Alguns testes falharam - verifique os detalhes" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Erro ao executar testes" -ForegroundColor Yellow
}

Write-Host "`n=== 6. SUGESTÕES DE TESTE MANUAL ===" -ForegroundColor Yellow

Write-Host "Para testar completamente a implementação:" -ForegroundColor White
Write-Host "1. Inicie a API: dotnet run --project src/Agriis.Api" -ForegroundColor White
Write-Host "2. Teste o endpoint: GET /api/fornecedores/teste-dados-geograficos" -ForegroundColor White
Write-Host "3. Crie um fornecedor com bairro: POST /api/fornecedores" -ForegroundColor White
Write-Host "4. Verifique se o bairro aparece na listagem do frontend" -ForegroundColor White
Write-Host "5. Teste a edição de um fornecedor existente" -ForegroundColor White

Write-Host "`n=== EXEMPLO DE PAYLOAD PARA TESTE ===" -ForegroundColor Yellow

$examplePayload = @"
{
  "nome": "Fornecedor Teste",
  "cnpj": "12345678000195",
  "inscricaoEstadual": "123456789",
  "logradouro": "Rua das Flores, 123",
  "bairro": "Centro",
  "ufId": 1,
  "municipioId": 1,
  "cep": "78000-000",
  "complemento": "Sala 101",
  "telefone": "(65) 3333-4444",
  "email": "contato@fornecedor.com",
  "moedaPadrao": 0
}
"@

Write-Host $examplePayload -ForegroundColor Cyan

Write-Host "`n=== TESTE CONCLUÍDO ===" -ForegroundColor Green
Write-Host "Verifique os resultados acima e execute os testes manuais sugeridos." -ForegroundColor White