using FaturamentoService.Domain.Entities;

namespace FaturamentoService.Application.DTOs;

// ── Entrada ────────────────────────────────────────────────────────────────

public sealed record CriarItemRequest(int ProdutoId, int Quantidade);

public sealed record CriarNotaRequest(List<CriarItemRequest> Itens);

// ── Saída ──────────────────────────────────────────────────────────────────

public sealed record ItemNotaResponse(
    int Id,
    int ProdutoId,
    string ProdutoDescricao,
    int Quantidade
);

public sealed record NotaFiscalResponse(
    int Id,
    int Numero,
    string Status,
    DateTime CriadaEm,
    DateTime AtualizadaEm,
    List<ItemNotaResponse> Itens
);

public sealed record ProdutoDto(
    int Id,
    string Codigo,
    string Descricao,
    int Saldo
);

public sealed record ErrorResponse(string Message, string? Detail = null);