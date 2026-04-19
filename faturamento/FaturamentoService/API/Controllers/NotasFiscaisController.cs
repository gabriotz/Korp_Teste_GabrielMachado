using FaturamentoService.Application.DTOs;
using FaturamentoService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.API.Controllers;

[ApiController]
[Route("notas-fiscais")]
[Produces("application/json")]
public sealed class NotasFiscaisController : ControllerBase
{
    private readonly INotaFiscalService _service;

    public NotasFiscaisController(INotaFiscalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var notas = await _service.GetAllAsync(ct);
        return Ok(notas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var nota = await _service.GetByIdAsync(id, ct);
        return Ok(nota);
    }

    [HttpGet("produtos-disponiveis")]
    public async Task<IActionResult> GetProdutos(CancellationToken ct)
    {
        var produtos = await _service.GetProdutosDisponiveisAsync(ct);
        return Ok(produtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarNotaRequest request, CancellationToken ct)
    {
        var criada = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
    }

    [HttpPost("{id:int}/imprimir")]
    public async Task<IActionResult> Imprimir(int id, CancellationToken ct)
    {
        var nota = await _service.ImprimirAsync(id, ct);
        return Ok(nota);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", service = "FaturamentoService" });

    [HttpPost("{id:int}/resumo")]
    public async Task<IActionResult> GerarResumo(int id, CancellationToken ct)
    {
        var resumo = await _service.GerarResumoAsync(id, ct);
        return Ok(resumo);
    }
}