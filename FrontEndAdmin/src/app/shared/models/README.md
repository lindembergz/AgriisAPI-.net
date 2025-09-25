# Shared Models Documentation

This directory contains all TypeScript interfaces and models used throughout the FrontEndAdmin application.

## Structure

### Base Models
- **base.model.ts**: Contains `BaseEntity` interface with common audit fields (id, dataCriacao, dataAtualizacao, ativo) and `BaseForm` interface for reactive forms.

### Core Entity Models
- **endereco.model.ts**: Address entity and form interfaces
- **user.model.ts**: User/Usuario entity and form interfaces (includes legacy User interface for backward compatibility)
- **cultura.model.ts**: Crop entity and form interfaces with `TipoCultura` enum
- **propriedade.model.ts**: Rural property entity and form interfaces
- **produtor.model.ts**: Rural producer entity and form interfaces with `TipoCliente` enum
- **ponto-distribuicao.model.ts**: Distribution point entity and form interfaces
- **fornecedor.model.ts**: Supplier entity and form interfaces

### Authentication Models
- **auth.model.ts**: Authentication-related interfaces including `LoginForm`, `AuthResponse`, `RefreshTokenRequest`, and `AuthState`

### API Models
- **api.model.ts**: Generic API response interfaces including `ApiResponse`, `PaginatedResponse`, `ApiError`, `ValidationError`, and `LoadingState`

### Form Models
- **forms.model.ts**: Strongly-typed reactive form interfaces for all entities using Angular's `FormControl`, `FormGroup`, and `FormArray`

### Legacy Models
- **order.model.ts**: Order entity for existing dashboard functionality

## Usage

### Importing Models
```typescript
// Import specific models
import { Produtor, ProdutorForm } from '@shared/models';

// Import all models
import * from '@shared/models';
```

### Using Entity Models
```typescript
const produtor: Produtor = {
  id: 1,
  dataCriacao: new Date(),
  ativo: true,
  codigo: 'PROD001',
  nome: 'João Silva',
  cpfCnpj: '12345678901',
  tipoCliente: TipoCliente.PF,
  enderecos: [],
  propriedades: []
};
```

### Using Form Models
```typescript
const produtorForm = this.fb.group<ProdutorFormControls>({
  dadosGerais: this.fb.group({
    nome: this.fb.control('', { validators: [Validators.required] }),
    cpfCnpj: this.fb.control('', { validators: [Validators.required] }),
    tipoCliente: this.fb.control(TipoCliente.PF),
    telefone: this.fb.control(''),
    email: this.fb.control('', { validators: [Validators.email] })
  }),
  enderecos: this.fb.array([]),
  propriedades: this.fb.array([]),
  usuarioMaster: this.fb.group({
    nome: this.fb.control(''),
    email: this.fb.control(''),
    senha: this.fb.control(''),
    telefone: this.fb.control('')
  })
});
```

## Enums

### TipoCliente
- `PF`: Pessoa Física (Individual)
- `PJ`: Pessoa Jurídica (Company)

### TipoCultura
- `SOJA`: Soybean
- `MILHO`: Corn
- `ALGODAO`: Cotton
- `OUTROS`: Others

### OrderStatus
- `APROVADO`: Approved
- `PENDENTE`: Pending
- `CANCELADO`: Cancelled

## Design Principles

1. **Consistency**: All entity models extend `BaseEntity` for common audit fields
2. **Type Safety**: Strongly-typed interfaces for both entities and reactive forms
3. **Reusability**: Shared components like `Endereco` are used across multiple entities
4. **Backward Compatibility**: Legacy interfaces are maintained with deprecation notices
5. **Form Integration**: Dedicated form interfaces for seamless Angular Reactive Forms integration

## Requirements Compliance

This implementation satisfies requirement 6.7 from the specifications:
- ✅ TypeScript interfaces for all data models (Produtor, Fornecedor, Endereco, Usuario, etc.)
- ✅ Base entity interface with common audit fields
- ✅ Form model interfaces for reactive forms