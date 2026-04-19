using System.Text.Json;
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

    private readonly IConfiguration _configuration;

    public NotaFiscalService(INotaFiscalRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

    var produtos = await GetProdutosDisponiveisAsync(ct);

    // Transaction garante que dois requests simultâneos não pegam o mesmo número
    await using var transaction = await _repository.BeginTransactionAsync(ct);
    try
    {
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
        await transaction.CommitAsync(ct);

        return ToResponse(nota);
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
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

    public async Task<ResumoNotaResponse> GerarResumoAsync(int id, CancellationToken ct = default)
{

    var nota = await _repository.GetByIdAsync(id, ct)
        ?? throw new NotaNaoEncontradaException(id);

    var itensTexto = string.Join(", ", nota.Itens.Select(i => $"{i.Quantidade}x {i.ProdutoDescricao}"));

    var prompt = $@"Gere um resumo curto e profissional em português de uma nota fiscal com os seguintes dados:
- Número da nota: {nota.Numero}
- Status: {nota.Status}
- Total de itens: {nota.Itens.Count}
- Produtos: {itensTexto}
- Data de emissão: {nota.CriadaEm:dd/MM/yyyy HH:mm}

O resumo deve ter no máximo 2 frases, ser direto e informativo.";

    var apiKey = _configuration["Gemini:ApiKey"];
    Console.WriteLine($"API KEY: '{apiKey}'");

    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

    var body = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
    };

    try
{
    var client = _httpClientFactory.CreateClient();
    var response = await client.PostAsJsonAsync(url, body, ct);
    
    var conteudo = await response.Content.ReadAsStringAsync(ct);
    
    if (!response.IsSuccessStatusCode)
        throw new Exception($"Gemini retornou {response.StatusCode}: {conteudo}");

    var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    var resumo = result
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString() ?? "Resumo indisponível.";

    return new ResumoNotaResponse(resumo.Trim());
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO GEMINI: {ex.Message}");
    return new ResumoNotaResponse("Não foi possível gerar o resumo da nota.");
}
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