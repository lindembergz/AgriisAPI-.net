using Bogus;
using Bogus.Extensions.Brazil;

namespace Agriis.Tests.Shared.Generators;

/// <summary>
/// Gerador de dados de teste, equivalente ao random_generators.py
/// </summary>
public class TestDataGenerator
{
    private readonly Faker _faker;
    private readonly Random _random;

    public TestDataGenerator()
    {
        _faker = new Faker("pt_BR");
        _random = new Random();
    }

    #region Documentos Brasileiros

    /// <summary>
    /// Gera um CPF válido
    /// </summary>
    public string GerarCpf()
    {
        return _faker.Person.Cpf(false); // false = sem formatação
    }

    /// <summary>
    /// Gera um CPF válido formatado
    /// </summary>
    public string GerarCpfFormatado()
    {
        return _faker.Person.Cpf(true); // true = com formatação
    }

    /// <summary>
    /// Gera um CNPJ válido
    /// </summary>
    public string GerarCnpj()
    {
        return _faker.Company.Cnpj(false); // false = sem formatação
    }

    /// <summary>
    /// Gera um CNPJ válido formatado
    /// </summary>
    public string GerarCnpjFormatado()
    {
        return _faker.Company.Cnpj(true); // true = com formatação
    }

    /// <summary>
    /// Gera uma inscrição estadual válida
    /// </summary>
    public string GerarInscricaoEstadual()
    {
        return _faker.Random.Replace("###.###.###");
    }

    /// <summary>
    /// Gera um NIRF (Número de Identificação da Propriedade Rural Federal) válido
    /// </summary>
    public string GerarNirf()
    {
        // NIRF tem formato: XXXXXXX-X.XXX.X
        return _faker.Random.Replace("#######-#.###.#");
    }

    #endregion

    #region Dados Pessoais

    /// <summary>
    /// Gera um nome completo
    /// </summary>
    public string GerarNome()
    {
        return _faker.Person.FullName;
    }

    /// <summary>
    /// Gera um nome de empresa
    /// </summary>
    public string GerarNomeEmpresa()
    {
        return _faker.Company.CompanyName();
    }

    /// <summary>
    /// Gera um email válido
    /// </summary>
    public string GerarEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Gera um telefone celular
    /// </summary>
    public string GerarCelular()
    {
        return _faker.Phone.PhoneNumber("(##) #####-####");
    }

    /// <summary>
    /// Gera um telefone fixo
    /// </summary>
    public string GerarTelefone()
    {
        return _faker.Phone.PhoneNumber("(##) ####-####");
    }

    #endregion

    #region Endereços

    /// <summary>
    /// Gera um CEP válido
    /// </summary>
    public string GerarCep()
    {
        return _faker.Address.ZipCode();
    }

    /// <summary>
    /// Gera um endereço completo
    /// </summary>
    public EnderecoTeste GerarEndereco()
    {
        return new EnderecoTeste
        {
            Logradouro = _faker.Address.StreetName(),
            Numero = _faker.Address.BuildingNumber(),
            Complemento = _faker.Random.Bool(0.3f) ? _faker.Address.SecondaryAddress() : null,
            Bairro = _faker.Address.County(),
            Cep = GerarCep(),
            Cidade = _faker.Address.City(),
            Estado = _faker.Address.StateAbbr(),
            Latitude = _faker.Address.Latitude(-33.7683777, 5.2842873), // Brasil
            Longitude = _faker.Address.Longitude(-73.9872354, -28.6341164) // Brasil
        };
    }

    #endregion

    #region Dados Agrícolas

    /// <summary>
    /// Gera uma área de plantio em hectares
    /// </summary>
    public decimal GerarAreaPlantio(decimal min = 1, decimal max = 10000)
    {
        return _faker.Random.Decimal(min, max);
    }

    /// <summary>
    /// Gera um nome de cultura
    /// </summary>
    public string GerarNomeCultura()
    {
        var culturas = new[]
        {
            "Soja", "Milho", "Algodão", "Feijão", "Arroz", "Trigo", "Cana-de-açúcar",
            "Café", "Sorgo", "Girassol", "Amendoim", "Mandioca", "Batata"
        };
        return _faker.PickRandom(culturas);
    }

