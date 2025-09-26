# UFs Module

Este módulo implementa o CRUD completo para Unidades Federativas (UFs) com relacionamento com País e dependência de Municípios.

## Funcionalidades

### ✅ Implementadas

- **CRUD Completo**: Criar, listar, editar, ativar/desativar e excluir UFs
- **Relacionamento com País**: Seleção obrigatória de país ao criar UF
- **Validação de Código**: Código único de 2 caracteres (formato: XX)
- **Contagem de Municípios**: Exibe quantos municípios cada UF possui
- **Validação de Dependências**: Impede exclusão de UF que possui municípios
- **Filtros**: Por status (ativas/inativas/todas)
- **Busca**: Por código, nome ou país
- **Responsividade**: Interface adaptada para mobile e tablet
- **Validação em Tempo Real**: Validação assíncrona de código único
- **Controle de Concorrência**: Prevenção de conflitos de edição simultânea

### 🎨 Interface

- **Lista Paginada**: Com ordenação e filtros
- **Formulário Modal**: Para criação e edição
- **Indicadores Visuais**: Tags coloridas para status e contagem
- **Tooltips Informativos**: Ajuda contextual
- **Feedback Visual**: Mensagens de sucesso/erro
- **Estados de Loading**: Indicadores de carregamento

### 🔧 Componentes

#### UfsComponent
- Estende `ReferenceCrudBaseComponent`
- Gerencia estado reativo com signals
- Implementa validações customizadas
- Controla relacionamentos com País

#### UfService
- Estende `ReferenceCrudService`
- Métodos específicos para relacionamentos
- Cache inteligente de dados
- Tratamento de erros padronizado

## Estrutura de Arquivos

```
ufs/
├── services/
│   └── uf.service.ts           # Serviço com métodos específicos
├── ufs.component.ts            # Componente principal
├── ufs.component.html          # Template do componente
├── ufs.component.scss          # Estilos do componente
├── ufs.component.spec.ts       # Testes unitários
├── ufs.routes.ts              # Configuração de rotas
└── README.md                  # Esta documentação
```

## Modelos de Dados

### UfDto
```typescript
interface UfDto extends BaseReferenceEntity {
  codigo: string;        // Código de 2 caracteres (SP, RJ, MG)
  paisId: number;        // ID do país
  pais?: PaisDto;        // Dados do país (opcional)
}
```

### CriarUfDto
```typescript
interface CriarUfDto {
  codigo: string;        // Obrigatório, 2 caracteres, maiúsculo
  nome: string;          // Obrigatório, 2-100 caracteres
  paisId: number;        // Obrigatório, ID válido de país
}
```

### AtualizarUfDto
```typescript
interface AtualizarUfDto {
  nome: string;          // Obrigatório, 2-100 caracteres
  ativo: boolean;        // Status ativo/inativo
}
```

## Validações

### Frontend
- **Código**: Obrigatório, exatamente 2 caracteres maiúsculos
- **Nome**: Obrigatório, 2-100 caracteres
- **País**: Obrigatório, deve ser um país válido e ativo
- **Unicidade**: Código deve ser único dentro do país

### Backend (Esperado)
- Validação de integridade referencial
- Verificação de dependências antes da exclusão
- Controle de concorrência otimista
- Validação de dados de entrada

## Endpoints da API

### Básicos (herdados)
- `GET /api/referencias/ufs` - Listar todas
- `GET /api/referencias/ufs/ativos` - Listar ativas
- `GET /api/referencias/ufs/{id}` - Obter por ID
- `POST /api/referencias/ufs` - Criar nova
- `PUT /api/referencias/ufs/{id}` - Atualizar
- `PATCH /api/referencias/ufs/{id}/ativar` - Ativar
- `PATCH /api/referencias/ufs/{id}/desativar` - Desativar
- `DELETE /api/referencias/ufs/{id}` - Excluir

### Específicos
- `GET /api/referencias/ufs/pais/{paisId}` - UFs por país
- `GET /api/referencias/ufs/ativos/pais/{paisId}` - UFs ativas por país
- `GET /api/referencias/ufs/{id}/tem-municipios` - Verificar dependências
- `GET /api/referencias/ufs/{id}/municipios/count` - Contagem de municípios
- `GET /api/referencias/ufs/{id}/municipios` - Municípios da UF
- `GET /api/referencias/ufs/validar-codigo` - Validar código único

