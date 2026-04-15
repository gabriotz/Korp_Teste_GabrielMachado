namespace FaturamentoService.Domain.Entities;

public enum StatusNota { Aberta, Fechada }

public class NotaFiscal
{
    public int Id { get; private set; }
    public int Numero { get; private set; }
    public StatusNota Status { get; private set; } = StatusNota.Aberta;
    public DateTime CriadaEm { get; private set; }
    public DateTime AtualizadaEm { get; private set; }
    public List<ItemNota> Itens { get; private set; } = new();

    private NotaFiscal() { }

    public static NotaFiscal Criar(int numero)
    {
        if (numero <= 0)
            throw new ArgumentException("Número deve ser maior que zero.", nameof(numero));

        return new NotaFiscal
        {
            Numero = numero,
            Status = StatusNota.Aberta,
            CriadaEm = DateTime.UtcNow,
            AtualizadaEm = DateTime.UtcNow
        };
    }

    public void AdicionarItem(int produtoId, string produtoDescricao, int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));

        if (string.IsNullOrWhiteSpace(produtoDescricao))
            throw new ArgumentException("Descrição do produto é obrigatória.", nameof(produtoDescricao));

        Itens.Add(ItemNota.Criar(produtoId, produtoDescricao, quantidade));
        AtualizadaEm = DateTime.UtcNow;
    }

    public void Fechar()
    {
        if (Status != StatusNota.Aberta)
            throw new Domain.Exceptions.NotaJaFechadaException(Id);

        if (!Itens.Any())
            throw new Domain.Exceptions.NotaSemItensException(Id);

        Status = StatusNota.Fechada;
        AtualizadaEm = DateTime.UtcNow;
    }
}