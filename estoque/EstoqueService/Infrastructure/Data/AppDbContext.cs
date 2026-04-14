using EstoqueService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Codigo)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(p => p.Descricao)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(p => p.Saldo)
                  .IsRequired();

            entity.Property(p => p.CriadoEm)
                  .IsRequired();

            entity.Property(p => p.AtualizadoEm)
                  .IsRequired();

            // Índice único no código — LINQ/Fluent API (citável no vídeo)
            entity.HasIndex(p => p.Codigo)
                  .IsUnique()
                  .HasDatabaseName("IX_Produtos_Codigo");
        });
    }
}