## Casos de Uso

### 1. Criar Nova UF
1. Usuário clica em "Nova UF"
2. Sistema carrega países ativos no dropdown
3. Usuário preenche código, nome e seleciona país
4. Sistema valida código único dentro do país
5. Sistema cria UF e atualiza lista

### 2. Editar UF Existente
1. Usuário clica em "Editar" na linha da UF
2. Sistema carrega dados atuais da UF
3. Usuário pode alterar apenas nome e status
4. Sistema valida e atualiza UF

### 3. Excluir UF
1. Usuário clica em "Excluir"
2. Sistema verifica se UF possui municípios
3. Se possui, exibe aviso e impede exclusão
4. Se não possui, confirma exclusão com usuário
5. Sistema exclui UF após confirmação

### 4. Filtrar por País
1. Sistema exibe país de cada UF na lista
2. Usuário pode buscar por nome do país
3. Lista é filtrada dinamicamente

## Testes

### Cobertura
- ✅ Criação do componente
- ✅ Carregamento de países
- ✅ Carregamento de UFs
- ✅ Validações de formulário
- ✅ Mapeamento de DTOs
- ✅ População de formulário
- ✅ Exibição de contadores
- ✅ Verificação de dependências
- ✅ Prevenção de exclusão
- ✅ Configuração de colunas

### Executar Testes
```bash
ng test --include="**/ufs/**/*.spec.ts"
```

## Dependências

### Serviços
- `UfService` - Operações CRUD específicas
- `PaisService` - Carregamento de países para dropdown
- `ReferenceCrudService` - Funcionalidades base
- `MessageService` - Notificações
- `ConfirmationService` - Diálogos de confirmação

### Componentes
- `ReferenceCrudBaseComponent` - Funcionalidades CRUD base
- PrimeNG: Select, Tag, Tooltip, InputText

## Configuração

### Rotas
```typescript
// app.routes.ts
{
  path: 'referencias/ufs',
  loadChildren: () => import('./features/referencias/ufs/ufs.routes').then(m => m.UFS_ROUTES)
}
```

### Lazy Loading
O módulo é carregado sob demanda quando o usuário acessa a rota `/referencias/ufs`.

## Melhorias Futuras

### 🚀 Próximas Implementações
- [ ] Importação em lote de UFs
- [ ] Exportação para Excel/CSV
- [ ] Histórico de alterações
- [ ] Integração com APIs externas (IBGE)
- [ ] Validação de CEP por UF
- [ ] Mapa interativo

### 🎯 Otimizações
- [ ] Virtual scrolling para listas grandes
- [ ] Cache mais inteligente
- [ ] Pré-carregamento de dados relacionados
- [ ] Compressão de imagens/ícones
- [ ] Service Worker para offline

## Troubleshooting

### Problemas Comuns

1. **Países não carregam no dropdown**
   - Verificar se PaisService está funcionando
   - Verificar se há países ativos cadastrados

2. **Validação de código único não funciona**
   - Verificar se endpoint de validação existe
   - Verificar se país está selecionado primeiro

3. **Contagem de municípios não aparece**
   - Verificar se endpoint de contagem existe
   - Verificar se há municípios cadastrados

4. **Erro ao excluir UF**
   - Verificar se UF possui municípios
   - Verificar permissões do usuário

### Debug
```typescript
// Habilitar logs detalhados
localStorage.setItem('debug', 'ufs:*');
```

## Contribuição

1. Seguir padrões estabelecidos no `ReferenceCrudBaseComponent`
2. Manter cobertura de testes acima de 80%
3. Documentar mudanças significativas
4. Testar responsividade em diferentes dispositivos
5. Validar acessibilidade (WCAG 2.1)

## Changelog

### v1.0.0 (2025-09-26)
- ✅ Implementação inicial completa
- ✅ CRUD com relacionamento País
- ✅ Validações e dependências
- ✅ Interface responsiva
- ✅ Testes unitários
- ✅ Documentação completa