# Estrutura do Projeto Agriis

## Visão Geral da Solução

A solução Agriis foi estruturada seguindo os princípios de arquitetura limpa e monólito modular, organizando o código em contextos de domínio bem definidos.

## Estrutura de Diretórios

```
nova_api/
├── Agriis.sln                           # Arquivo de solução
├── README.md                            # Documentação principal
├── STRUCTURE.md                         # Este arquivo
├── .gitignore                           # Arquivos ignorados pelo Git
│
├── src/                                 # Código fonte
│   ├── Agriis.Api/                      # Projeto principal da API
│   │   ├── Controllers/                 # Controllers da API (a serem criados)
│   │   ├── Middleware/                  # Middleware customizado (a ser criado)
│   │   ├── Configuration/               # Classes de configuração (a serem criadas)
│   │   ├── Contexto/                    # DbContext (a ser criado)
│   │   ├── Properties/                  # Propriedades do projeto
│   │   ├── appsettings.json             # Configurações base
│   │   ├── appsettings.Development.json # Configurações de desenvolvimento
│   │   ├── appsettings.Production.json  # Configurações de produção
│   │   ├── appsettings.Testing.json     # Configurações de teste
│   │   ├── Program.cs                   # Ponto de entrada da aplicação
│   │   └── Agriis.Api.csproj           # Arquivo de projeto
│   │
│   ├── Agriis.Compartilhado/            # Componentes compartilhados
│   │   ├── Agriis.Compartilhado.Dominio/
│   │   │   ├── Entidades/               # Entidades base (a serem criadas)
│   │   │   ├── ObjetosValor/            # Value objects base (a serem criados)
│   │   │   ├── Interfaces/              # Interfaces de domínio (a serem criadas)
│   │   │   ├── Enums/                   # Enums compartilhados (a serem criados)
│   │   │   └── Agriis.Compartilhado.Dominio.csproj
│   │   │
│   │   ├── Agriis.Compartilhado.Aplicacao/
│   │   │   ├── Resultados/              # Result patterns (a serem criados)
│   │   │   ├── Behaviors/               # MediatR behaviors (a serem criados)
│   │   │   ├── Interfaces/              # Interfaces de aplicação (a serem criadas)
│   │   │   └── Agriis.Compartilhado.Aplicacao.csproj
│   │   │
│   │   └── Agriis.Compartilhado.Infraestrutura/
│   │       ├── Persistencia/            # Repository base (a ser criado)
│   │       ├── Logging/                 # Configuração de logging (a ser criada)
│   │       ├── Integracoes/             # Integrações externas (a serem criadas)
│   │       └── Agriis.Compartilhado.Infraestrutura.csproj
│   │
│   └── Modulos/                         # Módulos de domínio
│       ├── Autenticacao/                # Módulo de autenticação
│       │   └── Agriis.Autenticacao.Dominio/
│       │       └── Agriis.Autenticacao.Dominio.csproj
│       │
│       ├── Produtores/                  # Módulo de produtores
│       │   └── Agriis.Produtores.Dominio/
│       │       ├── Entidades/           # Entidades do domínio (a serem criadas)
│       │       ├── ObjetosValor/        # Value objects (a serem criados)
│       │       ├── Enums/               # Enums específicos (a serem criados)
│       │       ├── Interfaces/          # Interfaces (a serem criadas)
│       │       ├── Servicos/            # Serviços de domínio (a serem criados)
│       │       └── Agriis.Produtores.Dominio.csproj
│       │
│       ├── Fornecedores/                # Módulo de fornecedores
│       │   └── Agriis.Fornecedores.Dominio/
│       │       └── Agriis.Fornecedores.Dominio.csproj
│       │
│       └── Pedidos/                     # Módulo de pedidos
│           └── Agriis.Pedidos.Dominio/
│               └── Agriis.Pedidos.Dominio.csproj
│
└── logs/                                # Diretório de logs (criado automaticamente)
```

