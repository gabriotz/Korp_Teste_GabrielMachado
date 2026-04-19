using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Interfaces;
using FaturamentoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Infrastructure.Repositories;

public sealed class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly AppDbContext _context;

    public NotaFiscalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotaFiscal>> GetAllAsync(CancellationToken ct = default)
    {
        // LINQ — Include para carregar os itens junto com a nota
        return await _context.NotasFiscais
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<NotaFiscal?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // LINQ — Include + FirstOrDefaultAsync
        return await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<int> GetProximoNumeroAsync(CancellationToken ct = default)
    {
        // LINQ — MaxAsync para numeração sequencial
        var ultimo = await _context.NotasFiscais
            .MaxAsync(n => (int?)n.Numero, ct) ?? 0;

        return ultimo + 1;
    }

    public async Task AddAsync(NotaFiscal nota, CancellationToken ct = default)
    {
        await _context.NotasFiscais.AddAsync(nota, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
{
    return await _context.Database.BeginTransactionAsync(ct);
}
}