# Script para executar a migração da tabela Produto para usar referências
# Este script executa a migration SQL diretamente no banco de dados

param(
    [string]$ConnectionString = $null,
    [switch]$DryRun = $false
)

# Função para obter a connection string do appsettings.json
function Get-ConnectionString {
    $appsettingsPath = Join-Path $PSScriptRoot "..\src\Agriis.Api\appsettings.Development.json"
    
    if (-not (Test-Path $appsettingsPath)) {
        $appsettingsPath = Join-Path $PSScriptRoot "..\src\Agriis.Api\appsettings.json"
    }
    
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        return $appsettings.ConnectionStrings.DefaultConnection
    }
    
    throw "Não foi possível encontrar o arquivo appsettings.json"
}

# Função para executar SQL no PostgreSQL
function Invoke-PostgreSql {
    param(
        [string]$ConnectionString,
        [string]$SqlScript,
        [switch]$DryRun
    )
    
    if ($DryRun) {
        Write-Host "DRY RUN - SQL que seria executado:" -ForegroundColor Yellow
        Write-Host $SqlScript -ForegroundColor Cyan
        return
    }
    
    # Verificar se o psql está disponível
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        throw "psql não encontrado. Instale o PostgreSQL client ou adicione ao PATH."
    }
    
    # Executar o script SQL
    $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
    try {
        $SqlScript | Out-File -FilePath $tempFile -Encoding UTF8
        
        Write-Host "Executando migração..." -ForegroundColor Green
        & psql $ConnectionString -f $tempFile
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migração executada com sucesso!" -ForegroundColor Green
        } else {
            throw "Erro ao executar a migração. Código de saída: $LASTEXITCODE"
        }
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile
        }
    }
}

# Função para verificar se a migração já foi aplicada
function Test-MigrationApplied {
    param([string]$ConnectionString)
    
    $checkSql = @"
SELECT COUNT(*) FROM "__EFMigrationsHistory" 
WHERE "MigrationId" = '20250925120000_RefatorarProdutoParaReferencias';
"@
    
    $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
    try {
        $checkSql | Out-File -FilePath $tempFile -Encoding UTF8
        $result = & psql $ConnectionString -t -A -f $tempFile
        return [int]$result -gt 0
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile
        }
    }
}

# Script principal
try {
    Write-Host "=== Migração da Tabela Produto para Referências ===" -ForegroundColor Cyan
    
    # Obter connection string
    if (-not $ConnectionString) {
        Write-Host "Obtendo connection string do appsettings.json..." -ForegroundColor Yellow
        $ConnectionString = Get-ConnectionString
    }
    
    Write-Host "Connection String: $($ConnectionString -replace 'Password=[^;]*', 'Password=***')" -ForegroundColor Gray
    
    # Verificar se a migração já foi aplicada
    if (-not $DryRun) {
        Write-Host "Verificando se a migração já foi aplicada..." -ForegroundColor Yellow
        if (Test-MigrationApplied -ConnectionString $ConnectionString) {
            Write-Host "A migração já foi aplicada anteriormente." -ForegroundColor Green
            return
        }
    }
    
    # Ler o script SQL
    $sqlScriptPath = Join-Path $PSScriptRoot "migrar_produto_referencias.sql"
    if (-not (Test-Path $sqlScriptPath)) {
        throw "Arquivo SQL não encontrado: $sqlScriptPath"
    }
    
    $sqlScript = Get-Content $sqlScriptPath -Raw
    
    # Executar a migração
    Invoke-PostgreSql -ConnectionString $ConnectionString -SqlScript $sqlScript -DryRun:$DryRun
    
    if (-not $DryRun) {
        Write-Host "Migração concluída com sucesso!" -ForegroundColor Green
        Write-Host "A tabela Produto agora usa referências para UnidadeMedida, Embalagem e AtividadeAgropecuaria." -ForegroundColor Green
    }
}
catch {
    Write-Host "Erro durante a migração: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}