## Projetos da Solução

### 1. Agriis.Api
- **Tipo**: ASP.NET Core Web API
- **Responsabilidade**: Ponto de entrada da aplicação, controllers, middleware, configuração
- **Dependências**: Todos os projetos compartilhados e módulos de domínio

### 2. Agriis.Compartilhado.Dominio
- **Tipo**: Class Library
- **Responsabilidade**: Entidades base, interfaces de domínio, enums compartilhados
- **Dependências**: Nenhuma (núcleo do domínio)

### 3. Agriis.Compartilhado.Aplicacao
- **Tipo**: Class Library
- **Responsabilidade**: Result patterns, behaviors, interfaces de aplicação
- **Dependências**: Agriis.Compartilhado.Dominio

### 4. Agriis.Compartilhado.Infraestrutura
- **Tipo**: Class Library
- **Responsabilidade**: Repository base, logging, integrações externas
- **Dependências**: Agriis.Compartilhado.Dominio, Agriis.Compartilhado.Aplicacao

### 5. Módulos de Domínio
- **Tipo**: Class Library (cada módulo)
- **Responsabilidade**: Lógica específica de cada contexto de domínio
- **Dependências**: Agriis.Compartilhado.Dominio

## Tecnologias e Pacotes Configurados

### Framework Base
- .NET 9
- ASP.NET Core 9

### Banco de Dados
- Entity Framework Core 9.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0
- Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite 9.0.0

### Autenticação
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0
- System.IdentityModel.Tokens.Jwt 8.2.1

### Logging
- Serilog.AspNetCore 9.0.0
- Serilog.Sinks.Console 6.0.0
- Serilog.Sinks.File 6.0.0
- Serilog.Sinks.PostgreSQL 2.3.0

### Validação
- FluentValidation.AspNetCore 11.3.0

### Comunicação em Tempo Real
- Microsoft.AspNetCore.SignalR 1.1.0

### Jobs em Background
- Hangfire.Core 1.8.17
- Hangfire.PostgreSql 1.20.11
- Hangfire.AspNetCore 1.8.17

### Health Checks
- Microsoft.Extensions.Diagnostics.HealthChecks 9.0.0
- Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 9.0.0
- AspNetCore.HealthChecks.Npgsql 8.0.2

### AWS SDK
- AWSSDK.S3 3.7.412.3
- AWSSDK.Extensions.NETCore.Setup 3.7.301

### Utilitários
- AutoMapper 12.0.1
- AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
- MediatR 12.4.1
- Newtonsoft.Json 13.0.3

## Configurações por Ambiente

### Development
- Banco PostgreSQL local
- Logs detalhados no console e arquivo
- CORS permissivo para desenvolvimento local
- SSL desabilitado para desenvolvimento

### Testing
- Banco PostgreSQL de teste
- Logs mínimos
- Configurações otimizadas para testes unitários e integração

### Production
- Configurações via variáveis de ambiente
- SSL obrigatório
- Logs estruturados com múltiplos destinos
- Health checks habilitados
- CORS restritivo

## Próximos Passos

1. **Task 2**: Implementar camada compartilhada (EntidadeBase, interfaces, Result pattern)
2. **Task 3**: Configurar Entity Framework Core e PostgreSQL
3. **Task 4**: Migrar enums e tipos básicos
4. **Task 5**: Implementar módulo de Endereços
5. **Task 6**: Implementar módulo de Usuários base
6. **Task 7**: Implementar módulo de Autenticação

## Status Atual

✅ **Concluído**: Estrutura base do projeto .NET Core 9
- Solução criada com arquitetura modular
- Projetos configurados para cada módulo
- Dependências NuGet essenciais configuradas
- Configurações appsettings.json para múltiplos ambientes
- Build da solução funcionando corretamente

🔄 **Em Andamento**: Nenhuma task em andamento

⏳ **Próximo**: Task 2 - Implementar camada compartilhada