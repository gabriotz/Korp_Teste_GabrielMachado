using EstoqueService.Application.DTOs;
using EstoqueService.Application.Interfaces;
using EstoqueService.Domain.Entities;
using EstoqueService.Domain.Exceptions;
using EstoqueService.Domain.Interfaces;

namespace EstoqueService.Application.Services;

public sealed class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;

    public ProdutoService(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProdutoResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var produtos = await _repository.GetAllAsync(ct);
        return produtos.Select(ToResponse).ToList().AsReadOnly();
    }

    public async Task<ProdutoResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var produto = await _repository.GetByIdAsync(id, ct)
            ?? throw new ProdutoNaoEncontradoException(id);

        return ToResponse(produto);
    }

    public async Task<ProdutoResponse> CreateAsync(CriarProdutoRequest request, CancellationToken ct = default)
    {
        var codigoNormalizado = request.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;

        var existe = await _repository.ExisteCodigoAsync(codigoNormalizado, ct);
        if (existe)
            throw new ProdutoDuplicadoException(codigoNormalizado);

        // Validações de domínio ficam na entidade (Factory Method)
        var produto = Produto.Criar(request.Codigo, request.Descricao, request.Saldo);

        await _repository.AddAsync(produto, ct);
        await _repository.SaveChangesAsync(ct);

        return ToResponse(produto);
    }

    public async Task AtualizarSaldoAsync(int id, AtualizarSaldoRequest request, CancellationToken ct = default)
    {
        var produto = await _repository.GetByIdAsync(id, ct)
            ?? throw new ProdutoNaoEncontradoException(id);

        // Regra de negócio encapsulada na entidade
        produto.AtualizarSaldo(request.Quantidade);

        await _repository.SaveChangesAsync(ct);
    }

    // ── Mapeamento ──────────────────────────────────────────────────────────

    private static ProdutoResponse ToResponse(Produto p) => new(
        p.Id,
        p.Codigo,
        p.Descricao,
        p.Saldo,
        p.CriadoEm,
        p.AtualizadoEm
    );
}