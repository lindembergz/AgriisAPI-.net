#!/bin/bash

# =====================================================
# SCRIPT BASH: CRIAR E APLICAR MIGRAÇÃO PARA CAMPO BAIRRO
# Data: $(date)
# Descrição: Cria e aplica migração do Entity Framework para adicionar campo Bairro
# =====================================================

echo "=== INICIANDO MIGRAÇÃO PARA CAMPO BAIRRO ==="

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Verificar se estamos no diretório correto
current_path=$(pwd)
echo -e "${YELLOW}Diretório atual: $current_path${NC}"

# Navegar para o diretório da API se necessário
if [ ! -d "src/Agriis.Api" ]; then
    if [ -d "nova_api/src/Agriis.Api" ]; then
        cd nova_api
        echo -e "${YELLOW}Navegando para o diretório nova_api${NC}"
    else
        echo -e "${RED}ERRO: Não foi possível encontrar o diretório da API${NC}"
        exit 1
    fi
fi

# Verificar se o dotnet está instalado
if command -v dotnet &> /dev/null; then
    dotnet_version=$(dotnet --version)
    echo -e "${GREEN}Versão do .NET: $dotnet_version${NC}"
else
    echo -e "${RED}ERRO: .NET CLI não encontrado. Instale o .NET SDK.${NC}"
    exit 1
fi

# Verificar se o Entity Framework Tools está instalado
if dotnet ef --version &> /dev/null; then
    echo -e "${GREEN}Entity Framework Tools encontrado${NC}"
else
    echo -e "${YELLOW}Instalando Entity Framework Tools...${NC}"
    dotnet tool install --global dotnet-ef
fi

echo -e "\n${GREEN}=== CRIANDO MIGRAÇÃO ===${NC}"

# Criar a migração
migration_name="AddBairroToFornecedor_$(date +%Y%m%d_%H%M%S)"
echo -e "${YELLOW}Nome da migração: $migration_name${NC}"

if dotnet ef migrations add "$migration_name" --project src/Agriis.Api --verbose; then
    echo -e "${GREEN}Migração criada com sucesso!${NC}"
else
    echo -e "${RED}ERRO ao criar migração${NC}"
    exit 1
fi

echo -e "\n${GREEN}=== APLICANDO MIGRAÇÃO ===${NC}"

# Aplicar a migração
if dotnet ef database update --project src/Agriis.Api --verbose; then
    echo -e "${GREEN}Migração aplicada com sucesso!${NC}"
else
    echo -e "${RED}ERRO ao aplicar migração${NC}"
    echo -e "${YELLOW}Você pode tentar aplicar manualmente com: dotnet ef database update --project src/Agriis.Api${NC}"
    exit 1
fi

echo -e "\n${GREEN}=== VALIDANDO MIGRAÇÃO ===${NC}"

# Executar script de validação SQL se existir
validation_script="scripts/add_bairro_fornecedor_migration.sql"
if [ -f "$validation_script" ]; then
    echo -e "${YELLOW}Executando validação SQL...${NC}"
    
    # Aqui você pode adicionar a lógica para executar o script SQL de validação
    # Por exemplo, usando psql se estiver disponível
    if command -v psql &> /dev/null; then
        echo -e "${YELLOW}psql encontrado. Você pode executar o script de validação manualmente:${NC}"
        echo -e "${WHITE}psql -d sua_database -f $validation_script${NC}"
    else
        echo -e "${YELLOW}Script de validação disponível em: $validation_script${NC}"
        echo -e "${YELLOW}Execute manualmente para validar a migração.${NC}"
    fi
else
    echo -e "${YELLOW}Script de validação não encontrado${NC}"
fi

echo -e "\n${GREEN}=== PRÓXIMOS PASSOS ===${NC}"
echo -e "${WHITE}1. Compile o projeto: dotnet build${NC}"
echo -e "${WHITE}2. Execute os testes: dotnet test${NC}"
echo -e "${WHITE}3. Teste a API para verificar se o campo Bairro está funcionando${NC}"
echo -e "${WHITE}4. Atualize o frontend se necessário${NC}"

echo -e "\n${GREEN}=== MIGRAÇÃO CONCLUÍDA COM SUCESSO! ===${NC}"

# Opcional: Compilar o projeto para verificar se não há erros
read -p "Deseja compilar o projeto agora? (s/n): " compile
if [[ $compile == "s" || $compile == "S" ]]; then
    echo -e "\n${YELLOW}Compilando projeto...${NC}"
    if dotnet build src/Agriis.Api; then
        echo -e "${GREEN}Compilação concluída com sucesso!${NC}"
    else
        echo -e "${RED}ERRO na compilação${NC}"
    fi
fi

echo -e "\n${GREEN}Script concluído!${NC}"