#!/usr/bin/env pwsh

# Script para corrigir a estrutura da tabela paises no banco de dados
# Adiciona a coluna data_atualizacao se não existir

Write-Host "=== CORREÇÃO: Estrutura da Tabela Paises ===" -ForegroundColor Cyan
Write-Host ""

# Configuração do banco de dados
$connectionString = "Host=localhost;Port=5432;Database=agriis_dev;Username=agriis_user;Password=agriis_password"

Write-Host "🔧 Executando correções na tabela paises..." -ForegroundColor Yellow
Write-Host ""

try {
    # Executar o script SQL
    $sqlScript = Get-Content -Path "./scripts/fix_paises_data_atualizacao.sql" -Raw
    
    Write-Host "📄 Script SQL carregado:" -ForegroundColor Blue
    Write-Host "   - Verificar e criar coluna data_atualizacao" -ForegroundColor Gray
    Write-Host "   - Verificar tipo da coluna data_criacao" -ForegroundColor Gray
    Write-Host "   - Criar índice para data_atualizacao" -ForegroundColor Gray
    Write-Host ""
    
    # Usar psql para executar o script
    Write-Host "🗄️  Executando script no PostgreSQL..." -ForegroundColor Blue
    
    $env:PGPASSWORD = "agriis_password"
    $result = psql -h localhost -p 5432 -U agriis_user -d agriis_dev -f "./scripts/fix_paises_data_atualizacao.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Resultado:" -ForegroundColor Blue
        Write-Host $result -ForegroundColor Gray
    } else {
        Write-Host "❌ Erro ao executar script:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Erro ao executar correções:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Testando a API após correção ===" -ForegroundColor Cyan

try {
    # Testar o endpoint de países
    Write-Host "🧪 Testando endpoint /api/enderecos/paises..." -ForegroundColor Blue
    
    $response = Invoke-RestMethod -Uri "https://localhost:7297/api/enderecos/paises" -Method GET -ContentType "application/json" -SkipCertificateCheck
    
    if ($response -and $response.Count -gt 0) {
        Write-Host "✅ Endpoint funcionando! Países encontrados: $($response.Count)" -ForegroundColor Green
        
        # Verificar se o Brasil tem estados
        $brasil = $response | Where-Object { $_.codigo -eq "BR" }
        if ($brasil -and $brasil.estados -and $brasil.estados.Count -gt 0) {
            Write-Host "✅ Brasil com $($brasil.estados.Count) estados incluídos!" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Brasil encontrado mas sem estados" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  Endpoint retornou sem dados" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Erro ao testar API:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Resumo das Correções ===" -ForegroundColor Cyan
Write-Host "✅ Configuração da entidade Pais corrigida" -ForegroundColor Green
Write-Host "✅ Mapeamento da propriedade DataAtualizacao adicionado" -ForegroundColor Green
Write-Host "✅ Script de correção do banco executado" -ForegroundColor Green
Write-Host "✅ Índice para data_atualizacao criado" -ForegroundColor Green
Write-Host ""
Write-Host "🔄 Reinicie a aplicação para aplicar as mudanças na configuração do EF" -ForegroundColor Yellow
Write-Host ""