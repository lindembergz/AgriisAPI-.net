# Agriis API - Sistema de Agronegócio

Sistema de agronegócio desenvolvido em C# .NET Core 9 que conecta produtores rurais e fornecedores de insumos agrícolas.

## Arquitetura

O projeto segue uma arquitetura de monólito modular com os seguintes princípios:

- **Arquitetura Limpa**: Separação clara entre camadas
- **Domain-Driven Design**: Modelagem baseada no domínio
- **Modularização**: Organização por contextos de domínio

## Estrutura do Projeto

```
src/
├── Agriis.Api/                          # API Web (Controllers, Middleware, Configuration)
├── Agriis.Compartilhado/                # Componentes compartilhados
│   ├── Agriis.Compartilhado.Dominio/    # Entidades base, interfaces, enums
│   ├── Agriis.Compartilhado.Aplicacao/  # Result patterns, behaviors
│   └── Agriis.Compartilhado.Infraestrutura/ # Repository base, logging, integrações
└── Modulos/                             # Módulos de domínio
    ├── Autenticacao/                    # Autenticação e autorização JWT
    ├── Produtores/                      # Gestão de produtores rurais
    ├── Fornecedores/                    # Gestão de fornecedores
    └── Pedidos/                         # Sistema de pedidos e propostas
```

## Tecnologias Utilizadas

- **.NET 9**: Framework principal
- **ASP.NET Core**: API Web
- **Entity Framework Core**: ORM
- **PostgreSQL**: Banco de dados principal
- **PostGIS**: Extensão para dados geoespaciais
- **JWT**: Autenticação
- **Serilog**: Logging estruturado
- **FluentValidation**: Validação de dados
- **AutoMapper**: Mapeamento de objetos
- **MediatR**: Padrão mediator
- **Hangfire**: Jobs em background
- **SignalR**: Notificações em tempo real
- **AWS SDK**: Integração com serviços AWS
- **Swagger/OpenAPI**: Documentação da API

## Configuração do Ambiente

### Pré-requisitos

- .NET 9 SDK
- PostgreSQL 13+
- PostGIS (extensão do PostgreSQL)

### Configuração do Banco de Dados

1. Instale o PostgreSQL e PostGIS
2. Crie os bancos de dados:
   ```sql
   CREATE DATABASE agriis_dev_db;
   CREATE DATABASE agriis_test_db;
   CREATE DATABASE agriis_dev_hangfire;
   CREATE DATABASE agriis_test_hangfire;
   ```
3. Habilite a extensão PostGIS:
   ```sql
   CREATE EXTENSION postgis;
   ```

### Configuração da Aplicação

1. Clone o repositório
2. Navegue até o diretório do projeto
3. Restaure as dependências:
   ```bash
   dotnet restore
   ```
4. Configure as strings de conexão no `appsettings.Development.json`
5. Execute as migrações do banco de dados:
   ```bash
   dotnet ef database update
   ```

## Executando a Aplicação

### Desenvolvimento
```bash
dotnet run --project src/Agriis.Api
```

### Build
```bash
dotnet build
```

### Testes
```bash
dotnet test
```

## Configuração por Ambiente

### Development
- Banco de dados local
- Logs detalhados
- CORS permissivo
- Configurações de desenvolvimento

### Testing
- Banco de dados de teste
- Logs mínimos
- Configurações otimizadas para testes

### Production
- Variáveis de ambiente para configurações sensíveis
- SSL obrigatório
- Logs estruturados com destinos externos
- Health checks habilitados

## Variáveis de Ambiente (Produção)

```bash
DB_HOST=localhost
DB_NAME=agriis_db
DB_USER=postgres
DB_PASSWORD=your_password
DB_PORT=5432
JWT_SECRET_KEY=your_jwt_secret_key
AWS_ACCESS_KEY=your_aws_access_key
AWS_SECRET_KEY=your_aws_secret_key
AWS_REGION=us-east-1
AWS_S3_BUCKET=your_s3_bucket
SERPRO_CONSUMER_KEY=your_serpro_key
SERPRO_CONSUMER_SECRET=your_serpro_secret
ALLOWED_HOSTS=your_domain.com
CORS_ALLOWED_ORIGINS=https://your_frontend.com
```

## Endpoints Principais

- `GET /` - Informações da API
- `GET /health` - Health check
- `GET /swagger` - Documentação da API (desenvolvimento)

## Logging

O sistema utiliza Serilog para logging estruturado com os seguintes destinos:

- **Console**: Para desenvolvimento
- **Arquivo**: Logs rotativos diários
- **PostgreSQL**: Para produção (opcional)

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.