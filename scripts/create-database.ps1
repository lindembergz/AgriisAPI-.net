# Script PowerShell para criar o banco de dados DBAgriis
# Execute este script a partir da pasta raiz do projeto

param(
    [string]$Host = "localhost",
    [string]$Port = "5432",
    [string]$Username = "postgres",
    [string]$Password = "RootPassword123"
)

Write-Host "Criando banco de dados DBAgriis..." -ForegroundColor Green

# Definir variável de ambiente para senha
$env:PGPASSWORD = $Password

try {
    # Verificar se o banco já existe
    $checkDb = psql -h $Host -p $Port -U $Username -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname='DBAgriis';"
    
    if ($checkDb -match "1") {
        Write-Host "Banco de dados DBAgriis já existe!" -ForegroundColor Yellow
    } else {
        # Criar o banco de dados
        Write-Host "Criando banco de dados..." -ForegroundColor Blue
        psql -h $Host -p $Port -U $Username -d postgres -c "CREATE DATABASE \"DBAgriis\";"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Banco de dados DBAgriis criado com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "Erro ao criar banco de dados!" -ForegroundColor Red
            exit 1
        }
    }
    
    # Verificar conexão com o novo banco
    Write-Host "Testando conexão com o banco..." -ForegroundColor Blue
    $testConnection = psql -h $Host -p $Port -U $Username -d DBAgriis -c "SELECT version();"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Conexão com DBAgriis estabelecida com sucesso!" -ForegroundColor Green
        Write-Host "Você pode agora executar: dotnet ef database update" -ForegroundColor Cyan
    } else {
        Write-Host "Erro ao conectar com o banco DBAgriis!" -ForegroundColor Red
    }
    
} catch {
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "Script concluído." -ForegroundColor Green