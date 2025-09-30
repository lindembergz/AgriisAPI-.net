# Correções Necessárias no Frontend - Módulo Municípios

## ✅ Correções Já Aplicadas

### **1. Modelos de Dados (reference.model.ts)**
- ✅ Alterado `codigoIbge` de `string` para `number`
- ✅ Adicionado `ufNome` e `ufCodigo` opcionais no `MunicipioDto`
- ✅ Atualizado `CriarMunicipioDto` para usar `codigoIbge: number`

### **2. Serviços (municipio.service.ts)**
- ✅ Atualizado método `validarCodigoIbgeUnico()` para aceitar `number`
- ✅ Atualizado método `obterDropdownPorUf()` para retornar `codigoIbge: number`
- ✅ Corrigido tipo da requisição HTTP em `obterDropdownPorUf()`

### **3. Componentes**
- ✅ Corrigido dados mockados em `geographic-selector.component.ts`
- ✅ Alterado `codigoIbge` de string para number nos dados de teste

## ⚠️ Correções Adicionais Necessárias

### **3. Componentes de Formulário**
Verificar e ajustar componentes que usam `codigoIbge`:

```typescript
// ANTES (string)
municipio.codigoIbge = "1234567";

// DEPOIS (number)  
municipio.codigoIbge = 1234567;
```

### **4. Validações de Formulário**
Ajustar validações para aceitar números:

```typescript
// Validação para código IBGE numérico
codigoIbgeControl: new FormControl(null, [
  Validators.required,
  Validators.min(1000000),
  Validators.max(9999999),
  Validators.pattern(/^\d{7}$/)
]);
```

### **5. Templates HTML**
Verificar inputs que podem precisar de ajuste:

```html
<!-- Usar type="number" para código IBGE -->
<input type="number" 
       formControlName="codigoIbge" 
       min="1000000" 
       max="9999999"
       placeholder="Código IBGE (7 dígitos)">
```

### **6. Exibição de Dados**
Verificar se há formatação específica para código IBGE:

```typescript
// Se necessário, formatar para exibição
formatarCodigoIbge(codigo: number): string {
  return codigo.toString().padStart(7, '0');
}
```

## 🔍 Arquivos que Podem Precisar de Verificação

### **Componentes**
- `municipio-form.component.ts`
- `municipio-list.component.ts` 
- `municipio-detail.component.ts`
- Qualquer componente que use dropdowns de municípios

### **Templates**
- `municipio-form.component.html`
- `municipio-list.component.html`
- Templates que exibem código IBGE

### **Validadores**
- Validadores customizados para código IBGE
- Máscaras de input se existirem

## 🧪 Testes Recomendados

### **1. Funcionalidade Básica**
- [ ] Listar municípios
- [ ] Criar novo município
- [ ] Editar município existente
- [ ] Validar código IBGE único

### **2. Dropdowns e Seleções**
- [ ] Dropdown de municípios por UF
- [ ] Busca de municípios por nome
- [ ] Seleção em formulários de endereço

### **3. Validações**
- [ ] Código IBGE obrigatório
- [ ] Código IBGE com 7 dígitos
- [ ] Validação de unicidade

## 📝 Notas Importantes

### **Relacionamento UF Temporariamente Limitado**
- O relacionamento com UF foi temporariamente ignorado no backend
- `ufNome` e `ufCodigo` podem vir vazios inicialmente
- Isso pode afetar a exibição de dados completos

### **Compatibilidade com Tabela Unificada**
- Agora usa a mesma tabela `municipios` do módulo Enderecos
- Dados podem ter campos adicionais (latitude, longitude, etc.)
- Frontend deve ser tolerante a campos extras

### **Migração de Dados**
- Códigos IBGE existentes como string precisam ser convertidos
- Verificar se há dados inconsistentes no banco

## ✅ Próximos Passos

1. **Testar a API** após as correções do backend
2. **Verificar componentes** que usam municípios
3. **Ajustar validações** conforme necessário
4. **Testar integração** com outros módulos
5. **Documentar mudanças** para a equipe