    /// <summary>
    /// Gera um nome de produto agrícola
    /// </summary>
    public string GerarNomeProduto()
    {
        var tipos = new[] { "Semente", "Fertilizante", "Defensivo", "Adubo", "Herbicida", "Fungicida", "Inseticida" };
        var culturas = new[] { "Soja", "Milho", "Algodão", "Feijão" };
        
        return $"{_faker.PickRandom(tipos)} para {_faker.PickRandom(culturas)}";
    }

    /// <summary>
    /// Gera um preço para produto
    /// </summary>
    public decimal GerarPreco(decimal min = 10, decimal max = 1000)
    {
        return _faker.Random.Decimal(min, max);
    }

    /// <summary>
    /// Gera uma quantidade
    /// </summary>
    public int GerarQuantidade(int min = 1, int max = 100)
    {
        return _faker.Random.Int(min, max);
    }

    /// <summary>
    /// Gera um peso em kg
    /// </summary>
    public decimal GerarPeso(decimal min = 0.1m, decimal max = 1000m)
    {
        return _faker.Random.Decimal(min, max);
    }

    /// <summary>
    /// Gera dimensões (altura, largura, profundidade) em cm
    /// </summary>
    public DimensoesTeste GerarDimensoes()
    {
        return new DimensoesTeste
        {
            Altura = _faker.Random.Decimal(1, 200),
            Largura = _faker.Random.Decimal(1, 200),
            Profundidade = _faker.Random.Decimal(1, 200)
        };
    }

    #endregion

    #region Datas

    /// <summary>
    /// Gera uma data no passado
    /// </summary>
    public DateTime GerarDataPassado(int diasAtras = 365)
    {
        return _faker.Date.Past(yearsToGoBack: 1, refDate: DateTime.Now.AddDays(-diasAtras));
    }

    /// <summary>
    /// Gera uma data no futuro
    /// </summary>
    public DateTime GerarDataFuturo(int diasAFrente = 365)
    {
        return _faker.Date.Future(yearsToGoForward: 1, refDate: DateTime.Now.AddDays(diasAFrente));
    }

    /// <summary>
    /// Gera uma data entre duas datas
    /// </summary>
    public DateTime GerarDataEntre(DateTime inicio, DateTime fim)
    {
        return _faker.Date.Between(inicio, fim);
    }

    /// <summary>
    /// Gera um período de safra
    /// </summary>
    public PeriodoSafraTeste GerarPeriodoSafra()
    {
        var inicio = GerarDataFuturo(30);
        var fim = inicio.AddMonths(_faker.Random.Int(3, 8));
        
        return new PeriodoSafraTeste
        {
            DataInicioPlantio = inicio,
            DataFimPlantio = fim,
            AnoColheita = fim.Year
        };
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Gera um ID aleatório
    /// </summary>
    public int GerarId(int min = 1, int max = 999999)
    {
        return _faker.Random.Int(min, max);
    }

    /// <summary>
    /// Gera uma string aleatória
    /// </summary>
    public string GerarString(int tamanho = 10)
    {
        return _faker.Random.String2(tamanho);
    }

    /// <summary>
    /// Gera um texto aleatório
    /// </summary>
    public string GerarTexto(int sentencas = 3)
    {
        return _faker.Lorem.Sentences(sentencas);
    }

    /// <summary>
    /// Gera um booleano aleatório
    /// </summary>
    public bool GerarBooleano()
    {
        return _faker.Random.Bool();
    }

    /// <summary>
    /// Escolhe um item aleatório de uma lista
    /// </summary>
    public T EscolherAleatorio<T>(params T[] items)
    {
        return _faker.PickRandom(items);
    }

    /// <summary>
    /// Gera uma lista de itens
    /// </summary>
    public List<T> GerarLista<T>(Func<T> gerador, int min = 1, int max = 5)
    {
        var quantidade = _faker.Random.Int(min, max);
        var lista = new List<T>();
        
        for (int i = 0; i < quantidade; i++)
        {
            lista.Add(gerador());
        }
        
        return lista;
    }

    #endregion
}

#region Classes de Apoio

public class EnderecoTeste
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DimensoesTeste
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Profundidade { get; set; }
}

public class PeriodoSafraTeste
{
    public DateTime DataInicioPlantio { get; set; }
    public DateTime DataFimPlantio { get; set; }
    public int AnoColheita { get; set; }
}

#endregion