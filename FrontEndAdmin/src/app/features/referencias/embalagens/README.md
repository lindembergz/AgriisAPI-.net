# Embalagens Module

Este módulo gerencia as embalagens (tipos de embalagem) utilizadas no sistema Agriis.

## Funcionalidades

- **CRUD Completo**: Criar, visualizar, editar e excluir embalagens
- **Relacionamento com UnidadeMedida**: Cada embalagem está associada a uma unidade de medida
- **Filtros Avançados**: 
  - Filtro por tipo de unidade de medida (Peso, Volume, Área, Unidade)
  - Filtro por unidade de medida específica
- **Validações**: Validação de campos obrigatórios e relacionamentos
- **Interface Responsiva**: Adaptada para desktop, tablet e mobile

## Estrutura

```
embalagens/
├── embalagens.component.ts          # Componente principal
├── embalagens.component.scss        # Estilos do componente
├── embalagens.component.spec.ts     # Testes unitários
├── embalagens.routes.ts             # Configuração de rotas
├── services/
│   └── embalagem.service.ts         # Serviço para operações CRUD
└── README.md                        # Esta documentação
```

## Componente Principal

### EmbalagensComponent

Estende `ReferenceCrudBaseComponent` para fornecer funcionalidade CRUD consistente.

**Propriedades principais:**
- `unidadesDisponiveis`: Lista de unidades de medida disponíveis
- `unidadesFiltradas`: Lista filtrada por tipo de unidade
- `tiposUnidadeDisponiveis`: Tipos de unidade disponíveis para filtro
- `tipoUnidadeSelecionado`: Tipo de unidade selecionado no filtro
- `unidadeMedidaSelecionada`: Unidade de medida selecionada no filtro

**Métodos principais:**
- `carregarUnidadesMedida()`: Carrega unidades de medida para dropdown
- `carregarTiposUnidade()`: Carrega tipos de unidade para filtro
- `onTipoUnidadeFilterChange()`: Manipula mudança no filtro de tipo
- `onUnidadeMedidaFilterChange()`: Manipula mudança no filtro de unidade
- `getTipoUnidadeDescricao()`: Obtém descrição do tipo de unidade

## Serviço

### EmbalagemService

Estende `ReferenceCrudService` para operações CRUD padronizadas.

**Métodos específicos:**
- `obterPorUnidadeMedida(unidadeMedidaId)`: Busca embalagens por unidade de medida
- `obterPorTipoUnidadeMedida(tipo)`: Busca embalagens por tipo de unidade
- `obterUnidadesMedidaParaDropdown()`: Obtém unidades para dropdown
- `obterTiposUnidade()`: Obtém tipos de unidade disponíveis
- `verificarNomeUnico(nome, idExcluir?)`: Verifica unicidade do nome
- `getTipoUnidadeDescricao(tipo)`: Obtém descrição do tipo

## Formulário

### Campos

1. **Nome** (obrigatório)
   - Tipo: Texto
   - Validação: Mínimo 2, máximo 100 caracteres
   - Exemplo: "Saco", "Caixa", "Tambor", "Fardo"

2. **Descrição** (opcional)
   - Tipo: Textarea
   - Validação: Máximo 500 caracteres
   - Exemplo: "Saco de papel kraft para produtos secos"

3. **Unidade de Medida** (obrigatório)
   - Tipo: Dropdown com busca
   - Relacionamento: FK para UnidadeMedida
   - Exibe: Símbolo, nome e tipo da unidade

4. **Status** (apenas edição)
   - Tipo: Checkbox
   - Controla se a embalagem está ativa

### Validações

- Nome é obrigatório e deve ser único
- Unidade de medida é obrigatória e deve existir
- Descrição é opcional mas limitada a 500 caracteres

## Filtros

### Filtro por Tipo de Unidade
- Permite filtrar embalagens por tipo de unidade de medida
- Tipos: Peso, Volume, Área, Unidade
- Atualiza automaticamente o dropdown de unidades disponíveis

### Filtro por Unidade de Medida
- Permite filtrar embalagens por unidade específica
- Dropdown com busca por nome e símbolo
- Mostra tipo da unidade como badge

