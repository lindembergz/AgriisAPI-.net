#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Testa as correções aplicadas ao CodigoIbge
.DESCRIPTION
    Script para validar se todas as correções do CodigoIbge foram aplicadas corretamente
#>

param(
    [string]$ProjectPath = "src/Agriis.Api"
)

Write-Host "Testando Correções do CodigoIbge" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# 1. Verificar se o projeto compila
Write-Host "`n1. Verificando compilação..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build $ProjectPath --no-restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✅ Projeto compila sem erros" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Erro de compilação detectado" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ❌ Erro ao compilar: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Verificar se as interfaces estão corretas
Write-Host "`n2. Verificando interfaces..." -ForegroundColor Yellow

$interfaceFile = "src/Modulos/Referencias/Agriis.Referencias.Dominio/Interfaces/IMunicipioRepository.cs"
if (Test-Path $interfaceFile) {
    $content = Get-Content $interfaceFile -Raw
    
    # Verificar se tem os métodos int e string
    if ($content -match "ObterPorCodigoIbgeAsync\(int codigoIbge" -and 
        $content -match "ObterPorCodigoIbgeAsync\(string codigoIbge") {
        Write-Host "   ✅ Interface IMunicipioRepository correta" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Interface IMunicipioRepository incorreta" -ForegroundColor Red
    }
    
    if ($content -match "ExisteCodigoIbgeAsync\(int codigoIbge" -and 
        $content -match "ExisteCodigoIbgeAsync\(string codigoIbge") {
        Write-Host "   ✅ Métodos ExisteCodigoIbgeAsync corretos" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Métodos ExisteCodigoIbgeAsync incorretos" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ Arquivo de interface não encontrado" -ForegroundColor Red
}

# 3. Verificar implementação do repositório
Write-Host "`n3. Verificando implementação do repositório..." -ForegroundColor Yellow

$repoFile = "src/Modulos/Referencias/Agriis.Referencias.Infraestrutura/Repositorios/MunicipioRepository.cs"
if (Test-Path $repoFile) {
    $content = Get-Content $repoFile -Raw
    
    # Verificar se tem conversão ToString() nos métodos de busca
    if ($content -match "CodigoIbge\.ToString\(\)\.StartsWith" -and 
        $content -match "CodigoIbge\.ToString\(\)\.Contains") {
        Write-Host "   ✅ Métodos de busca corrigidos" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Métodos de busca não corrigidos" -ForegroundColor Red
    }
    
    # Verificar se tem métodos de compatibilidade
    if ($content -match "TryParse") {
        Write-Host "   ✅ Métodos de compatibilidade implementados" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Métodos de compatibilidade não implementados" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ Arquivo de repositório não encontrado" -ForegroundColor Red
}

# 4. Verificar DTOs
Write-Host "`n4. Verificando DTOs..." -ForegroundColor Yellow

$dtoFile = "src/Modulos/Referencias/Agriis.Referencias.Aplicacao/DTOs/MunicipioDto.cs"
if (Test-Path $dtoFile) {
    $content = Get-Content $dtoFile -Raw
    
    if ($content -match "public int CodigoIbge") {
        Write-Host "   ✅ DTOs usam int CodigoIbge" -ForegroundColor Green
    } else {
        Write-Host "   ❌ DTOs não usam int CodigoIbge" -ForegroundColor Red
    }
    
    if ($content -match "Range\(1000000") {
        Write-Host "   ✅ Validação de 7 dígitos mantida" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Validação de 7 dígitos não encontrada" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ Arquivo de DTO não encontrado" -ForegroundColor Red
}

# 5. Verificar serviço
Write-Host "`n5. Verificando serviço..." -ForegroundColor Yellow

$serviceFile = "src/Modulos/Referencias/Agriis.Referencias.Aplicacao/Servicos/MunicipioService.cs"
if (Test-Path $serviceFile) {
    $content = Get-Content $serviceFile -Raw
    
    if ($content -match "ToString\(\)") {
        Write-Host "   ✅ Serviço usa conversão ToString() corretamente" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Serviço não usa conversão ToString()" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ Arquivo de serviço não encontrado" -ForegroundColor Red
}

Write-Host "`nVerificação de correções concluída!" -ForegroundColor Cyan
Write-Host "Consulte o arquivo CORRECOES_CODIGOIBGE_COMPLETAS.md para detalhes completos." -ForegroundColor Gray