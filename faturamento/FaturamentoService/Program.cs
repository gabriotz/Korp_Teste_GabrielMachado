using FaturamentoService.API.Middleware;
using FaturamentoService.Application.Interfaces;
using FaturamentoService.Application.Services;
using FaturamentoService.Domain.Interfaces;
using FaturamentoService.Infrastructure.Data;
using FaturamentoService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Infra ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ── HttpClient para comunicação com o EstoqueService ───────────────────────
builder.Services.AddHttpClient("Estoque", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EstoqueUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// ── DI ─────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

// ── API ────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

var app = builder.Build();

// ── Migrations automáticas ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ── Pipeline ───────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();

app.Run();