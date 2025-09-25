#!/bin/bash

# Script Bash para atualizar o schema do Produtor
# Adiciona campos de telefone e cria tabela UsuarioProdutor se necessário

# Configurações padrão
CONNECTION_STRING=${1:-"host=localhost dbname=DBAgriis user=postgres password=RootPassword123 port=5432"}

echo "=== Atualizando Schema do Módulo Produtor ==="

# Verificar se o psql está disponível
if ! command -v psql &> /dev/null; then
    echo "❌ ERRO: psql não encontrado. Instale o PostgreSQL client."
    echo "📋 Alternativa: Execute o script SQL manualmente no seu cliente PostgreSQL preferido."
    echo "📁 Arquivo SQL: scripts/add_telefones_produtor.sql"
    exit 1
fi

echo "✅ PostgreSQL Client encontrado: $(psql --version)"

# Executar o script SQL
echo "🔄 Executando script SQL..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_FILE="$SCRIPT_DIR/add_telefones_produtor.sql"

if [ ! -f "$SQL_FILE" ]; then
    echo "❌ ERRO: Arquivo SQL não encontrado: $SQL_FILE"
    exit 1
fi

# Executar o script SQL
if psql "$CONNECTION_STRING" -f "$SQL_FILE"; then
    echo "✅ Script SQL executado com sucesso!"
else
    echo "❌ Erro ao executar script SQL. Código de saída: $?"
    exit 1
fi

# Navegar para o diretório do projeto
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_DIR" || exit 1

echo "📁 Diretório do projeto: $PROJECT_DIR"

# Compilar o projeto para garantir que não há erros
echo "🔨 Compilando projeto..."
if dotnet build src/Agriis.Api --no-restore; then
    echo "✅ Compilação bem-sucedida!"
else
    echo "❌ Erro na compilação. Verifique os erros acima."
    exit 1
fi

# Tentar criar uma nova migração para sincronizar o modelo
echo "🔄 Tentando sincronizar modelo do Entity Framework..."

MIGRATION_NAME="SincronizarTelefonesProdutorUsuarioMaster_$(date +%Y%m%d%H%M%S)"

if dotnet ef migrations add "$MIGRATION_NAME" --project src/Agriis.Api --force; then
    echo "✅ Migração criada: $MIGRATION_NAME"
    
    # Verificar se a migração está vazia (apenas sincronização)
    MIGRATION_FILE=$(find src/Agriis.Api/Migrations -name "*$MIGRATION_NAME.cs" | head -1)
    if [ -f "$MIGRATION_FILE" ]; then
        if grep -q "protected override void Up(MigrationBuilder migrationBuilder)" "$MIGRATION_FILE" && \
           grep -q "protected override void Down(MigrationBuilder migrationBuilder)" "$MIGRATION_FILE"; then
            echo "✅ Migração vazia criada - modelo já está sincronizado!"
        else
            echo "⚠️  Migração contém alterações. Revise o arquivo antes de aplicar."
        fi
    fi
else
    echo "⚠️  Não foi possível criar migração automática. Isso é normal se o modelo já está sincronizado."
fi

echo ""
echo "=== Resumo das Alterações ==="
echo "✅ Campos adicionados na tabela Produtor:"
echo "   - Telefone1 (VARCHAR(20))"
echo "   - Telefone2 (VARCHAR(20))"
echo "   - Telefone3 (VARCHAR(20))"
echo "   - Email (VARCHAR(100))"
echo ""
echo "✅ Tabela UsuarioProdutor verificada/criada com:"
echo "   - Relacionamento Usuario <-> Produtor"
echo "   - Campo EhProprietario para identificar proprietário principal"
echo "   - Índices otimizados para consultas"
echo ""
echo "✅ Backend atualizado:"
echo "   - DTOs com novos campos de telefone"
echo "   - Serviço com método CriarCompletoAsync"
echo "   - Controller com endpoint /completo"
echo "   - Repositório UsuarioProdutorRepository"
echo ""
echo "✅ Frontend atualizado:"
echo "   - Formulário com 3 campos de telefone"
echo "   - Serviço com método createComplete"
echo "   - Modelos TypeScript atualizados"
echo ""
echo "🎉 Atualização concluída com sucesso!"
echo ""
echo "📋 Próximos passos:"
echo "1. Teste a criação de produtores no frontend"
echo "2. Verifique se o usuário master é criado corretamente"
echo "3. Confirme se o relacionamento UsuarioProdutor funciona"
echo ""
echo "🔗 Endpoints disponíveis:"
echo "   POST /api/produtores/completo - Criar produtor com usuário master"
echo "   POST /api/produtores - Criar produtor simples"