using FaturamentoService.Domain.Entities;

namespace FaturamentoService.Domain.Interfaces;

public interface INotaFiscalRepository
{
    Task<IReadOnlyList<NotaFiscal>> GetAllAsync(CancellationToken ct = default);
    Task<NotaFiscal?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> GetProximoNumeroAsync(CancellationToken ct = default);
    Task AddAsync(NotaFiscal nota, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}