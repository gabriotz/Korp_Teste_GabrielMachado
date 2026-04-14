namespace EstoqueService.Domain.Exceptions;

/// <summary>Base para todas as exceções de domínio do EstoqueService.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>Lançada quando se tenta criar um produto com código já existente.</summary>
public sealed class ProdutoDuplicadoException : DomainException
{
    public string Codigo { get; }

    public ProdutoDuplicadoException(string codigo)
        : base($"Já existe um produto com o código '{codigo}'.") 
    { 
        Codigo = codigo;
    }
}

/// <summary>Lançada quando o produto solicitado não é encontrado.</summary>
public sealed class ProdutoNaoEncontradoException : DomainException
{
    public int Id { get; }

    public ProdutoNaoEncontradoException(int id)
        : base($"Produto com Id {id} não encontrado.") 
    { 
        Id = id;
    }
}

/// <summary>Lançada quando a operação resultaria em saldo negativo.</summary>
public sealed class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException(string message) : base(message) { }
}