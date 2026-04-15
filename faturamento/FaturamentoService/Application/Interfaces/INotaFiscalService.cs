using FaturamentoService.Application.DTOs;

namespace FaturamentoService.Application.Interfaces;

public interface INotaFiscalService
{
    Task<IReadOnlyList<NotaFiscalResponse>> GetAllAsync(CancellationToken ct = default);
    Task<NotaFiscalResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ProdutoDto>> GetProdutosDisponiveisAsync(CancellationToken ct = default);
    Task<NotaFiscalResponse> CreateAsync(CriarNotaRequest request, CancellationToken ct = default);
    Task<NotaFiscalResponse> ImprimirAsync(int id, CancellationToken ct = default);
}