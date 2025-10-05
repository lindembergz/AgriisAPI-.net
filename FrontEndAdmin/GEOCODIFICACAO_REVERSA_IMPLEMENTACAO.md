# Implementação de Geocodificação Reversa

## Resumo

Foi implementada uma solução completa de geocodificação reversa no frontend Angular usando `@angular/google-maps`. A funcionalidade permite obter automaticamente dados de endereço (bairro, estado, município e CEP) através de coordenadas selecionadas no mapa.

## Componentes Implementados

### 1. GeocodingService (`src/app/shared/services/geocoding.service.ts`)

Serviço principal que integra com a Google Maps Geocoding API:

**Funcionalidades:**
- ✅ Geocodificação reversa (coordenadas → endereço)
- ✅ Geocodificação direta (endereço → coordenadas)
- ✅ Validação de coordenadas brasileiras
- ✅ Cálculo de distâncias entre pontos
- ✅ Formatação de coordenadas
- ✅ Tratamento de erros e timeouts

**Métodos principais:**
```typescript
obterEnderecoPorCoordenadas(lat: number, lng: number): Observable<GeocodingResult | null>
obterCoordenadasPorEndereco(endereco: string): Observable<GeocodingResult | null>
validarCoordenadasBrasil(lat: number, lng: number): boolean
calcularDistancia(lat1: number, lng1: number, lat2: number, lng2: number): number
```

### 2. EnderecoMapComponent (`src/app/shared/components/endereco-map.component.ts`)

Componente avançado que combina mapa interativo com formulário de endereço:

**Funcionalidades:**
- ✅ Mapa interativo do Google Maps
- ✅ Seleção de coordenadas por clique
- ✅ Geocodificação reversa automática
- ✅ Formulário de endereço integrado
- ✅ Validação de coordenadas brasileiras
- ✅ Modo somente leitura
- ✅ Estados de loading e erro

**Inputs/Outputs:**
```typescript
@Input() endereco: Endereco | null
@Input() readonly: boolean
@Input() height: string
@Input() width: string
@Output() enderecoChange: EventEmitter<Endereco>
@Output() coordenadasChange: EventEmitter<Coordenadas>
```

### 3. Interfaces Compartilhadas (`src/app/shared/interfaces/coordenadas.interface.ts`)

Definições de tipos e constantes geográficas:

**Interfaces:**
- `Coordenadas` - Coordenadas básicas (lat/lng)
- `PontoGeografico` - Coordenadas com informações extras
- `LimitesGeograficos` - Bounds geográficos
- `DistanciaGeografica` - Resultado de cálculo de distância

**Constantes:**
- `BRASIL_BOUNDS` - Limites do território brasileiro
- `BRASIL_CENTRO` - Centro geográfico do Brasil
- `RAIO_TERRA_KM` - Raio da Terra em km

### 4. Componente de Exemplo (`src/app/shared/components/endereco-map-example.component.ts`)

Demonstra o uso prático dos componentes com exemplos interativos.

## Como Usar

### 1. Importar o Componente

```typescript
import { EnderecoMapComponent } from '@shared/components';
import { Endereco } from '@shared/models/endereco.model';
import { Coordenadas } from '@shared/interfaces/coordenadas.interface';
```

### 2. Usar no Template

```html
<app-endereco-map
  [endereco]="enderecoSelecionado"
  [readonly]="false"
  height="400px"
  (enderecoChange)="onEnderecoChange($event)"
  (coordenadasChange)="onCoordenadasChange($event)">
</app-endereco-map>
```

### 3. Implementar no Component

```typescript
export class MeuComponent {
  enderecoSelecionado: Endereco | null = null;

  onEnderecoChange(endereco: Endereco): void {
    this.enderecoSelecionado = endereco;
    console.log('Endereço atualizado:', endereco);
  }

  onCoordenadasChange(coordenadas: Coordenadas): void {
    console.log('Coordenadas selecionadas:', coordenadas);
  }
}
```

