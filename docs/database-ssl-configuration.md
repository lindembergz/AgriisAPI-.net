# Configuração SSL para PostgreSQL

## Visão Geral

Este documento descreve como configurar conexões SSL seguras com PostgreSQL para diferentes ambientes.

## Configurações por Ambiente

### Desenvolvimento
- SSL Mode: `Disable` - Para facilitar desenvolvimento local
- Trust Server Certificate: Não aplicável

### Produção
- SSL Mode: `Require` - Força conexão SSL
- Trust Server Certificate: `false` - Valida certificados
- Variáveis de ambiente para credenciais

## Strings de Conexão

### Desenvolvimento (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432;SSL Mode=Disable"
  }
}
```

### Produção (appsettings.Production.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Port=${DB_PORT};SSL Mode=Require;Trust Server Certificate=false"
  }
}
```

## Variáveis de Ambiente para Produção

Configure as seguintes variáveis de ambiente:

- `DB_HOST`: Endereço do servidor PostgreSQL
- `DB_NAME`: Nome do banco de dados
- `DB_USER`: Usuário do banco de dados
- `DB_PASSWORD`: Senha do banco de dados
- `DB_PORT`: Porta do PostgreSQL (padrão: 5432)

## Configurações Adicionais de Segurança

### Certificados SSL
Para produção, certifique-se de que:
1. O servidor PostgreSQL tenha certificados SSL válidos
2. Os certificados sejam de uma CA confiável
3. Configure `Trust Server Certificate=false` para validar certificados

### Timeout e Retry Policy
A configuração atual inclui:
- **Command Timeout**: 30s (desenvolvimento) / 60s (produção)
- **Retry Policy**: 3 tentativas com delay máximo de 30s
- **Connection Pooling**: Gerenciado automaticamente pelo Npgsql

## Monitoramento

### Health Checks
O sistema inclui health checks para:
- Conectividade com PostgreSQL
- Status do contexto do Entity Framework

Acesse `/health` para verificar o status da conexão.

### Logs
As conexões são logadas com diferentes níveis:
- **Desenvolvimento**: Logs detalhados incluindo dados sensíveis
- **Produção**: Logs básicos sem dados sensíveis

## Troubleshooting

### Problemas Comuns

1. **Erro de SSL**: Verifique se o PostgreSQL está configurado para aceitar conexões SSL
2. **Timeout**: Ajuste os valores de timeout conforme necessário
3. **Certificados**: Certifique-se de que os certificados SSL estão válidos

### Comandos Úteis

```bash
# Testar conexão SSL
psql "host=localhost port=5432 dbname=DBAgriis user=postgres sslmode=require"

# Verificar status SSL no PostgreSQL
SELECT * FROM pg_stat_ssl;
```

## Referências

- [Npgsql Connection Strings](https://www.npgsql.org/doc/connection-string-parameters.html)
- [PostgreSQL SSL Documentation](https://www.postgresql.org/docs/current/ssl-tcp.html)
- [Entity Framework Core Connection Strings](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings)