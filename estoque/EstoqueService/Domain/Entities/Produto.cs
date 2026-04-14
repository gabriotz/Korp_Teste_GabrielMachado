namespace EstoqueService.Domain.Entities;

public class Produto
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public int Saldo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    // EF Core precisa de construtor sem parâmetros
    private Produto() { }

    public static Produto Criar(string codigo, string descricao, int saldo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código é obrigatório.", nameof(codigo));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        if (saldo < 0)
            throw new ArgumentException("Saldo não pode ser negativo.", nameof(saldo));

        return new Produto
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descricao = descricao.Trim(),
            Saldo = saldo,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };
    }

    public void AtualizarSaldo(int quantidade)
    {
        if (Saldo + quantidade < 0)
            throw new Domain.Exceptions.SaldoInsuficienteException(
                $"Saldo insuficiente. Saldo atual: {Saldo}, tentativa de débito: {Math.Abs(quantidade)}."
            );

        Saldo += quantidade;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        Descricao = descricao.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }
}