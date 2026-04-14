using System.Text.Json;
using EstoqueService.Application.DTOs;
using EstoqueService.Domain.Exceptions;

namespace EstoqueService.API.Middleware;

/// <summary>
/// Captura exceções não tratadas e as converte em respostas HTTP padronizadas.
/// Isso centraliza o tratamento de erros e mantém os controllers limpos.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            ProdutoNaoEncontradoException ex =>
                (StatusCodes.Status404NotFound,
                 new ErrorResponse(ex.Message)),

            ProdutoDuplicadoException ex =>
                (StatusCodes.Status409Conflict,
                 new ErrorResponse(ex.Message)),

            SaldoInsuficienteException ex =>
                (StatusCodes.Status422UnprocessableEntity,
                 new ErrorResponse(ex.Message)),

            ArgumentException ex =>
                (StatusCodes.Status400BadRequest,
                 new ErrorResponse(ex.Message)),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 new ErrorResponse("Erro interno no servidor.", exception.Message))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}