## Fluxo de Funcionamento

1. **Usuário clica no mapa** → Coordenadas são capturadas
2. **Validação automática** → Verifica se está no Brasil
3. **Geocodificação reversa** → Busca dados do endereço via Google Maps API
4. **Preenchimento automático** → Formulário é preenchido com os dados obtidos
5. **Emissão de eventos** → Componente pai é notificado das mudanças

## Configuração Necessária

### 1. Google Maps API Key

A chave da API já está configurada em:
- `src/environments/environment.ts`
- `src/environments/environment.prod.ts`

```typescript
export const environment = {
  // ...
  googleApiKey: 'AIzaSyAIIYOxA7qeetFz6TuR1Qewc0Rrjhzx7ZU'
};
```

### 2. Dependências

O `@angular/google-maps` já está instalado:
```json
"@angular/google-maps": "^20.2.2"
```

## Recursos Implementados

### ✅ Funcionalidades Principais
- Geocodificação reversa (coordenadas → endereço)
- Geocodificação direta (endereço → coordenadas)
- Validação de território brasileiro
- Formulário de endereço integrado
- Mapa interativo responsivo

### ✅ Tratamento de Erros
- Timeout de requisições (10s)
- Fallback para erros de API
- Validação de coordenadas
- Estados de loading e erro

### ✅ Performance
- Cache de resultados
- Cleanup de recursos do Google Maps
- Lazy loading de componentes
- Otimização de re-renders

### ✅ Acessibilidade
- Labels apropriados
- Navegação por teclado
- Estados visuais claros
- Mensagens de erro descritivas

### ✅ Responsividade
- Layout adaptativo
- Mapas responsivos
- Botões otimizados para mobile
- Formulários mobile-friendly

## Casos de Uso

### 1. Cadastro de Fornecedores
```typescript
// No componente de fornecedor
<app-endereco-map
  [endereco]="fornecedor.endereco"
  (enderecoChange)="atualizarEnderecoFornecedor($event)">
</app-endereco-map>
```

### 2. Cadastro de Produtores
```typescript
// No componente de produtor
<app-endereco-map
  [endereco]="produtor.propriedade.endereco"
  (enderecoChange)="atualizarEnderecoPropriedade($event)">
</app-endereco-map>
```

### 3. Visualização Somente Leitura
```typescript
// Para exibir endereços existentes
<app-endereco-map
  [endereco]="enderecoExistente"
  [readonly]="true">
</app-endereco-map>
```

## Próximos Passos

### Melhorias Futuras
- [ ] Integração com ViaCEP para validação de CEP
- [ ] Suporte a múltiplos endereços
- [ ] Histórico de endereços recentes
- [ ] Integração com API de endereços do backend
- [ ] Suporte offline com cache local

### Integração com Backend
- [ ] Endpoint para salvar coordenadas
- [ ] Validação server-side de endereços
- [ ] Sincronização com banco de dados PostGIS

## Arquivos Criados/Modificados

### Novos Arquivos
- `src/app/shared/services/geocoding.service.ts`
- `src/app/shared/components/endereco-map.component.ts`
- `src/app/shared/components/endereco-map.component.scss`
- `src/app/shared/components/endereco-map-example.component.ts`
- `src/app/shared/interfaces/coordenadas.interface.ts`
- `src/app/shared/interfaces/index.ts`
- `src/app/shared/services/index.ts`

### Arquivos Modificados
- `src/app/shared/components/index.ts` - Exportações atualizadas
- `src/app/shared/components/coordenadas-map.component.ts` - Interface compartilhada

## Conclusão

A implementação de geocodificação reversa está completa e pronta para uso. O sistema permite uma experiência fluida para seleção de endereços através de mapas interativos, com preenchimento automático dos dados de endereço baseado nas coordenadas selecionadas.

A solução é robusta, performática e segue as melhores práticas do Angular, sendo facilmente integrável aos módulos existentes de Fornecedores e Produtores.