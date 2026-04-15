using FaturamentoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNota> ItenNota => Set<ItemNota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.ToTable("NotasFiscais");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Numero).IsRequired();
            entity.Property(n => n.Status).IsRequired();
            entity.Property(n => n.CriadaEm).IsRequired();
            entity.Property(n => n.AtualizadaEm).IsRequired();

            // Índice único no número da nota
            entity.HasIndex(n => n.Numero)
                  .IsUnique()
                  .HasDatabaseName("IX_NotasFiscais_Numero");

            entity.HasMany(n => n.Itens)
                  .WithOne(i => i.NotaFiscal)
                  .HasForeignKey(i => i.NotaFiscalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNota>(entity =>
        {
            entity.ToTable("ItensNota");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProdutoId).IsRequired();
            entity.Property(i => i.ProdutoDescricao).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantidade).IsRequired();
        });
    }
}