using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Culturas.Dominio.Entidades;

public class Cultura : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }

    protected Cultura() { } // EF Constructor

    public Cultura(string nome, string? descricao = null)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        Ativo = true;
    }

    public void AtualizarNome(string nome)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        AtualizarDataModificacao();
    }

    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }

    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }
}