using EstoqueService.Application.DTOs;
using EstoqueService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueService.API.Controllers;

[ApiController]
[Route("produtos")]
[Produces("application/json")]
public sealed class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os produtos ordenados por descrição.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var produtos = await _service.GetAllAsync(ct);
        return Ok(produtos);
    }

    /// <summary>Retorna um produto pelo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var produto = await _service.GetByIdAsync(id, ct);
        return Ok(produto);
    }

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CriarProdutoRequest request, CancellationToken ct)
    {
        var criado = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    /// <summary>
    /// Atualiza o saldo do produto.
    /// Quantidade positiva = entrada de estoque.
    /// Quantidade negativa = saída de estoque.
    /// </summary>
    [HttpPatch("{id:int}/saldo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarSaldo(int id, [FromBody] AtualizarSaldoRequest request, CancellationToken ct)
    {
        await _service.AtualizarSaldoAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Health check do serviço.</summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health() => Ok(new { status = "ok", service = "EstoqueService" });
}