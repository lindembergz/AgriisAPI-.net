@echo off
echo Executando migration manual para adicionar campos de endereco ao Fornecedor...
echo.

REM Configurar variáveis de conexão
set PGHOST=localhost
set PGPORT=5432
set PGDATABASE=DBAgriis
set PGUSER=postgres

echo Conectando ao PostgreSQL...
psql -f adicionar_campos_endereco_fornecedor.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Migration executada com sucesso!
    echo Os seguintes campos foram adicionados à tabela Fornecedor:
    echo - Municipio (varchar 100)
    echo - Uf (varchar 2)
    echo - Cep (varchar 10)
    echo - Complemento (varchar 200)
    echo - Latitude (numeric 10,8)
    echo - Longitude (numeric 11,8)
) else (
    echo.
    echo ❌ Erro ao executar migration!
    echo Verifique se o PostgreSQL está rodando e as credenciais estão corretas.
)

echo.
pause