using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Entidades;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace Agriis.PontosDistribuicao.Dominio.Entidades;

/// <summary>
/// Entidade que representa um ponto de distribuição de produtos
/// </summary>
public class PontoDistribuicao : EntidadeBase
{
    /// <summary>
    /// Nome do ponto de distribuição
    /// </summary>
    public string Nome { get; private set; } = string.Empty;
    
    /// <summary>
    /// Descrição do ponto de distribuição
    /// </summary>
    public string? Descricao { get; private set; }
    
    /// <summary>
    /// Indica se o ponto está ativo
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// ID do fornecedor proprietário do ponto
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// ID do endereço do ponto de distribuição
    /// </summary>
    public int EnderecoId { get; private set; }
    
    /// <summary>
    /// Endereço do ponto de distribuição
    /// </summary>
    public virtual Endereco Endereco { get; private set; } = null!;
    
    /// <summary>
    /// Estrutura JSON com cobertura territorial (estados e municípios atendidos)
    /// Formato: { "estados": [1, 2, 3], "municipios": [100, 200, 300] }
    /// </summary>
    public JsonDocument? CoberturaTerritorios { get; private set; }
    
    /// <summary>
    /// Raio de cobertura em quilômetros (opcional)
    /// </summary>
    public double? RaioCobertura { get; private set; }
    
    /// <summary>
    /// Capacidade máxima de armazenamento (opcional)
    /// </summary>
    public decimal? CapacidadeMaxima { get; private set; }
    
    /// <summary>
    /// Unidade da capacidade (toneladas, m³, etc.)
    /// </summary>
    public string? UnidadeCapacidade { get; private set; }
    
    /// <summary>
    /// Horário de funcionamento (JSON)
    /// Formato: { "segunda": "08:00-18:00", "terca": "08:00-18:00", ... }
    /// </summary>
    public JsonDocument? HorarioFuncionamento { get; private set; }
    
    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Observacoes { get; private set; }
    
    /// <summary>
    /// Construtor protegido para o Entity Framework
    /// </summary>
    protected PontoDistribuicao() { }
    
    /// <summary>
    /// Construtor para criar um novo ponto de distribuição
    /// </summary>
    /// <param name="nome">Nome do ponto</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="enderecoId">ID do endereço</param>
    /// <param name="descricao">Descrição do ponto</param>
    /// <param name="raioCobertura">Raio de cobertura em km</param>
    /// <param name="capacidadeMaxima">Capacidade máxima</param>
    /// <param name="unidadeCapacidade">Unidade da capacidade</param>
    /// <param name="observacoes">Observações</param>
    public PontoDistribuicao(string nome, int fornecedorId, int enderecoId, 
                           string? descricao = null, double? raioCobertura = null,
                           decimal? capacidadeMaxima = null, string? unidadeCapacidade = null,
                           string? observacoes = null)
    {
        ValidarParametros(nome, fornecedorId, enderecoId);
        
        Nome = nome;
        FornecedorId = fornecedorId;
        EnderecoId = enderecoId;
        Descricao = descricao;
        RaioCobertura = raioCobertura;
        CapacidadeMaxima = capacidadeMaxima;
        UnidadeCapacidade = unidadeCapacidade;
        Observacoes = observacoes;
        Ativo = true;
    }
    
