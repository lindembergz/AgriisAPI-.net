namespace Agriis.Compartilhado.Dominio.Enums;

/// <summary>
/// Roles/perfis de usuário no sistema
/// </summary>
public enum Roles
{
    /// <summary>
    /// Comprador/Produtor
    /// </summary>
    RoleComprador = 1,
    
    /// <summary>
    /// Administrador do sistema
    /// </summary>
    RoleAdmin = 2,
    
    /// <summary>
    /// Administrador web do fornecedor
    /// </summary>
    RoleFornecedorWebAdmin = 3,
    
    /// <summary>
    /// Representante comercial do fornecedor
    /// </summary>
    RoleFornecedorWebRepresentante = 4
}

/// <summary>
/// Tipos de entrega para BARTER
/// </summary>
public enum BarterTipoEntrega
{
    /// <summary>
    /// Free On Board - mercadoria entregue no local de embarque
    /// </summary>
    Fob = 0,
    
    /// <summary>
    /// Cost, Insurance and Freight - mercadoria entregue no destino
    /// </summary>
    Cif = 1
}

/// <summary>
/// Modalidade de pagamento
/// </summary>
public enum ModalidadePagamento
{
    /// <summary>
    /// Pagamento via troca por produtos agrícolas
    /// </summary>
    Barter = 0,
    
    /// <summary>
    /// Pagamento monetário normal
    /// </summary>
    Normal = 1
}

/// <summary>
/// Tipo de cálculo de cubagem
/// </summary>
public enum CalculoCubagem
{
    /// <summary>
    /// Cálculo baseado na densidade do produto
    /// </summary>
    Densidade = 0,
    
    /// <summary>
    /// Cálculo baseado em 300kg/m³
    /// </summary>
    Base300KgM3 = 1
}

/// <summary>
/// Tipo de cálculo de frete
/// </summary>
public enum CalculoFrete
{
    /// <summary>
    /// Baseado no peso nominal
    /// </summary>
    PesoNominal = 0,
    
    /// <summary>
    /// Baseado no peso cubado
    /// </summary>
    PesoCubado = 1
}

/// <summary>
/// Classificação de produto
/// </summary>
public enum ClassificacaoProduto
{
    /// <summary>
    /// Classificação A - alta prioridade
    /// </summary>
    A = 0,
    
    /// <summary>
    /// Classificação B - média prioridade
    /// </summary>
    B = 1,
    
    /// <summary>
    /// Classificação C - baixa prioridade
    /// </summary>
    C = 2,
    
    /// <summary>
    /// Classificação D - menor prioridade
    /// </summary>
    D = 3
}

/// <summary>
/// Tipo de acesso para auditoria
/// </summary>
public enum TipoAcessoAuditoria
{
    /// <summary>
    /// Acesso de produtor
    /// </summary>
    Produtor = 1,
    
    /// <summary>
    /// Acesso de consultor agronômico
    /// </summary>
    ConsultorAgronomico = 2,
    
    /// <summary>
    /// Acesso de fornecedor
    /// </summary>
    Fornecedor = 3,
    
    /// <summary>
    /// Outros tipos de acesso
    /// </summary>
    Outros = 4
}

/// <summary>
/// Status de tentativa no SERPRO
/// </summary>
public enum StatusTentativaSerpro
{
    /// <summary>
    /// Autorizado pelo SERPRO
    /// </summary>
    Autorizado = 1,
    
    /// <summary>
    /// Não autorizado pelo SERPRO
    /// </summary>
    NaoAutorizado = 2
}