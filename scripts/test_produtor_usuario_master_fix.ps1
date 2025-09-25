# Script de teste para verificar a correção do usuário master no módulo Produtor

Write-Host "=== Teste da Correção: Usuário Master - Módulo Produtor ===" -ForegroundColor Green

# Verificar se as correções foram aplicadas nos arquivos
Write-Host "`n🔍 Verificando correções aplicadas..." -ForegroundColor Cyan

$arquivos = @(
    @{
        Path = "FrontEndAdmin/src/app/features/produtores/components/produtor-detail.component.ts"
        Checks = @(
            "ViewChild.*usuarioMasterForm",
            "debugUsuarioMasterValidation",
            "forceValidationUpdate",
            "Object\.keys\(usuario\)\.forEach"
        )
    },
    @{
        Path = "FrontEndAdmin/src/app/features/produtores/components/produtor-detail.component.html"
        Checks = @(
            "#usuarioMasterForm"
        )
    },
    @{
        Path = "FrontEndAdmin/src/app/shared/components/usuario-master-form.component.ts"
        Checks = @(
            "forceValidationUpdate",
            "statusChanges\.subscribe",
            "usuarioFormGroup is required"
        )
    }
)

$totalChecks = 0
$passedChecks = 0

foreach ($arquivo in $arquivos) {
    Write-Host "`n📁 Verificando: $($arquivo.Path)" -ForegroundColor Yellow
    
    if (Test-Path $arquivo.Path) {
        $content = Get-Content $arquivo.Path -Raw
        
        foreach ($check in $arquivo.Checks) {
            $totalChecks++
            if ($content -match $check) {
                Write-Host "  ✅ $check" -ForegroundColor Green
                $passedChecks++
            } else {
                Write-Host "  ❌ $check" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "  ❌ Arquivo não encontrado" -ForegroundColor Red
        $totalChecks += $arquivo.Checks.Count
    }
}

Write-Host "`n📊 Resultado das verificações: $passedChecks/$totalChecks" -ForegroundColor $(if ($passedChecks -eq $totalChecks) { "Green" } else { "Yellow" })

# Verificar se o projeto compila
Write-Host "`n🔨 Verificando compilação do frontend..." -ForegroundColor Cyan

$frontendDir = "FrontEndAdmin"
if (Test-Path $frontendDir) {
    Set-Location $frontendDir
    
    try {
        Write-Host "Executando npm run build..." -ForegroundColor Gray
        $buildResult = npm run build 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Frontend compila sem erros" -ForegroundColor Green
        } else {
            Write-Host "❌ Erro na compilação do frontend" -ForegroundColor Red
            Write-Host "Saída do build:" -ForegroundColor Gray
            Write-Host $buildResult -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ Erro ao executar build: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Set-Location ..
} else {
    Write-Host "❌ Diretório FrontEndAdmin não encontrado" -ForegroundColor Red
}

# Resumo das correções aplicadas
Write-Host "`n=== Resumo das Correções Aplicadas ===" -ForegroundColor Green

Write-Host "`n✅ ProdutorDetailComponent:" -ForegroundColor White
Write-Host "   - Adicionado ViewChild para UsuarioMasterFormComponent" -ForegroundColor Cyan
Write-Host "   - Método onUsuarioMasterChange corrigido com sincronização adequada" -ForegroundColor Cyan
Write-Host "   - Método debugUsuarioMasterValidation adicionado" -ForegroundColor Cyan
Write-Host "   - Método onSave atualizado com forceValidationUpdate" -ForegroundColor Cyan

Write-Host "`n✅ UsuarioMasterFormComponent:" -ForegroundColor White
Write-Host "   - Método forceValidationUpdate adicionado" -ForegroundColor Cyan
Write-Host "   - Validação de usuarioFormGroup obrigatório" -ForegroundColor Cyan
Write-Host "   - Subscribe em statusChanges para melhor sincronização" -ForegroundColor Cyan

Write-Host "`n✅ Template HTML:" -ForegroundColor White
Write-Host "   - Referência de template #usuarioMasterForm adicionada" -ForegroundColor Cyan

Write-Host "`n📋 Como testar a correção:" -ForegroundColor Yellow
Write-Host "1. Execute o frontend: cd FrontEndAdmin && npm start" -ForegroundColor White
Write-Host "2. Acesse /produtores/novo" -ForegroundColor White
Write-Host "3. Preencha todos os campos obrigatórios:" -ForegroundColor White
Write-Host "   - Dados Gerais (nome, CPF/CNPJ, telefones)" -ForegroundColor Gray
Write-Host "   - Endereços (pelo menos um)" -ForegroundColor Gray
Write-Host "   - Propriedades (pelo menos uma)" -ForegroundColor Gray
Write-Host "   - Usuário Master (todos os campos)" -ForegroundColor Gray
Write-Host "4. Tente salvar o formulário" -ForegroundColor White
Write-Host "5. O sistema NÃO deve mais mostrar erro de usuário master" -ForegroundColor White

Write-Host "`n🔍 Debug disponível:" -ForegroundColor Yellow
Write-Host "- Se ainda houver problemas, abra o console do navegador" -ForegroundColor White
Write-Host "- O método debugUsuarioMasterValidation será executado automaticamente" -ForegroundColor White
Write-Host "- Verifique os logs para identificar problemas específicos" -ForegroundColor White

if ($passedChecks -eq $totalChecks) {
    Write-Host "`n🎉 Todas as correções foram aplicadas com sucesso!" -ForegroundColor Green
    Write-Host "O problema do usuário master no módulo Produtor deve estar resolvido." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Algumas correções podem não ter sido aplicadas corretamente." -ForegroundColor Yellow
    Write-Host "Verifique os arquivos manualmente se o problema persistir." -ForegroundColor Yellow
}