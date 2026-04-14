namespace EstoqueService.Application.DTOs;

// ── Entrada ────────────────────────────────────────────────────────────────

/// <summary>Payload para criação de produto.</summary>
public sealed record CriarProdutoRequest(
    string Codigo,
    string Descricao,
    int Saldo
);

/// <summary>Payload para atualização de saldo (positivo = entrada, negativo = saída).</summary>
public sealed record AtualizarSaldoRequest(int Quantidade);

// ── Saída ──────────────────────────────────────────────────────────────────

/// <summary>Projeção de Produto retornada nas respostas da API.</summary>
public sealed record ProdutoResponse(
    int Id,
    string Codigo,
    string Descricao,
    int Saldo,
    DateTime CriadoEm,
    DateTime AtualizadoEm
);

/// <summary>Envelope padrão para erros de negócio.</summary>
public sealed record ErrorResponse(string Message, string? Detail = null);