    /// <summary>
    /// Atualiza os dados básicos do ponto de distribuição
    /// </summary>
    /// <param name="nome">Nome do ponto</param>
    /// <param name="descricao">Descrição do ponto</param>
    /// <param name="raioCobertura">Raio de cobertura em km</param>
    /// <param name="capacidadeMaxima">Capacidade máxima</param>
    /// <param name="unidadeCapacidade">Unidade da capacidade</param>
    /// <param name="observacoes">Observações</param>
    public void Atualizar(string nome, string? descricao = null, double? raioCobertura = null,
                         decimal? capacidadeMaxima = null, string? unidadeCapacidade = null,
                         string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
            
        Nome = nome;
        Descricao = descricao;
        RaioCobertura = raioCobertura;
        CapacidadeMaxima = capacidadeMaxima;
        UnidadeCapacidade = unidadeCapacidade;
        Observacoes = observacoes;
        
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define a cobertura territorial do ponto de distribuição
    /// </summary>
    /// <param name="estadosIds">Lista de IDs dos estados atendidos</param>
    /// <param name="municipiosIds">Lista de IDs dos municípios atendidos</param>
    public void DefinirCoberturaTerritorios(IEnumerable<int>? estadosIds = null, IEnumerable<int>? municipiosIds = null)
    {
        var cobertura = new
        {
            estados = estadosIds?.ToList() ?? new List<int>(),
            municipios = municipiosIds?.ToList() ?? new List<int>()
        };
        
        CoberturaTerritorios = JsonSerializer.SerializeToDocument(cobertura);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Define o horário de funcionamento do ponto
    /// </summary>
    /// <param name="horarios">Dicionário com os horários por dia da semana</param>
    public void DefinirHorarioFuncionamento(Dictionary<string, string> horarios)
    {
        if (horarios == null || !horarios.Any())
        {
            HorarioFuncionamento = null;
            AtualizarDataModificacao();
            return;
        }
        
        // Validar dias da semana válidos
        var diasValidos = new[] { "segunda", "terca", "quarta", "quinta", "sexta", "sabado", "domingo" };
        var diasInvalidos = horarios.Keys.Where(dia => !diasValidos.Contains(dia.ToLower())).ToList();
        
        if (diasInvalidos.Any())
            throw new ArgumentException($"Dias inválidos: {string.Join(", ", diasInvalidos)}");
        
        HorarioFuncionamento = JsonSerializer.SerializeToDocument(horarios);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa o ponto de distribuição
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Desativa o ponto de distribuição
    /// </summary>
    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Verifica se o ponto atende um estado específico
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <returns>True se atende o estado</returns>
    public bool AtendeEstado(int estadoId)
    {
        if (CoberturaTerritorios == null)
            return false;
            
        try
        {
            var estados = CoberturaTerritorios.RootElement.GetProperty("estados");
            return estados.EnumerateArray().Any(e => e.GetInt32() == estadoId);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Verifica se o ponto atende um município específico
    /// </summary>
    /// <param name="municipioId">ID do município</param>
    /// <returns>True se atende o município</returns>
    public bool AtendeMunicipio(int municipioId)
    {
        if (CoberturaTerritorios == null)
            return false;
            
        try
        {
            var municipios = CoberturaTerritorios.RootElement.GetProperty("municipios");
            return municipios.EnumerateArray().Any(m => m.GetInt32() == municipioId);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Calcula a distância em quilômetros para um endereço
    /// </summary>
    /// <param name="endereco">Endereço de destino</param>
    /// <returns>Distância em quilômetros ou null se não houver localização</returns>
    public double? CalcularDistanciaKm(Endereco endereco)
    {
        return Endereco?.CalcularDistanciaKm(endereco);
    }
    
    /// <summary>
    /// Calcula a distância em quilômetros para um município
    /// </summary>
    /// <param name="municipio">Município de destino</param>
    /// <returns>Distância em quilômetros ou null se não houver localização</returns>
    public double? CalcularDistanciaKm(Municipio municipio)
    {
        return Endereco?.CalcularDistanciaKm(municipio);
    }
    
    /// <summary>
    /// Verifica se o ponto está dentro do raio de cobertura para um endereço
    /// </summary>
    /// <param name="endereco">Endereço a verificar</param>
    /// <returns>True se está dentro do raio de cobertura</returns>
    public bool EstaDentroRaioCobertura(Endereco endereco)
    {
        if (!RaioCobertura.HasValue)
            return true; // Se não tem raio definido, considera que atende
            
        var distancia = CalcularDistanciaKm(endereco);
        return distancia.HasValue && distancia.Value <= RaioCobertura.Value;
    }
    
    /// <summary>
    /// Verifica se o ponto está dentro do raio de cobertura para um município
    /// </summary>
    /// <param name="municipio">Município a verificar</param>
    /// <returns>True se está dentro do raio de cobertura</returns>
    public bool EstaDentroRaioCobertura(Municipio municipio)
    {
        if (!RaioCobertura.HasValue)
            return true; // Se não tem raio definido, considera que atende
            
        var distancia = CalcularDistanciaKm(municipio);
        return distancia.HasValue && distancia.Value <= RaioCobertura.Value;
    }
    
    /// <summary>
    /// Obtém a lista de IDs dos estados atendidos
    /// </summary>
    /// <returns>Lista de IDs dos estados</returns>
    public List<int> ObterEstadosAtendidos()
    {
        if (CoberturaTerritorios == null)
            return new List<int>();
            
        try
        {
            var estados = CoberturaTerritorios.RootElement.GetProperty("estados");
            return estados.EnumerateArray().Select(e => e.GetInt32()).ToList();
        }
        catch
        {
            return new List<int>();
        }
    }
    
    /// <summary>
    /// Obtém a lista de IDs dos municípios atendidos
    /// </summary>
    /// <returns>Lista de IDs dos municípios</returns>
    public List<int> ObterMunicipiosAtendidos()
    {
        if (CoberturaTerritorios == null)
            return new List<int>();
            
        try
        {
            var municipios = CoberturaTerritorios.RootElement.GetProperty("municipios");
            return municipios.EnumerateArray().Select(m => m.GetInt32()).ToList();
        }
        catch
        {
            return new List<int>();
        }
    }
    
    /// <summary>
    /// Obtém o horário de funcionamento formatado
    /// </summary>
    /// <returns>Dicionário com os horários por dia da semana</returns>
    public Dictionary<string, string> ObterHorarioFuncionamento()
    {
        if (HorarioFuncionamento == null)
            return new Dictionary<string, string>();
            
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(HorarioFuncionamento) 
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    
    /// <summary>
    /// Verifica se o ponto possui capacidade definida
    /// </summary>
    /// <returns>True se possui capacidade máxima definida</returns>
    public bool PossuiCapacidadeDefinida()
    {
        return CapacidadeMaxima.HasValue && CapacidadeMaxima.Value > 0;
    }
    
    private static void ValidarParametros(string nome, int fornecedorId, int enderecoId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
            
        if (fornecedorId <= 0)
            throw new ArgumentException("ID do fornecedor deve ser maior que zero", nameof(fornecedorId));
            
        if (enderecoId <= 0)
            throw new ArgumentException("ID do endereço deve ser maior que zero", nameof(enderecoId));
    }
}