@echo off
echo ========================================
echo Executando script de criacao das tabelas de referencias
echo ========================================

REM Configuracoes do banco de dados
set DB_HOST=localhost
set DB_PORT=5432
set DB_NAME=DBAgriis
set DB_USER=postgres
set DB_PASSWORD=RootPassword123

echo Conectando ao banco de dados %DB_NAME%...
echo.

REM Executar o script SQL
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -f criar_tabelas_referencias.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Script executado com sucesso!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo Erro ao executar o script!
    echo Codigo de erro: %ERRORLEVEL%
    echo ========================================
)

echo.
echo Pressione qualquer tecla para continuar...
pause > nul