## Tabela

### Colunas

1. **Nome**: Nome da embalagem
2. **Descrição**: Descrição detalhada (oculta em mobile)
3. **Unidade de Medida**: Símbolo, nome e tipo (coluna customizada)
4. **Status**: Ativo/Inativo com tag colorida (oculta em mobile)
5. **Criado em**: Data de criação (oculta em mobile/tablet)
6. **Ações**: Editar, Ativar/Desativar, Excluir

### Funcionalidades da Tabela

- Paginação configurável (5, 10, 20, 50 itens)
- Ordenação por múltiplas colunas
- Busca global nos campos nome e descrição
- Filtros de status (Todas, Ativas, Inativas)
- Layout responsivo com colunas adaptáveis

## Relacionamentos

### UnidadeMedida
- **Tipo**: Muitos para Um (N:1)
- **Chave Estrangeira**: `unidadeMedidaId`
- **Navigation Property**: `unidadeMedida`
- **Validação**: Unidade deve existir e estar ativa

## APIs Utilizadas

### Endpoints Principais
- `GET /api/referencias/embalagens` - Lista todas as embalagens
- `GET /api/referencias/embalagens/ativos` - Lista embalagens ativas
- `GET /api/referencias/embalagens/{id}` - Obtém embalagem por ID
- `POST /api/referencias/embalagens` - Cria nova embalagem
- `PUT /api/referencias/embalagens/{id}` - Atualiza embalagem
- `PATCH /api/referencias/embalagens/{id}/ativar` - Ativa embalagem
- `PATCH /api/referencias/embalagens/{id}/desativar` - Desativa embalagem
- `DELETE /api/referencias/embalagens/{id}` - Remove embalagem

### Endpoints de Relacionamento
- `GET /api/referencias/embalagens/unidade-medida/{id}` - Por unidade de medida
- `GET /api/referencias/embalagens/tipo-unidade/{tipo}` - Por tipo de unidade
- `GET /api/referencias/unidades-medida/dropdown-completo` - Unidades para dropdown
- `GET /api/referencias/unidades-medida/tipos` - Tipos de unidade

### Endpoints de Validação
- `GET /api/referencias/embalagens/existe-nome/{nome}` - Verifica nome único
- `GET /api/referencias/embalagens/{id}/pode-remover` - Verifica se pode remover

## Testes

### Testes Unitários
- Criação e inicialização do componente
- Configuração correta da entidade
- Carregamento de dados relacionados
- Validações de formulário
- Mapeamento de DTOs
- Funcionalidade de filtros
- Manipulação de eventos

### Cenários de Teste
1. **Criação**: Criar nova embalagem com dados válidos
2. **Edição**: Editar embalagem existente
3. **Validação**: Testar validações de campos obrigatórios
4. **Filtros**: Testar filtros por tipo e unidade
5. **Relacionamento**: Testar carregamento de unidades de medida
6. **Responsividade**: Testar layout em diferentes tamanhos de tela

## Uso

### Importação
```typescript
import { EmbalagensComponent } from './embalagens.component';
import { EmbalagemService } from './services/embalagem.service';
```

### Roteamento
```typescript
{
  path: 'embalagens',
  loadChildren: () => import('./embalagens/embalagens.routes').then(m => m.EMBALAGENS_ROUTES)
}
```

### Dependências
- `ReferenceCrudBaseComponent`: Componente base para CRUD
- `ReferenceCrudService`: Serviço base para operações CRUD
- `UnidadeMedidaService`: Para carregar unidades de medida
- PrimeNG: Componentes de UI (Table, Select, InputText, etc.)

## Considerações de Performance

- **Cache**: Unidades de medida são cacheadas por 15 minutos
- **Lazy Loading**: Módulo carregado sob demanda
- **Filtros**: Aplicados no frontend para melhor responsividade
- **Paginação**: Reduz carga inicial da tabela
- **Busca**: Implementada com debounce para evitar requisições excessivas

## Acessibilidade

- Labels associados aos campos de formulário
- Tooltips informativos nos botões de ação
- Navegação por teclado suportada
- Mensagens de erro claras e específicas
- Contraste adequado para diferentes temas