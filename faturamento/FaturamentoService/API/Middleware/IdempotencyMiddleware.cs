using System.Text;
using System.Text.Json;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.API.Middleware;

public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // Só aplica em POSTs
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        // Verifica se tem o header
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
        {
            await _next(context);
            return;
        }

        // Verifica se já existe no banco
        var existing = await db.IdempotencyRecords
            .FirstOrDefaultAsync(r => r.Key == key.ToString());

        if (existing != null)
        {
            // Retorna a resposta cacheada
            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(existing.Response);
            return;
        }

        // Captura a resposta original
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;

        // Salva no banco só se foi sucesso
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var record = IdempotencyRecord.Criar(key.ToString(), responseBody, context.Response.StatusCode);
            db.IdempotencyRecords.Add(record);
            await db.SaveChangesAsync();
        }
    }
}