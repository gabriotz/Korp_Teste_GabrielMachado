using EstoqueService.Domain.Entities;

namespace EstoqueService.Domain.Interfaces;

/// <summary>
/// Contrato para persistência de Produtos.
/// A camada de Domínio define a interface; a Infrastructure a implementa.
/// </summary>
public interface IProdutoRepository
{
    Task<IReadOnlyList<Produto>> GetAllAsync(CancellationToken ct = default);
    Task<Produto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);
    Task AddAsync(Produto produto, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}