# Conversores de Enum e Objetos de Valor

Este documento explica como usar os conversores de enum e objetos de valor no Entity Framework Core.

## Conversores de Enum

Os conversores de enum estão disponíveis na classe `EnumConverters` e convertem enums para inteiros no PostgreSQL.

### Exemplo de uso em uma entidade:

```csharp
public class Produtor : EntidadeBase
{
    public StatusProdutor Status { get; set; }
    public Cpf Cpf { get; set; }
    public AreaPlantio AreaPlantio { get; set; }
}
```

### Configuração no DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar enum
    modelBuilder.Entity<Produtor>()
        .Property(p => p.Status)
        .HasConversion(EnumConverters.StatusProdutorConverter);

    // Configurar objetos de valor
    modelBuilder.Entity<Produtor>()
        .Property(p => p.Cpf)
        .HasConversion(ValueObjectConverters.CpfConverter);

    modelBuilder.Entity<Produtor>()
        .Property(p => p.AreaPlantio)
        .HasConversion(ValueObjectConverters.AreaPlantioConverter);
}
```

## Conversores Disponíveis

### Enums:
- `StatusProdutorConverter`
- `StatusPedidoConverter`
- `StatusCarrinhoConverter`
- `AcaoCompradorPedidoConverter`
- `TipoVendaConverter`
- `RolesConverter`
- `BarterTipoEntregaConverter`
- `ModalidadePagamentoConverter`
- `CalculoCubagemConverter`
- `CalculoFreteConverter`
- `ClassificacaoProdutoConverter`
- `TipoAcessoAuditoriaConverter`
- `StatusTentativaSerproConverter`
- `TipoUnidadeConverter`
- `MoedaConverter`
- `StatusGenericoConverter`
- `TipoOperacaoConverter`
- `NivelLogConverter`
- `TipoDocumentoConverter`
- `EstadoBrasilConverter`
- `TipoEnderecoConverter`
- `TipoContatoConverter`
- `TipoArquivoConverter`
- `ExtensaoArquivoConverter`

### Objetos de Valor:
- `CpfConverter`
- `CnpjConverter`
- `AreaPlantioConverter`

## Validadores

A classe `ValidadorDocumentosBrasileiros` fornece métodos estáticos para validação de documentos brasileiros:

```csharp
// Validar CPF
bool cpfValido = ValidadorDocumentosBrasileiros.ValidarCpf("123.456.789-01");

// Validar CNPJ
bool cnpjValido = ValidadorDocumentosBrasileiros.ValidarCnpj("12.345.678/0001-95");

// Validar email
bool emailValido = ValidadorDocumentosBrasileiros.ValidarEmail("teste@exemplo.com");

// Formatar documentos
string cpfFormatado = ValidadorDocumentosBrasileiros.FormatarCpf("12345678901");
string cnpjFormatado = ValidadorDocumentosBrasileiros.FormatarCnpj("12345678000195");
```

## Objetos de Valor

### CPF
```csharp
var cpf = new Cpf("123.456.789-01");
Console.WriteLine(cpf.Valor); // "12345678901"
Console.WriteLine(cpf.ValorFormatado); // "123.456.789-01"

// Conversão implícita
Cpf cpf2 = "12345678901";
string valorCpf = cpf2; // "12345678901"
```

### CNPJ
```csharp
var cnpj = new Cnpj("12.345.678/0001-95");
Console.WriteLine(cnpj.Valor); // "12345678000195"
Console.WriteLine(cnpj.ValorFormatado); // "12.345.678/0001-95"
```

### AreaPlantio
```csharp
var area = new AreaPlantio(100.5m);
Console.WriteLine(area.Valor); // 100.5
Console.WriteLine(area.ValorFormatado); // "100,50 ha"
Console.WriteLine(area.EmMetrosQuadrados); // 1005000

// Operações matemáticas
var area1 = new AreaPlantio(50);
var area2 = new AreaPlantio(30);
var areaTotal = area1 + area2; // 80 hectares

// Conversões
var areaDeAlqueires = AreaPlantio.DeAlqueiresPaulistas(10); // ~24.2 hectares
```