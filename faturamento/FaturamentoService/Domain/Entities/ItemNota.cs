namespace FaturamentoService.Domain.Entities;

public class ItemNota
{
    public int Id { get; private set; }
    public int NotaFiscalId { get; private set; }
    public int ProdutoId { get; private set; }
    public string ProdutoDescricao { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public NotaFiscal NotaFiscal { get; private set; } = null!;

    private ItemNota() { }

    public static ItemNota Criar(int produtoId, string produtoDescricao, int quantidade)
    {
        return new ItemNota
        {
            ProdutoId = produtoId,
            ProdutoDescricao = produtoDescricao.Trim(),
            Quantidade = quantidade
        };
    }
}