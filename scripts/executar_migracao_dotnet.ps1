# Script alternativo para executar a migração usando dotnet ef sql
# Este script gera o SQL da migration e permite executá-lo manualmente

param(
    [switch]$GenerateOnly = $false,
    [switch]$Execute = $false
)

$projectPath = Join-Path $PSScriptRoot "..\src\Agriis.Api"
$outputPath = Join-Path $PSScriptRoot "migration_sql_output.sql"

Write-Host "=== Migração da Tabela Produto - Dotnet EF ===" -ForegroundColor Cyan

try {
    # Navegar para o diretório do projeto
    Push-Location $projectPath
    
    if ($GenerateOnly -or -not $Execute) {
        Write-Host "Gerando SQL da migração..." -ForegroundColor Yellow
        
        # Gerar o SQL da migração
        $sqlOutput = & dotnet ef migrations script --output $outputPath --idempotent 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "SQL gerado com sucesso em: $outputPath" -ForegroundColor Green
            Write-Host "Conteúdo do SQL:" -ForegroundColor Cyan
            Get-Content $outputPath | Write-Host
        } else {
            Write-Host "Erro ao gerar SQL:" -ForegroundColor Red
            Write-Host $sqlOutput -ForegroundColor Red
        }
    }
    
    if ($Execute) {
        Write-Host "Executando migração..." -ForegroundColor Yellow
        
        # Tentar executar a migração
        $result = & dotnet ef database update --verbose 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migração executada com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "Erro ao executar migração:" -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            
            Write-Host "`nTentando abordagem alternativa..." -ForegroundColor Yellow
            
            # Se falhar, tentar executar apenas nossa migração específica
            $result2 = & dotnet ef database update 20250925120000_RefatorarProdutoParaReferencias --verbose 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Migração específica executada com sucesso!" -ForegroundColor Green
            } else {
                Write-Host "Erro na abordagem alternativa:" -ForegroundColor Red
                Write-Host $result2 -ForegroundColor Red
                
                Write-Host "`nUse o script SQL manual: .\executar_migracao_produto.ps1" -ForegroundColor Yellow
            }
        }
    }
}
catch {
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Pop-Location
}

if (-not $Execute) {
    Write-Host "`nPara executar a migração, use:" -ForegroundColor Yellow
    Write-Host "  .\executar_migracao_dotnet.ps1 -Execute" -ForegroundColor Cyan
    Write-Host "`nOu use o script SQL manual:" -ForegroundColor Yellow
    Write-Host "  .\executar_migracao_produto.ps1" -ForegroundColor Cyan
}