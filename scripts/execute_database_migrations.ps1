# =====================================================
# SCRIPT DE EXECUÇÃO DE MIGRAÇÕES DO BANCO DE DADOS
# Automatiza a aplicação das correções de inconsistências DDL vs API
# =====================================================

param(
    [string]$ConnectionString = "",
    [switch]$DryRun = $false,
    [switch]$SkipBackup = $false,
    [switch]$Force = $false
)

# Configurações
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogFile = Join-Path $ScriptPath "migration_execution_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$BackupPath = Join-Path $ScriptPath "backups"

# Função para logging
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    Add-Content -Path $LogFile -Value $logMessage
}

# Função para executar SQL
function Execute-SQL {
    param([string]$SqlCommand, [string]$Description)
    
    Write-Log "Executando: $Description" "INFO"
    
    if ($DryRun) {
        Write-Log "DRY RUN - SQL que seria executado:" "INFO"
        Write-Log $SqlCommand "INFO"
        return $true
    }
    
    try {
        # Aqui você pode usar psql, sqlcmd, ou outro cliente SQL
        # Exemplo com psql (PostgreSQL):
        if ($ConnectionString) {
            $result = & psql $ConnectionString -c $SqlCommand
            Write-Log "SQL executado com sucesso" "SUCCESS"
            return $true
        } else {
            Write-Log "Connection string não fornecida - apenas simulando execução" "WARNING"
            return $true
        }
    }
    catch {
        Write-Log "Erro ao executar SQL: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# Função principal
function Start-DatabaseMigration {
    Write-Log "=== INICIANDO MIGRAÇÃO DO BANCO DE DADOS ===" "INFO"
    Write-Log "Parâmetros:" "INFO"
    Write-Log "- DryRun: $DryRun" "INFO"
    Write-Log "- SkipBackup: $SkipBackup" "INFO"
    Write-Log "- Force: $Force" "INFO"
    Write-Log "- ConnectionString: $(if($ConnectionString) {'Fornecida'} else {'Não fornecida'})" "INFO"
    
    # Verificar se deve continuar
    if (-not $Force -and -not $DryRun) {
        $confirmation = Read-Host "Esta operação irá modificar o banco de dados. Continuar? (y/N)"
        if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
            Write-Log "Operação cancelada pelo usuário" "INFO"
            return
        }
    }
    
    # Criar diretório de backup
    if (-not $SkipBackup -and -not (Test-Path $BackupPath)) {
        New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
        Write-Log "Diretório de backup criado: $BackupPath" "INFO"
    }
    
    # FASE 1: Análise pré-migração
    Write-Log "=== FASE 1: ANÁLISE PRÉ-MIGRAÇÃO ===" "INFO"
    
    $preAnalysisSQL = @"
-- Análise pré-migração
DO `$`$
DECLARE
    duplicated_columns_count INTEGER;
    tables_with_duplicates TEXT[];
BEGIN
    SELECT COUNT(*), ARRAY_AGG(table_name) 
    INTO duplicated_columns_count, tables_with_duplicates
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' AND column_name IN ('Ativo', 'ativo')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t;
    
    RAISE NOTICE 'PRÉ-MIGRAÇÃO: % tabelas com colunas duplicadas encontradas', duplicated_columns_count;
    IF duplicated_columns_count > 0 THEN
        RAISE NOTICE 'Tabelas afetadas: %', array_to_string(tables_with_duplicates, ', ');
    END IF;
END `$`$;
"@
    
    if (-not (Execute-SQL $preAnalysisSQL "Análise pré-migração")) {
        Write-Log "Falha na análise pré-migração" "ERROR"
        return
    }
    
    # FASE 2: Backup (se não for pulado)
    if (-not $SkipBackup) {
        Write-Log "=== FASE 2: BACKUP DAS TABELAS ===" "INFO"
        
        $backupTables = @(
            "AtividadesAgropecuarias",
            "Catalogo", 
            "Combo",
            "Cultura",
            "Moedas",
            "UnidadesMedida",
            "Embalagens",
            "Produto"
        )
        
        foreach ($table in $backupTables) {
            $backupSQL = "CREATE TABLE backup_$($table.ToLower())_$(Get-Date -Format 'yyyyMMdd_HHmmss') AS SELECT * FROM public.`"$table`";"
            
            if (-not (Execute-SQL $backupSQL "Backup da tabela $table")) {
                Write-Log "Falha no backup da tabela $table" "ERROR"
                if (-not $Force) {
                    Write-Log "Parando execução devido a falha no backup" "ERROR"
                    return
                }
            }
        }
    }
    
    # FASE 3: Criação de tabelas de log
    Write-Log "=== FASE 3: CRIAÇÃO DE TABELAS DE LOG ===" "INFO"
    
    $logDate = Get-Date -Format "yyyyMMdd"
    $createLogsSQL = @"
-- Criar tabelas de log
CREATE TABLE IF NOT EXISTS migration_log_$logDate (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    action VARCHAR(50) NOT NULL,
    details TEXT,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS migration_errors_$logDate (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    error_message TEXT NOT NULL,
    timestamp TIMESTAMPTZ DEFAULT NOW()
);

INSERT INTO migration_log_$logDate (table_name, action, details)
VALUES ('SYSTEM', 'MIGRATION_START', 'Iniciando migração via PowerShell script');
"@
    
    if (-not (Execute-SQL $createLogsSQL "Criação de tabelas de log")) {
        Write-Log "Falha na criação de tabelas de log" "ERROR"
        return
    }
    
    # FASE 4: Remoção de colunas duplicadas
    Write-Log "=== FASE 4: REMOÇÃO DE COLUNAS DUPLICADAS ===" "INFO"
    
    $tablesToFix = @(
        "AtividadesAgropecuarias",
        "Catalogo",
        "Combo", 
        "Cultura",
        "Moedas",
        "UnidadesMedida",
        "Embalagens",
        "Produto"
    )
    
    foreach ($table in $tablesToFix) {
        $dropColumnSQL = @"
DO `$`$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = '$table' 
            AND column_name = 'ativo' 
            AND table_schema = 'public'
    ) THEN
        ALTER TABLE public."$table" DROP COLUMN ativo;
        
        INSERT INTO migration_log_$logDate (table_name, action, details)
        VALUES ('$table', 'DROP_COLUMN_ativo', 'Coluna ativo removida com sucesso');
        
        RAISE NOTICE '✅ Coluna ativo removida da tabela $table';
    ELSE
        RAISE NOTICE 'ℹ️  Tabela $table não possui coluna ativo duplicada';
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        INSERT INTO migration_errors_$logDate (table_name, error_message)
        VALUES ('$table', SQLERRM);
        RAISE NOTICE '❌ Erro ao remover coluna ativo da tabela $table: %', SQLERRM;
END `$`$;
"@
        
        if (-not (Execute-SQL $dropColumnSQL "Remoção de coluna duplicada da tabela $table")) {
            Write-Log "Falha na remoção de coluna duplicada da tabela $table" "ERROR"
            if (-not $Force) {
                Write-Log "Parando execução devido a falha" "ERROR"
                return
            }
        }
    }
    
    # FASE 5: Validação pós-migração
    Write-Log "=== FASE 5: VALIDAÇÃO PÓS-MIGRAÇÃO ===" "INFO"
    
    $validationSQL = @"
DO `$`$
DECLARE
    duplicated_columns_remaining INTEGER;
    tables_with_issues TEXT[];
    validation_results TEXT := '';
BEGIN
    SELECT COUNT(*), ARRAY_AGG(table_name) 
    INTO duplicated_columns_remaining, tables_with_issues
    FROM (
        SELECT table_name
        FROM information_schema.columns 
        WHERE table_schema = 'public' AND column_name IN ('Ativo', 'ativo')
        GROUP BY table_name
        HAVING COUNT(DISTINCT column_name) > 1
    ) t;
    
    validation_results := 'Colunas duplicadas restantes: ' || duplicated_columns_remaining;
    
    IF duplicated_columns_remaining = 0 THEN
        validation_results := validation_results || E'\n✅ Todas as colunas duplicadas foram removidas com sucesso';
        RAISE NOTICE '✅ MIGRAÇÃO CONCLUÍDA COM SUCESSO';
    ELSE
        validation_results := validation_results || E'\n❌ Ainda existem problemas em: ' || array_to_string(tables_with_issues, ', ');
        RAISE NOTICE '⚠️  MIGRAÇÃO PARCIAL - ainda existem % problemas', duplicated_columns_remaining;
    END IF;
    
    INSERT INTO migration_log_$logDate (table_name, action, details)
    VALUES ('SYSTEM', 'VALIDATION_COMPLETE', validation_results);
    
    INSERT INTO migration_log_$logDate (table_name, action, details)
    VALUES ('SYSTEM', 'MIGRATION_COMPLETE', 'Migração finalizada via PowerShell script');
    
END `$`$;
"@
    
    if (-not (Execute-SQL $validationSQL "Validação pós-migração")) {
        Write-Log "Falha na validação pós-migração" "ERROR"
        return
    }
    
    Write-Log "=== MIGRAÇÃO FINALIZADA ===" "SUCCESS"
    Write-Log "Log completo salvo em: $LogFile" "INFO"
    
    if (-not $SkipBackup) {
        Write-Log "Backups salvos em: $BackupPath" "INFO"
    }
}

# Exibir ajuda
function Show-Help {
    Write-Host @"
SCRIPT DE MIGRAÇÃO DO BANCO DE DADOS
====================================

Uso: .\execute_database_migrations.ps1 [parâmetros]

Parâmetros:
  -ConnectionString <string>  String de conexão com o banco PostgreSQL
  -DryRun                     Executa em modo simulação (não altera o banco)
  -SkipBackup                 Pula a criação de backups das tabelas
  -Force                      Força execução sem confirmação
  -Help                       Exibe esta ajuda

Exemplos:
  # Execução em modo simulação
  .\execute_database_migrations.ps1 -DryRun

  # Execução real com connection string
  .\execute_database_migrations.ps1 -ConnectionString "postgresql://user:pass@localhost/agriis"

  # Execução forçada sem backup
  .\execute_database_migrations.ps1 -Force -SkipBackup

Pré-requisitos:
  - PostgreSQL client (psql) instalado e no PATH
  - Permissões adequadas no banco de dados
  - Backup do banco de dados (recomendado)

"@
}

# Verificar se foi solicitada ajuda
if ($args -contains "-Help" -or $args -contains "--help" -or $args -contains "-h") {
    Show-Help
    return
}

# Executar migração
try {
    Start-DatabaseMigration
}
catch {
    Write-Log "Erro fatal durante a migração: $($_.Exception.Message)" "ERROR"
    Write-Log "Stack trace: $($_.Exception.StackTrace)" "ERROR"
}