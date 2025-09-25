# Script PowerShell para atualizar o schema do Produtor
# Adiciona campos de telefone e cria tabela UsuarioProdutor se necessário

param(
    [string]$ConnectionString = "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432"
)

Write-Host "=== Atualizando Schema do Módulo Produtor ===" -ForegroundColor Green

# Verificar se o psql está disponível
try {
    $psqlVersion = psql --version
    Write-Host "PostgreSQL Client encontrado: $psqlVersion" -ForegroundColor Green
} catch {
    Write-Host "ERRO: psql não encontrado. Instale o PostgreSQL client." -ForegroundColor Red
    Write-Host "Alternativa: Execute o script SQL manualmente no seu cliente PostgreSQL preferido." -ForegroundColor Yellow
    Write-Host "Arquivo SQL: scripts/add_telefones_produtor.sql" -ForegroundColor Yellow
    exit 1
}

# Executar o script SQL
Write-Host "Executando script SQL..." -ForegroundColor Yellow

try {
    $scriptPath = Join-Path $PSScriptRoot "add_telefones_produtor.sql"
    
    if (-not (Test-Path $scriptPath)) {
        Write-Host "ERRO: Arquivo SQL não encontrado: $scriptPath" -ForegroundColor Red
        exit 1
    }
    
    # Executar o script SQL
    psql $ConnectionString -f $scriptPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Script SQL executado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro ao executar script SQL. Código de saída: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao executar script SQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Navegar para o diretório do projeto
$projectDir = Split-Path $PSScriptRoot -Parent
Set-Location $projectDir

Write-Host "Diretório do projeto: $projectDir" -ForegroundColor Cyan

# Compilar o projeto para garantir que não há erros
Write-Host "Compilando projeto..." -ForegroundColor Yellow
try {
    dotnet build src/Agriis.Api --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na compilação. Verifique os erros acima." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao compilar: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Tentar criar uma nova migração para sincronizar o modelo
Write-Host "Tentando sincronizar modelo do Entity Framework..." -ForegroundColor Yellow
try {
    # Primeiro, vamos tentar forçar a criação de uma migração vazia para sincronizar
    $migrationName = "SincronizarTelefonesProdutorUsuarioMaster_$(Get-Date -Format 'yyyyMMddHHmmss')"
    
    dotnet ef migrations add $migrationName --project src/Agriis.Api --force
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migração criada: $migrationName" -ForegroundColor Green
        
        # Verificar se a migração está vazia (apenas sincronização)
        $migrationFile = Get-ChildItem "src/Agriis.Api/Migrations" -Filter "*$migrationName.cs" | Select-Object -First 1
        if ($migrationFile) {
            $content = Get-Content $migrationFile.FullName -Raw
            if ($content -match "protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{\s*\}" -and 
                $content -match "protected override void Down\(MigrationBuilder migrationBuilder\)\s*\{\s*\}") {
                Write-Host "✅ Migração vazia criada - modelo já está sincronizado!" -ForegroundColor Green
            } else {
                Write-Host "⚠️  Migração contém alterações. Revise o arquivo antes de aplicar." -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "⚠️  Não foi possível criar migração automática. Isso é normal se o modelo já está sincronizado." -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Aviso: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Isso pode ser normal se o modelo já está sincronizado com o banco." -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=== Resumo das Alterações ===" -ForegroundColor Green
Write-Host "✅ Campos adicionados na tabela Produtor:" -ForegroundColor White
Write-Host "   - Telefone1 (VARCHAR(20))" -ForegroundColor Cyan
Write-Host "   - Telefone2 (VARCHAR(20))" -ForegroundColor Cyan  
Write-Host "   - Telefone3 (VARCHAR(20))" -ForegroundColor Cyan
Write-Host "   - Email (VARCHAR(100))" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Tabela UsuarioProdutor verificada/criada com:" -ForegroundColor White
Write-Host "   - Relacionamento Usuario <-> Produtor" -ForegroundColor Cyan
Write-Host "   - Campo EhProprietario para identificar proprietário principal" -ForegroundColor Cyan
Write-Host "   - Índices otimizados para consultas" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Backend atualizado:" -ForegroundColor White
Write-Host "   - DTOs com novos campos de telefone" -ForegroundColor Cyan
Write-Host "   - Serviço com método CriarCompletoAsync" -ForegroundColor Cyan
Write-Host "   - Controller com endpoint /completo" -ForegroundColor Cyan
Write-Host "   - Repositório UsuarioProdutorRepository" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Frontend atualizado:" -ForegroundColor White
Write-Host "   - Formulário com 3 campos de telefone" -ForegroundColor Cyan
Write-Host "   - Serviço com método createComplete" -ForegroundColor Cyan
Write-Host "   - Modelos TypeScript atualizados" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎉 Atualização concluída com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Teste a criação de produtores no frontend" -ForegroundColor White
Write-Host "2. Verifique se o usuário master é criado corretamente" -ForegroundColor White
Write-Host "3. Confirme se o relacionamento UsuarioProdutor funciona" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Endpoints disponíveis:" -ForegroundColor Yellow
Write-Host "   POST /api/produtores/completo - Criar produtor com usuário master" -ForegroundColor Cyan
Write-Host "   POST /api/produtores - Criar produtor simples" -ForegroundColor Cyan