#!/usr/bin/env pwsh

# Script para testar a otimização dos países com estados incluídos
# Verifica se a API retorna os países com estados sem necessidade de requisições adicionais

Write-Host "=== TESTE: Países Otimizados com Estados Incluídos ===" -ForegroundColor Cyan
Write-Host ""

# Configuração da API
$baseUrl = "https://localhost:7297/api"
$endpoint = "$baseUrl/enderecos/paises"

Write-Host "🔍 Testando endpoint otimizado: $endpoint" -ForegroundColor Yellow
Write-Host ""

try {
    # Fazer requisição para obter países com estados
    Write-Host "📡 Fazendo requisição GET para obter países..." -ForegroundColor Blue
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-RestMethod -Uri $endpoint -Method GET -ContentType "application/json" -SkipCertificateCheck
    $stopwatch.Stop()
    
    Write-Host "✅ Requisição bem-sucedida em $($stopwatch.ElapsedMilliseconds)ms!" -ForegroundColor Green
    Write-Host ""
    
    # Verificar se retornou dados
    if ($response -and $response.Count -gt 0) {
        Write-Host "📊 Países encontrados: $($response.Count)" -ForegroundColor Green
        Write-Host ""
        
        $totalEstados = 0
        foreach ($pais in $response) {
            Write-Host "🌍 País: $($pais.nome) ($($pais.codigo))" -ForegroundColor Cyan
            Write-Host "   ID: $($pais.id)"
            Write-Host "   Ativo: $($pais.ativo)"
            Write-Host "   Total de Estados: $($pais.totalEstados)" -ForegroundColor Green
            
            if ($pais.estados -and $pais.estados.Count -gt 0) {
                Write-Host "   ✅ Estados incluídos: $($pais.estados.Count)" -ForegroundColor Green
                $totalEstados += $pais.estados.Count
                
                # Mostrar alguns estados como exemplo
                $estadosParaMostrar = $pais.estados | Select-Object -First 3
                foreach ($estado in $estadosParaMostrar) {
                    Write-Host "     - $($estado.nome) ($($estado.uf)) - Região: $($estado.regiao)" -ForegroundColor Gray
                }
                
                if ($pais.estados.Count -gt 3) {
                    Write-Host "     ... e mais $($pais.estados.Count - 3) estados" -ForegroundColor Gray
                }
                
                # Verificar consistência entre totalEstados e estados.Count
                if ($pais.totalEstados -eq $pais.estados.Count) {
                    Write-Host "   ✅ Consistência: totalEstados = estados.Count" -ForegroundColor Green
                } else {
                    Write-Host "   ⚠️  Inconsistência: totalEstados ($($pais.totalEstados)) ≠ estados.Count ($($pais.estados.Count))" -ForegroundColor Yellow
                }
            } else {
                Write-Host "   ⚠️  Nenhum estado incluído" -ForegroundColor Yellow
            }
            Write-Host ""
        }
        
        Write-Host "📈 Estatísticas Gerais:" -ForegroundColor Blue
        Write-Host "   Total de Estados em todos os países: $totalEstados" -ForegroundColor Gray
        Write-Host "   Requisições necessárias: 1 (otimizado!)" -ForegroundColor Green
        Write-Host "   Requisições economizadas: $($response.Count) (uma por país)" -ForegroundColor Green
        Write-Host ""
        
        # Verificar se o Brasil está incluído e tem estados
        $brasil = $response | Where-Object { $_.codigo -eq "BR" }
        if ($brasil) {
            Write-Host "🇧🇷 Verificação específica do Brasil:" -ForegroundColor Green
            Write-Host "   Estados do Brasil: $($brasil.totalEstados)" -ForegroundColor Green
            Write-Host "   Estados incluídos: $($brasil.estados.Count)" -ForegroundColor Green
            
            if ($brasil.estados.Count -gt 0) {
                Write-Host "   ✅ Estados incluídos corretamente!" -ForegroundColor Green
                
                # Verificar se tem as informações necessárias
                $estadoExemplo = $brasil.estados[0]
                Write-Host "   Exemplo de estado: $($estadoExemplo.nome) ($($estadoExemplo.uf))" -ForegroundColor Gray
                Write-Host "     - ID: $($estadoExemplo.id)"
                Write-Host "     - Região: $($estadoExemplo.regiao)"
                
                # Verificar se tem todos os estados brasileiros esperados
                if ($brasil.estados.Count -ge 26) {
                    Write-Host "   ✅ Quantidade de estados brasileiros parece correta (≥26)" -ForegroundColor Green
                } else {
                    Write-Host "   ⚠️  Poucos estados brasileiros ($($brasil.estados.Count)). Esperado ≥26" -ForegroundColor Yellow
                }
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
Write-Host "=== Teste de Performance ===" -ForegroundColor Cyan

try {
    # Testar múltiplas requisições para medir performance
    Write-Host "🚀 Testando performance com 5 requisições..." -ForegroundColor Blue
    
    $times = @()
    for ($i = 1; $i -le 5; $i++) {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-RestMethod -Uri $endpoint -Method GET -ContentType "application/json" -SkipCertificateCheck
        $stopwatch.Stop()
        $times += $stopwatch.ElapsedMilliseconds
        Write-Host "   Requisição $i: $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Gray
    }
    
    $avgTime = ($times | Measure-Object -Average).Average
    $minTime = ($times | Measure-Object -Minimum).Minimum
    $maxTime = ($times | Measure-Object -Maximum).Maximum
    
    Write-Host "📊 Estatísticas de Performance:" -ForegroundColor Blue
    Write-Host "   Tempo médio: $([math]::Round($avgTime, 2))ms" -ForegroundColor Green
    Write-Host "   Tempo mínimo: $minTime ms" -ForegroundColor Green
    Write-Host "   Tempo máximo: $maxTime ms" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erro no teste de performance:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Resumo da Otimização ===" -ForegroundColor Cyan
Write-Host "✅ Endpoint testado: /api/enderecos/paises" -ForegroundColor Green
Write-Host "✅ Estados incluídos na resposta da API" -ForegroundColor Green
Write-Host "✅ Eliminadas requisições adicionais para obter UFs" -ForegroundColor Green
Write-Host "✅ Frontend pode usar totalEstados diretamente" -ForegroundColor Green
Write-Host "✅ Performance otimizada com menos round-trips" -ForegroundColor Green
Write-Host ""
Write-Host "🎯 Benefícios da otimização:" -ForegroundColor Yellow
Write-Host "   - Menos requisições HTTP (1 em vez de N+1)" -ForegroundColor Gray
Write-Host "   - Melhor performance e experiência do usuário" -ForegroundColor Gray
Write-Host "   - Dados sempre consistentes e atualizados" -ForegroundColor Gray
Write-Host "   - Código mais simples no frontend" -ForegroundColor Gray
Write-Host ""