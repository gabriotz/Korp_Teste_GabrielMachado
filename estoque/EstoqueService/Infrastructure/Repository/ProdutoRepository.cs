using EstoqueService.Domain.Entities;
using EstoqueService.Domain.Interfaces;
using EstoqueService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Infrastructure.Repositories;

public sealed class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Produto>> GetAllAsync(CancellationToken ct = default)
    {
        // LINQ — OrderBy (citável no vídeo)
        return await _context.Produtos
            .OrderBy(p => p.Descricao)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Produto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Produtos.FindAsync([id], ct);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default)
    {
        // LINQ — AnyAsync (citável no vídeo)
        return await _context.Produtos
            .AnyAsync(p => p.Codigo == codigo, ct);
    }

    public async Task AddAsync(Produto produto, CancellationToken ct = default)
    {
        await _context.Produtos.AddAsync(produto, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}