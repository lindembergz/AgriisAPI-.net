#!/usr/bin/env pwsh

# Script para testar o endpoint de países com estados incluídos
# Verifica se a API retorna os países com seus respectivos estados

Write-Host "=== TESTE: Países com Estados Incluídos ===" -ForegroundColor Cyan
Write-Host ""

# Configuração da API
$baseUrl = "https://localhost:7297/api"
$endpoint = "$baseUrl/enderecos/paises"

Write-Host "🔍 Testando endpoint: $endpoint" -ForegroundColor Yellow
Write-Host ""

try {
    # Fazer requisição para obter países com estados
    Write-Host "📡 Fazendo requisição GET para obter países..." -ForegroundColor Blue
    
    $response = Invoke-RestMethod -Uri $endpoint -Method GET -ContentType "application/json" -SkipCertificateCheck
    
    Write-Host "✅ Requisição bem-sucedida!" -ForegroundColor Green
    Write-Host ""
    
    # Verificar se retornou dados
    if ($response -and $response.Count -gt 0) {
        Write-Host "📊 Países encontrados: $($response.Count)" -ForegroundColor Green
        Write-Host ""
        
        foreach ($pais in $response) {
            Write-Host "🌍 País: $($pais.nome) ($($pais.codigo))" -ForegroundColor Cyan
            Write-Host "   ID: $($pais.id)"
            Write-Host "   Ativo: $($pais.ativo)"
            Write-Host "   Total de Estados: $($pais.totalEstados)"
            
            if ($pais.estados -and $pais.estados.Count -gt 0) {
                Write-Host "   Estados incluídos: $($pais.estados.Count)" -ForegroundColor Green
                
                # Mostrar alguns estados como exemplo
                $estadosParaMostrar = $pais.estados | Select-Object -First 5
                foreach ($estado in $estadosParaMostrar) {
                    Write-Host "     - $($estado.nome) ($($estado.uf)) - Região: $($estado.regiao)" -ForegroundColor Gray
                }
                
                if ($pais.estados.Count -gt 5) {
                    Write-Host "     ... e mais $($pais.estados.Count - 5) estados" -ForegroundColor Gray
                }
            } else {
                Write-Host "   ⚠️  Nenhum estado incluído" -ForegroundColor Yellow
            }
            Write-Host ""
        }
        
        # Verificar se o Brasil está incluído e tem estados
        $brasil = $response | Where-Object { $_.codigo -eq "BR" }
        if ($brasil) {
            Write-Host "🇧🇷 Verificação específica do Brasil:" -ForegroundColor Green
            Write-Host "   Estados do Brasil: $($brasil.estados.Count)" -ForegroundColor Green
            
            if ($brasil.estados.Count -gt 0) {
                Write-Host "   ✅ Estados incluídos corretamente!" -ForegroundColor Green
                
                # Verificar se tem as informações necessárias
                $estadoExemplo = $brasil.estados[0]
                Write-Host "   Exemplo de estado: $($estadoExemplo.nome) ($($estadoExemplo.uf))" -ForegroundColor Gray
                Write-Host "     - ID: $($estadoExemplo.id)"
                Write-Host "     - Região: $($estadoExemplo.regiao)"
            } else {
                Write-Host "   ❌ Brasil sem estados - possível problema no mapeamento" -ForegroundColor Red
            }
        } else {
            Write-Host "⚠️  Brasil não encontrado na resposta" -ForegroundColor Yellow
        }
        
    } else {
        Write-Host "⚠️  Nenhum país encontrado" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Erro na requisição:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        Write-Host "Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Teste de Países Ativos com Estados ===" -ForegroundColor Cyan

try {
    $endpointAtivos = "$baseUrl/enderecos/paises/ativos"
    Write-Host "📡 Testando endpoint de países ativos: $endpointAtivos" -ForegroundColor Blue
    
    $responseAtivos = Invoke-RestMethod -Uri $endpointAtivos -Method GET -ContentType "application/json" -SkipCertificateCheck
    
    Write-Host "✅ Países ativos obtidos: $($responseAtivos.Count)" -ForegroundColor Green
    
    foreach ($pais in $responseAtivos) {
        Write-Host "🌍 $($pais.nome): $($pais.totalEstados) estados" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "❌ Erro ao obter países ativos:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Resumo do Teste ===" -ForegroundColor Cyan
Write-Host "✅ Endpoint testado: /api/enderecos/paises" -ForegroundColor Green
Write-Host "✅ Endpoint testado: /api/enderecos/paises/ativos" -ForegroundColor Green
Write-Host "📋 Verificar se os países incluem os estados corretamente" -ForegroundColor Yellow
Write-Host "📋 Verificar se o frontend pode usar esses dados sem requisições adicionais" -ForegroundColor Yellow
Write-Host ""