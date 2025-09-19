namespace Agriis.Compartilhado.Dominio.Enums;

/// <summary>
/// Tipos de unidade de medida
/// </summary>
public enum TipoUnidade
{
    Quilo = 1,
    Tonelada = 2,
    Litro = 3,
    Dose = 4,
    Frasco = 5,
    Sementes = 6,
    Ovos = 7,
    Parasitoide = 8,
    Hectare = 9
}

/// <summary>
/// Tipos de moeda
/// </summary>
public enum Moeda
{
    Real = 1,
    Dolar = 2
}

/// <summary>
/// Status genérico para entidades
/// </summary>
public enum StatusGenerico
{
    Ativo = 1,
    Inativo = 2,
    Pendente = 3,
    Aprovado = 4,
    Rejeitado = 5,
    Cancelado = 6
}

/// <summary>
/// Tipos de operação para auditoria
/// </summary>
public enum TipoOperacao
{
    Criacao = 1,
    Atualizacao = 2,
    Exclusao = 3,
    Consulta = 4
}

/// <summary>
/// Níveis de log
/// </summary>
public enum NivelLog
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

/// <summary>
/// Tipos de documento brasileiro
/// </summary>
public enum TipoDocumento
{
    CPF = 1,
    CNPJ = 2,
    RG = 3,
    InscricaoEstadual = 4,
    InscricaoMunicipal = 5
}

/// <summary>
/// Estados brasileiros
/// </summary>
public enum EstadoBrasil
{
    AC = 1,  // Acre
    AL = 2,  // Alagoas
    AP = 3,  // Amapá
    AM = 4,  // Amazonas
    BA = 5,  // Bahia
    CE = 6,  // Ceará
    DF = 7,  // Distrito Federal
    ES = 8,  // Espírito Santo
    GO = 9,  // Goiás
    MA = 10, // Maranhão
    MT = 11, // Mato Grosso
    MS = 12, // Mato Grosso do Sul
    MG = 13, // Minas Gerais
    PA = 14, // Pará
    PB = 15, // Paraíba
    PR = 16, // Paraná
    PE = 17, // Pernambuco
    PI = 18, // Piauí
    RJ = 19, // Rio de Janeiro
    RN = 20, // Rio Grande do Norte
    RS = 21, // Rio Grande do Sul
    RO = 22, // Rondônia
    RR = 23, // Roraima
    SC = 24, // Santa Catarina
    SP = 25, // São Paulo
    SE = 26, // Sergipe
    TO = 27  // Tocantins
}

/// <summary>
/// Tipos de endereço
/// </summary>
public enum TipoEndereco
{
    Residencial = 1,
    Comercial = 2,
    Rural = 3,
    Correspondencia = 4
}

/// <summary>
/// Tipos de contato
/// </summary>
public enum TipoContato
{
    Telefone = 1,
    Celular = 2,
    WhatsApp = 3,
    Email = 4,
    Fax = 5
}

/// <summary>
/// Tipos de arquivo
/// </summary>
public enum TipoArquivo
{
    Imagem = 1,
    Documento = 2,
    Planilha = 3,
    PDF = 4,
    Video = 5,
    Audio = 6
}

/// <summary>
/// Extensões de arquivo permitidas
/// </summary>
public enum ExtensaoArquivo
{
    JPG = 1,
    JPEG = 2,
    PNG = 3,
    GIF = 4,
    PDF = 5,
    DOC = 6,
    DOCX = 7,
    XLS = 8,
    XLSX = 9,
    TXT = 10,
    CSV = 11
}