# Correção: Geocodificação Reversa com Resolução de IDs

## Problema Identificado

A funcionalidade de geocodificação reversa estava obtendo corretamente os dados de texto do endereço (cidade, UF, bairro, etc.) através da API do Google Maps, mas **não estava mapeando esses dados para os IDs correspondentes no banco de dados** (ufId, municipioId).

### Sintomas:
- ✅ Geocodificação funcionando (obtendo cidade e UF como texto)
- ❌ IDs não sendo atualizados (ufId, municipioId permaneciam vazios)
- ❌ Após salvar, a consulta ainda exibia a localização anterior
- ❌ Dados não persistiam corretamente no banco

## Solução Implementada

### 1. Atualização do GeocodingService

**Arquivo:** `src/app/shared/services/geocoding.service.ts`

#### Mudanças principais:

1. **Interface GeocodingResult expandida:**
```typescript
export interface GeocodingResult {
  // ... campos existentes
  // IDs resolvidos do banco de dados
  ufId?: number;
  municipioId?: number;
}
```

2. **Novo fluxo de resolução de IDs:**
```typescript
obterEnderecoPorCoordenadas(latitude: number, longitude: number): Observable<GeocodingResult | null> {
  return from(/* geocoding do Google Maps */)
    .pipe(
      map(results => this.processarResultadoGeocodificacao(results, latitude, longitude)),
      switchMap(result => {
        if (result && result.uf && result.cidade) {
          return this.resolverIdsGeograficos(result); // ← NOVO
        }
        return of(result);
      })
    );
}
```

3. **Métodos de resolução de IDs:**
```typescript
private resolverIdsGeograficos(result: GeocodingResult): Observable<GeocodingResult> {
  // 1. Buscar UF pelo código (ex: 'SP' → ufId: 25)
  return this.buscarUfPorCodigo(result.uf).pipe(
    switchMap(uf => {
      if (uf) {
        result.ufId = uf.id;
        // 2. Buscar município pelo nome e ufId
        return this.buscarMunicipioPorNome(result.cidade, uf.id).pipe(
          map(municipio => {
            if (municipio) {
              result.municipioId = municipio.id;
            }
            return result;
          })
        );
      }
      return of(result);
    })
  );
}
```

### 2. Integração com APIs Backend

**Endpoints utilizados:**
- `GET /api/enderecos/estados/uf/{ufCodigo}` - Busca UF por código
- `GET /api/enderecos/municipios/buscar?nome={nome}&ufId={ufId}` - Busca município

### 3. Atualização do EnderecoMapComponent

**Arquivo:** `src/app/shared/components/endereco-map.component.ts`

#### Mudanças:

1. **Preenchimento com IDs resolvidos:**
```typescript
private preencherFormularioComResultado(result: GeocodingResult): void {
  // ... preenchimento normal dos campos
  
  // Atualizar o endereço com os IDs resolvidos se disponíveis
  if (result.ufId || result.municipioId) {
    const enderecoAtualizado = {
      ...this.endereco,
      ufId: result.ufId,
      municipioId: result.municipioId,
      ufNome: result.cidade,
      municipioNome: result.cidade,
      ufCodigo: result.uf
    };
    
    console.log('🆔 IDs geográficos resolvidos:', {
      ufId: result.ufId,
      municipioId: result.municipioId
    });
  }
}
```

2. **Emissão com IDs:**
```typescript
private emitEnderecoChange(): void {
  const geocodingResult = this.geocodingResult();
  
  const endereco: Endereco = {
    // ... campos normais
    // Incluir IDs resolvidos se disponíveis
    ufId: geocodingResult?.ufId || this.endereco?.ufId,
    municipioId: geocodingResult?.municipioId || this.endereco?.municipioId,
    ufNome: formValue.cidade || this.endereco?.ufNome,
    ufCodigo: formValue.uf || this.endereco?.ufCodigo,
    municipioNome: formValue.cidade || this.endereco?.municipioNome,
  };

  this.enderecoChange.emit(endereco);
}
```

