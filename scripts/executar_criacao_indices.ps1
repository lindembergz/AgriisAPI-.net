# Script PowerShell para executar a criação de índices faltantes
# Data: 2025-01-27
# Uso: .\executar_criacao_indices.ps1

param(
    [string]$ConnectionString = $null,
    [switch]$DryRun = $false,
    [switch]$SkipBackup = $false
)

# Configurações padrão
$defaultConnectionString = "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432"
$scriptPath = Join-Path $PSScriptRoot "criar_indices_faltantes.sql"

# Usar connection string fornecida ou padrão
$connString = if ($ConnectionString) { $ConnectionString } else { $defaultConnectionString }

Write-Host "=== Criação de Índices Faltantes ===" -ForegroundColor Green
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
    Write-Host ""
    Get-Content $scriptPath | Select-Object -First 50 | Write-Host -ForegroundColor Gray
    Write-Host "... (arquivo completo tem $(Get-Content $scriptPath | Measure-Object -Line | Select-Object -ExpandProperty Lines) linhas)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para executar de verdade, remova o parâmetro -DryRun" -ForegroundColor Yellow
    exit 0
}

# Fazer backup dos índices existentes (se não for pulado)
if (-not $SkipBackup) {
    Write-Host "1. Fazendo backup dos índices existentes..." -ForegroundColor Cyan
    $backupFile = "indices_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    $backupPath = Join-Path $PSScriptRoot $backupFile

    try {
        $backupQuery = @"
SELECT 
    'DROP INDEX IF EXISTS ' || schemaname || '."' || indexname || '";' as drop_statement
FROM pg_indexes 
WHERE schemaname = 'public'
    AND indexname NOT LIKE 'pg_%'
    AND indexname NOT LIKE '%_pkey'
ORDER BY indexname;
"@
        
        psql $connString -c $backupQuery -t --quiet | Out-File -FilePath $backupPath -Encoding UTF8
        Write-Host "✓ Backup de índices salvo em: $backupPath" -ForegroundColor Green
    } catch {
        Write-Warning "Não foi possível fazer backup dos índices: $($_.Exception.Message)"
    }
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

# Contar índices existentes antes da execução
Write-Host "3. Contando índices existentes..." -ForegroundColor Cyan
try {
    $countBefore = psql $connString -c "SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public' AND indexname NOT LIKE 'pg_%';" -t --quiet
    Write-Host "✓ Índices existentes: $($countBefore.Trim())" -ForegroundColor Green
} catch {
    Write-Warning "Não foi possível contar índices existentes: $($_.Exception.Message)"
    $countBefore = "N/A"
}

# Executar o script SQL
Write-Host "4. Executando script de criação de índices..." -ForegroundColor Cyan
Write-Host "   Isso pode demorar alguns minutos dependendo do tamanho das tabelas..." -ForegroundColor Yellow

try {
    $startTime = Get-Date
    $result = psql $connString -f $scriptPath 2>&1
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Script executado com sucesso em $($duration.TotalSeconds.ToString('F2')) segundos!" -ForegroundColor Green
        
        # Mostrar apenas mensagens importantes do resultado
        $importantLines = $result | Where-Object { 
            $_ -match "CREATE INDEX|ERROR|WARNING|NOTICE" -and 
            $_ -notmatch "already exists" 
        }
        
        if ($importantLines) {
            Write-Host "Mensagens importantes:" -ForegroundColor Cyan
            $importantLines | Write-Host -ForegroundColor Gray
        }
    } else {
        throw "Erro na execução: $result"
    }
} catch {
    Write-Error "Falha na execução do script: $($_.Exception.Message)"
    if (-not $SkipBackup -and (Test-Path $backupPath)) {
        Write-Host "Você pode usar o backup para reverter se necessário: $backupPath" -ForegroundColor Yellow
    }
    exit 1
}

# Contar índices após a execução
Write-Host "5. Verificando índices criados..." -ForegroundColor Cyan
try {
    $countAfter = psql $connString -c "SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public' AND indexname NOT LIKE 'pg_%';" -t --quiet
    $newIndices = [int]$countAfter.Trim() - [int]$countBefore.Trim()
    Write-Host "✓ Índices após execução: $($countAfter.Trim())" -ForegroundColor Green
    Write-Host "✓ Novos índices criados: $newIndices" -ForegroundColor Green
} catch {
    Write-Warning "Não foi possível contar índices após execução: $($_.Exception.Message)"
}

# Verificar alguns índices específicos importantes
Write-Host "6. Verificando índices específicos importantes..." -ForegroundColor Cyan
try {
    $checkIndices = @"
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtos_Codigo') THEN '✓'
        ELSE '✗'
    END || ' IX_Produtos_Codigo' as status
UNION ALL
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Produtos_Nome') THEN '✓'
        ELSE '✗'
    END || ' IX_Produtos_Nome'
UNION ALL
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Fornecedor_UfId') THEN '✓'
        ELSE '✗'
    END || ' IX_Fornecedor_UfId'
UNION ALL
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Usuarios_DataCriacao') THEN '✓'
        ELSE '✗'
    END || ' IX_Usuarios_DataCriacao';
"@
    
    $verification = psql $connString -c $checkIndices -t --quiet
    Write-Host "Verificação de índices importantes:" -ForegroundColor Cyan
    $verification | ForEach-Object { 
        $line = $_.Trim()
        if ($line -like "✓*") {
            Write-Host $line -ForegroundColor Green
        } elseif ($line -like "✗*") {
            Write-Host $line -ForegroundColor Red
        }
    }
} catch {
    Write-Warning "Não foi possível verificar índices específicos: $($_.Exception.Message)"
}

# Mostrar estatísticas finais
Write-Host ""
Write-Host "=== Criação de Índices Concluída ===" -ForegroundColor Green
Write-Host "✓ Índices faltantes criados com sucesso" -ForegroundColor Green
if (-not $SkipBackup -and (Test-Path $backupPath)) {
    Write-Host "✓ Backup dos índices salvo em: $backupPath" -ForegroundColor Green
}
Write-Host "✓ Tempo de execução: $($duration.TotalSeconds.ToString('F2')) segundos" -ForegroundColor Green
Write-Host ""

Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Testar a API para verificar se as consultas estão mais rápidas" -ForegroundColor White
Write-Host "2. Monitorar o desempenho das consultas" -ForegroundColor White
Write-Host "3. Considerar criar índices adicionais baseados nos padrões de uso" -ForegroundColor White
Write-Host ""

Write-Host "Para verificar todos os índices criados:" -ForegroundColor Cyan
Write-Host "psql `"$connString`" -c `"SELECT schemaname, tablename, indexname FROM pg_indexes WHERE schemaname = 'public' ORDER BY tablename, indexname;`"" -ForegroundColor Gray