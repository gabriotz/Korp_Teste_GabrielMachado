using FaturamentoService.Application.DTOs;
using FaturamentoService.Application.Interfaces;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Exceptions;
using FaturamentoService.Domain.Interfaces;

namespace FaturamentoService.Application.Services;

public sealed class NotaFiscalService : INotaFiscalService
{
    private readonly INotaFiscalRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotaFiscalService(INotaFiscalRepository repository, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<NotaFiscalResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var notas = await _repository.GetAllAsync(ct);
        return notas.Select(ToResponse).ToList().AsReadOnly();
    }

    public async Task<NotaFiscalResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var nota = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotaNaoEncontradaException(id);

        return ToResponse(nota);
    }

    public async Task<IReadOnlyList<ProdutoDto>> GetProdutosDisponiveisAsync(CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Estoque");
            var response = await client.GetAsync("/produtos", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProdutoDto>>(cancellationToken: ct) ?? new();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new EstoqueIndisponivelException();
        }
    }

    public async Task<NotaFiscalResponse> CreateAsync(CriarNotaRequest request, CancellationToken ct = default)
    {
        if (request.Itens == null || !request.Itens.Any())
            throw new ArgumentException("A nota deve ter ao menos um item.");

        // Busca produtos do Estoque pra guardar a descrição no item
        var produtos = await GetProdutosDisponiveisAsync(ct);

        // LINQ — MaxAsync via repositório (numeração sequencial)
        var numero = await _repository.GetProximoNumeroAsync(ct);
        var nota = NotaFiscal.Criar(numero);

        foreach (var item in request.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId)
                ?? throw new ArgumentException($"Produto {item.ProdutoId} não encontrado no estoque.");

            nota.AdicionarItem(produto.Id, produto.Descricao, item.Quantidade);
        }

        await _repository.AddAsync(nota, ct);
        await _repository.SaveChangesAsync(ct);

        return ToResponse(nota);
    }

    public async Task<NotaFiscalResponse> ImprimirAsync(int id, CancellationToken ct = default)
    {
        var nota = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotaNaoEncontradaException(id);

        // Valida regras de domínio (lança NotaJaFechadaException ou NotaSemItensException)
        nota.Fechar();

        // Debita saldo de cada item no Estoque
        var client = _httpClientFactory.CreateClient("Estoque");
        foreach (var item in nota.Itens)
        {
            try
            {
                var response = await client.PatchAsJsonAsync(
                    $"/produtos/{item.ProdutoId}/saldo",
                    new { Quantidade = -item.Quantidade },
                    cancellationToken: ct
                );

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync(ct);
                    throw new SaldoInsuficienteException($"Erro ao debitar produto {item.ProdutoId}: {erro}");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new EstoqueIndisponivelException();
            }
        }

        await _repository.SaveChangesAsync(ct);
        return ToResponse(nota);
    }

    // ── Mapeamento ──────────────────────────────────────────────────────────

    private static NotaFiscalResponse ToResponse(NotaFiscal n) => new(
        n.Id,
        n.Numero,
        n.Status.ToString(),
        n.CriadaEm,
        n.AtualizadaEm,
        n.Itens.Select(i => new ItemNotaResponse(i.Id, i.ProdutoId, i.ProdutoDescricao, i.Quantidade)).ToList()
    );
}