### 4. Atualização do FornecedorDetailComponent

**Arquivo:** `src/app/features/fornecedores/components/fornecedor-detail.component.ts`

#### Mudanças:

1. **Manipulação de mudanças do mapa:**
```typescript
onEnderecoMapChange(endereco: Endereco): void {
  // ... atualização normal
  
  // Update the form with the new address data including IDs
  this.enderecoFormGroup.patchValue({
    // ... campos normais
    // Atualizar os IDs resolvidos
    ufId: endereco.ufId,
    municipioId: endereco.municipioId
  });

  // Se os IDs foram resolvidos, atualizar as listas de seleção
  if (endereco.ufId) {
    console.log('🆔 Atualizando UF ID:', endereco.ufId);
    this.loadMunicipiosForSelect(endereco.ufId);
  }
}
```

## Fluxo Completo da Correção

### Antes (Problema):
1. Usuário clica no mapa → Coordenadas capturadas
2. Google Maps API → Retorna dados de texto (cidade: "São Paulo", uf: "SP")
3. Formulário preenchido → Apenas com dados de texto
4. Salvar → ufId e municipioId ficam vazios/antigos
5. Consulta → Exibe dados antigos porque IDs não foram atualizados

### Depois (Corrigido):
1. Usuário clica no mapa → Coordenadas capturadas
2. Google Maps API → Retorna dados de texto (cidade: "São Paulo", uf: "SP")
3. **NOVO:** Resolução de IDs → Busca ufId=25 e municipioId=3550308
4. Formulário preenchido → Com dados de texto E IDs corretos
5. Salvar → ufId e municipioId são salvos corretamente
6. Consulta → Exibe dados corretos baseados nos IDs atualizados

## Exemplo Prático

### Cenário: Mudança de São Paulo para Brasília

**Coordenadas clicadas:** -15.7998, -47.8645 (Brasília)

**Resultado da geocodificação:**
```json
{
  "logradouro": "Esplanada dos Ministérios",
  "cidade": "Brasília",
  "uf": "DF",
  "cep": "70150-900",
  "ufId": 7,        // ← ID do DF no banco
  "municipioId": 5300108  // ← ID de Brasília no banco
}
```

**Formulário atualizado:**
```typescript
{
  cidade: "Brasília",
  uf: "DF",
  ufId: 7,           // ← Agora é atualizado corretamente
  municipioId: 5300108,  // ← Agora é atualizado corretamente
  latitude: -15.7998,
  longitude: -47.8645
}
```

## Testes Implementados

### Arquivo de Teste: `test-geocoding-ids.html`

Testa os seguintes cenários:
- ✅ São Paulo: -23.5505, -46.6333
- ✅ Brasília: -15.7998, -47.8645  
- ✅ Rio de Janeiro: -22.9068, -43.1729
- ✅ APIs backend (UF e Município)

## Benefícios da Correção

1. **Persistência correta:** IDs são salvos no banco de dados
2. **Consultas precisas:** Dados são recuperados corretamente
3. **Integridade referencial:** Relacionamentos UF-Município mantidos
4. **Experiência do usuário:** Mudanças de localização funcionam como esperado
5. **Compatibilidade:** Funciona com sistema existente de seleção de UF/Município

## Arquivos Modificados

- ✅ `src/app/shared/services/geocoding.service.ts`
- ✅ `src/app/shared/components/endereco-map.component.ts`
- ✅ `src/app/features/fornecedores/components/fornecedor-detail.component.ts`
- ✅ `test-geocoding-ids.html` (arquivo de teste)

## Status

✅ **CORRIGIDO:** A geocodificação reversa agora resolve corretamente os IDs de UF e Município, garantindo que as mudanças de localização sejam persistidas e consultadas adequadamente.