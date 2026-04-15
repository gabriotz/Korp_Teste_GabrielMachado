namespace FaturamentoService.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class NotaNaoEncontradaException : DomainException
{
    public NotaNaoEncontradaException(int id)
        : base($"Nota fiscal com Id {id} não encontrada.") { }
}

public sealed class NotaJaFechadaException : DomainException
{
    public NotaJaFechadaException(int id)
        : base($"Nota fiscal {id} já está fechada.") { }
}

public sealed class NotaSemItensException : DomainException
{
    public NotaSemItensException(int id)
        : base($"Nota fiscal {id} não possui itens.") { }
}

public sealed class EstoqueIndisponivelException : DomainException
{
    public EstoqueIndisponivelException()
        : base("Serviço de Estoque indisponível.") { }
}

public sealed class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException(string message) : base(message) { }
}