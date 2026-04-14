using EstoqueService.Application.DTOs;

namespace EstoqueService.Application.Interfaces;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ProdutoResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProdutoResponse> CreateAsync(CriarProdutoRequest request, CancellationToken ct = default);
    Task AtualizarSaldoAsync(int id, AtualizarSaldoRequest request, CancellationToken ct = default);
}