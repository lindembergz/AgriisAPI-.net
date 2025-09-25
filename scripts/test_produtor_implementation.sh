#!/bin/bash

# Script de teste para validar a implementação do módulo Produtor
# Testa a API e verifica se os novos campos estão funcionando

API_URL=${1:-"https://localhost:7001"}
TEST_EMAIL=${2:-"produtor.teste@agriis.com"}
TEST_PASSWORD=${3:-"Teste123!"}

echo "=== Testando Implementação do Módulo Produtor ==="

# Função para fazer requisições HTTP
make_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local url="$API_URL$endpoint"
    
    if [ -n "$data" ]; then
        curl -s -k -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -d "$data" \
            -w "\nHTTP_CODE:%{http_code}"
    else
        curl -s -k -X "$method" "$url" \
            -w "\nHTTP_CODE:%{http_code}"
    fi
}

# Teste 1: Verificar se a API está rodando
echo ""
echo "🔍 Teste 1: Verificando se a API está acessível..."
response=$(make_request "GET" "/")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ API está acessível"
    echo "   Resposta: $body"
else
    echo "❌ API não está acessível (HTTP $http_code)"
    echo "📋 Verifique se a API está rodando em: $API_URL"
    exit 1
fi

# Teste 2: Listar produtores existentes
echo ""
echo "🔍 Teste 2: Listando produtores existentes..."
response=$(make_request "GET" "/api/produtores")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "200" ]; then
    echo "✅ Endpoint de listagem funcionando"
    echo "   Resposta: $body"
else
    echo "❌ Erro ao listar produtores (HTTP $http_code)"
    echo "   Resposta: $body"
fi

# Teste 3: Criar um produtor completo com os novos campos
echo ""
echo "🔍 Teste 3: Criando produtor completo com novos campos..."

novo_produtor_completo='{
    "nome": "Produtor Teste Completo",
    "cpfCnpj": "12345678901",
    "tipoCliente": "PF",
    "telefone1": "(11) 99999-1111",
    "telefone2": "(11) 99999-2222", 
    "telefone3": "(11) 99999-3333",
    "email": "'$TEST_EMAIL'",
    "areaPlantio": 100.5,
    "culturas": [1, 2],
    "usuarioMaster": {
        "nome": "Usuario Master Teste",
        "email": "'$TEST_EMAIL'",
        "senha": "'$TEST_PASSWORD'",
        "telefone": "(11) 99999-0000",
        "cpf": "98765432100"
    }
}'

response=$(make_request "POST" "/api/produtores/completo" "$novo_produtor_completo")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "201" ] || [ "$http_code" = "200" ]; then
    echo "✅ Produtor completo criado com sucesso!"
    echo "   Resposta: $body"
    
    # Extrair ID do produtor (assumindo formato JSON simples)
    produtor_id=$(echo "$body" | grep -o '"id":[0-9]*' | cut -d: -f2)
    
    if [ -n "$produtor_id" ]; then
        # Teste 4: Buscar o produtor criado
        echo ""
        echo "🔍 Teste 4: Verificando se os campos foram salvos (ID: $produtor_id)..."
        
        response=$(make_request "GET" "/api/produtores/$produtor_id")
        http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
        body=$(echo "$response" | sed '/HTTP_CODE:/d')
        
        if [ "$http_code" = "200" ]; then
            echo "✅ Produtor recuperado com sucesso!"
            
            # Verificar se os novos campos estão presentes
            if echo "$body" | grep -q '"telefone1"' && \
               echo "$body" | grep -q '"telefone2"' && \
               echo "$body" | grep -q '"telefone3"' && \
               echo "$body" | grep -q '"email"'; then
                echo "✅ Todos os novos campos estão presentes!"
            else
                echo "❌ Alguns campos novos estão faltando"
            fi
            
            echo "   Dados do produtor: $body"
        else
            echo "❌ Erro ao buscar produtor (HTTP $http_code)"
            echo "   Resposta: $body"
        fi
    fi
    
else
    echo "❌ Erro ao criar produtor completo (HTTP $http_code)"
    echo "   Resposta: $body"
    echo "📋 Verifique se:"
    echo "   - O banco de dados foi atualizado com os novos campos"
    echo "   - O serviço de usuários está funcionando"
    echo "   - As validações estão corretas"
fi

# Teste 5: Criar um produtor simples (método antigo)
echo ""
echo "🔍 Teste 5: Testando criação de produtor simples (compatibilidade)..."

novo_produtor_simples='{
    "nome": "Produtor Teste Simples",
    "cpf": "11122233344",
    "telefone1": "(11) 88888-1111",
    "telefone2": "(11) 88888-2222",
    "telefone3": "(11) 88888-3333", 
    "email": "produtor.simples@agriis.com",
    "areaPlantio": 50.0,
    "culturas": [1]
}'

response=$(make_request "POST" "/api/produtores" "$novo_produtor_simples")
http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d: -f2)
body=$(echo "$response" | sed '/HTTP_CODE:/d')

if [ "$http_code" = "201" ] || [ "$http_code" = "200" ]; then
    echo "✅ Produtor simples criado com sucesso!"
    echo "   Resposta: $body"
else
    echo "❌ Erro ao criar produtor simples (HTTP $http_code)"
    echo "   Resposta: $body"
fi

echo ""
echo "=== Resumo dos Testes ==="
echo "📊 Verifique os resultados acima para confirmar se tudo está funcionando"
echo ""
echo "📋 Para testar no frontend:"
echo "1. Acesse o formulário de criação de produtores"
echo "2. Preencha os 3 campos de telefone"
echo "3. Preencha os dados do usuário master"
echo "4. Submeta o formulário"
echo "5. Verifique se o produtor e usuário foram criados"
echo ""
echo "🔗 Endpoints testados:"
echo "   GET  /api/produtores - Listar produtores"
echo "   POST /api/produtores/completo - Criar produtor com usuário master"
echo "   POST /api/produtores - Criar produtor simples"
echo "   GET  /api/produtores/{id} - Buscar produtor por ID"