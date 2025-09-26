# Adição de Campos de Dimensões na Tabela Produto

## Objetivo
Sincronizar a tabela `Produto` do banco de dados com o mapeamento da API .NET, adicionando os campos de dimensões físicas que estavam faltando.

## Problema Identificado
A configuração do Entity Framework (`ProdutoConfiguration.cs`) estava mapeando campos de dimensões que não existiam no banco de dados:
- `Altura` (numeric(10,2))
- `Largura` (numeric(10,2)) 
- `Comprimento` (numeric(10,2))
- `PesoNominal` (numeric(10,3))

## Arquivos Criados

### 1. `adicionar_campos_dimensoes_produto.sql`
Script SQL principal que:
- Adiciona as 4 colunas faltantes na tabela `Produto`
- Define valores padrão apropriados
- Cria índices para otimização
- Atualiza produtos existentes com valores calculados baseados no peso da embalagem
- Inclui verificações de validação

### 2. `executar_adicao_dimensoes_produto.ps1`
Script PowerShell para execução segura que:
- Faz backup da estrutura da tabela antes das alterações
- Verifica conexão com o banco
- Valida se a tabela existe
- Verifica campos conflitantes
- Executa o script SQL
- Valida se as alterações foram aplicadas corretamente
- Suporte para modo dry-run (`-DryRun`)

### 3. `rollback_dimensoes_produto.sql`
Script de rollback para reverter as alterações se necessário:
- Remove as colunas adicionadas
- Remove os índices criados
- Inclui verificações de segurança

## Como Usar

### Execução Normal
```powershell
# Navegar para o diretório de scripts
cd nova_api/scripts

# Executar com connection string padrão
.\executar_adicao_dimensoes_produto.ps1

# Executar com connection string customizada
.\executar_adicao_dimensoes_produto.ps1 -ConnectionString "Host=seu-host;Database=seu-db;Username=user;Password=pass"
```

### Modo Dry-Run (apenas validação)
```powershell
.\executar_adicao_dimensoes_produto.ps1 -DryRun
```

### Execução Manual do SQL
```bash
# Usando psql diretamente
psql "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123" -f adicionar_campos_dimensoes_produto.sql
```

### Rollback (se necessário)
```bash
psql "sua-connection-string" -f rollback_dimensoes_produto.sql
```

## Campos Adicionados

| Campo | Tipo | Padrão | Descrição |
|-------|------|--------|-----------|
| `Altura` | numeric(10,2) | 0 | Altura do produto em centímetros |
| `Largura` | numeric(10,2) | 0 | Largura do produto em centímetros |
| `Comprimento` | numeric(10,2) | 0 | Comprimento do produto em centímetros |
| `PesoNominal` | numeric(10,3) | 0 | Peso nominal do produto em quilogramas |

## Lógica de Valores Iniciais

Para produtos existentes, os valores são calculados baseados no `PesoEmbalagem`:

- **Produtos leves** (≤ 1kg): 10×15×20 cm
- **Produtos médios** (1-5kg): 15×25×30 cm  
- **Produtos pesados** (>5kg): 20×35×40 cm
- **PesoNominal**: Igual ao `PesoEmbalagem` inicialmente

## Índices Criados

- `IX_Produto_Dimensoes`: Índice composto em (Altura, Largura, Comprimento)
- `IX_Produto_PesoNominal`: Índice em PesoNominal

## Validação Pós-Execução

Após executar o script, verifique:

1. **Estrutura das colunas**:
```sql
SELECT column_name, data_type, numeric_precision, numeric_scale, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'Produto' AND table_schema = 'public'
    AND column_name IN ('Altura', 'Largura', 'Comprimento', 'PesoNominal');
```

2. **Dados atualizados**:
```sql
SELECT "Id", "Nome", "PesoEmbalagem", "Altura", "Largura", "Comprimento", "PesoNominal"
FROM public."Produto" 
LIMIT 10;
```

3. **Teste da API**: Execute a aplicação .NET e verifique se não há erros de mapeamento.

## Próximos Passos

1. ✅ Executar o script de migração
2. ⏳ Testar a API .NET para verificar mapeamento
3. ⏳ Executar testes de integração
4. ⏳ Atualizar dados de produtos com dimensões reais (se disponível)
5. ⏳ Considerar criar migration do Entity Framework para futuras alterações

## Backup e Segurança

- O script PowerShell cria backup automático da estrutura da tabela
- Sempre teste em ambiente de desenvolvimento primeiro
- Mantenha backups dos dados antes de executar em produção
- O rollback está disponível caso necessário

## Troubleshooting

### Erro: "psql não encontrado"
Instale o PostgreSQL client ou adicione ao PATH do sistema.

### Erro: "Conexão recusada"
Verifique se o PostgreSQL está rodando e a connection string está correta.

### Erro: "Campos já existem"
O script verifica campos existentes. Confirme se deseja continuar ou use o rollback primeiro.

### Erro de permissão
Certifique-se de que o usuário do banco tem permissões para ALTER TABLE.