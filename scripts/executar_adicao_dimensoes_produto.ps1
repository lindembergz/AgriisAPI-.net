# Script PowerShell para executar a adição de campos de dimensões na tabela Produto
# Data: 2025-01-27
# Uso: .\executar_adicao_dimensoes_produto.ps1

param(
    [string]$ConnectionString = $null,
    [switch]$DryRun = $false
)

# Configurações padrão
$defaultConnectionString = "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432"
$scriptPath = Join-Path $PSScriptRoot "adicionar_campos_dimensoes_produto.sql"

# Usar connection string fornecida ou padrão
$connString = if ($ConnectionString) { $ConnectionString } else { $defaultConnectionString }

Write-Host "=== Adição de Campos de Dimensões na Tabela Produto ===" -ForegroundColor Green
Write-Host "Data: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Verificar se o arquivo SQL existe
if (-not (Test-Path $scriptPath)) {
    Write-Error "Arquivo SQL não encontrado: $scriptPath"
    exit 1
}

# Verificar se psql está disponível
try {
    $null = Get-Command psql -ErrorAction Stop
    Write-Host "✓ PostgreSQL psql encontrado" -ForegroundColor Green
} catch {
    Write-Error "psql não encontrado. Instale o PostgreSQL client."
    exit 1
}

if ($DryRun) {
    Write-Host "=== MODO DRY RUN - Apenas validação ===" -ForegroundColor Yellow
    Write-Host "Script que seria executado:" -ForegroundColor Yellow
    Get-Content $scriptPath | Write-Host -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para executar de verdade, remova o parâmetro -DryRun" -ForegroundColor Yellow
    exit 0
}

# Fazer backup da estrutura da tabela antes da alteração
Write-Host "1. Fazendo backup da estrutura da tabela Produto..." -ForegroundColor Cyan
$backupFile = "produto_structure_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
$backupPath = Join-Path $PSScriptRoot $backupFile

try {
    $backupCommand = "pg_dump --schema-only --table=public.Produto `"$connString`""
    Invoke-Expression $backupCommand | Out-File -FilePath $backupPath -Encoding UTF8
    Write-Host "✓ Backup salvo em: $backupPath" -ForegroundColor Green
} catch {
    Write-Warning "Não foi possível fazer backup: $($_.Exception.Message)"
}

# Verificar conexão com o banco
Write-Host "2. Testando conexão com o banco de dados..." -ForegroundColor Cyan
try {
    $testQuery = "SELECT version();"
    $result = psql $connString -c $testQuery -t 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Conexão estabelecida com sucesso" -ForegroundColor Green
    } else {
        throw "Erro na conexão: $result"
    }
} catch {
    Write-Error "Falha na conexão com o banco: $($_.Exception.Message)"
    exit 1
}

# Verificar se a tabela Produto existe
Write-Host "3. Verificando se a tabela Produto existe..." -ForegroundColor Cyan
try {
    $checkTable = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Produto' AND table_schema = 'public';"
    $tableExists = psql $connString -c $checkTable -t --quiet
    if ($tableExists.Trim() -eq "1") {
        Write-Host "✓ Tabela Produto encontrada" -ForegroundColor Green
    } else {
        Write-Error "Tabela Produto não encontrada no schema public"
        exit 1
    }
} catch {
    Write-Error "Erro ao verificar tabela: $($_.Exception.Message)"
    exit 1
}

# Verificar se os campos já existem
Write-Host "4. Verificando campos existentes..." -ForegroundColor Cyan
try {
    $checkFields = @"
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'Produto' 
    AND table_schema = 'public' 
    AND column_name IN ('Altura', 'Largura', 'Comprimento', 'PesoNominal');
"@
    $existingFields = psql $connString -c $checkFields -t --quiet
    $fieldList = $existingFields -split "`n" | Where-Object { $_.Trim() -ne "" }
    
    if ($fieldList.Count -gt 0) {
        Write-Host "⚠ Campos já existentes: $($fieldList -join ', ')" -ForegroundColor Yellow
        $continue = Read-Host "Continuar mesmo assim? (s/N)"
        if ($continue -ne "s" -and $continue -ne "S") {
            Write-Host "Operação cancelada pelo usuário" -ForegroundColor Yellow
            exit 0
        }
    } else {
        Write-Host "✓ Nenhum campo conflitante encontrado" -ForegroundColor Green
    }
} catch {
    Write-Warning "Não foi possível verificar campos existentes: $($_.Exception.Message)"
}

# Executar o script SQL
Write-Host "5. Executando script de adição de campos..." -ForegroundColor Cyan
try {
    $result = psql $connString -f $scriptPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Script executado com sucesso!" -ForegroundColor Green
        Write-Host $result -ForegroundColor Gray
    } else {
        throw "Erro na execução: $result"
    }
} catch {
    Write-Error "Falha na execução do script: $($_.Exception.Message)"
    Write-Host "Você pode restaurar a estrutura usando: psql `"$connString`" -f `"$backupPath`"" -ForegroundColor Yellow
    exit 1
}

# Verificar se os campos foram criados corretamente
Write-Host "6. Verificando campos criados..." -ForegroundColor Cyan
try {
    $verifyFields = @"
SELECT 
    column_name,
    data_type,
    numeric_precision,
    numeric_scale,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Produto' 
    AND table_schema = 'public'
    AND column_name IN ('Altura', 'Largura', 'Comprimento', 'PesoNominal')
ORDER BY column_name;
"@
    $verification = psql $connString -c $verifyFields
    Write-Host $verification -ForegroundColor Gray
} catch {
    Write-Warning "Não foi possível verificar os campos criados: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "=== Migração Concluída ===" -ForegroundColor Green
Write-Host "✓ Campos de dimensões adicionados à tabela Produto" -ForegroundColor Green
Write-Host "✓ Backup da estrutura salvo em: $backupPath" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Testar a API para verificar se o mapeamento está funcionando" -ForegroundColor White
Write-Host "2. Executar testes de integração" -ForegroundColor White
Write-Host "3. Atualizar dados de produtos existentes se necessário" -ForegroundColor White