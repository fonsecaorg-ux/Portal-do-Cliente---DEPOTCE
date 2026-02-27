using Microsoft.EntityFrameworkCore;
using SimuladorMBM.Models;

namespace SimuladorMBM.Data;

public class MbmDbContext : DbContext
{
    public MbmDbContext(DbContextOptions<MbmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Isotanque> Isotanques { get; set; }
    public DbSet<StatusIsotanque> StatusIsotanques { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Isotanque>()
            .HasOne(i => i.Cliente)
            .WithMany(c => c.Isotanques)
            .HasForeignKey(i => i.ClienteId);

        modelBuilder.Entity<Isotanque>()
            .HasOne(i => i.Produto)
            .WithMany(p => p.Isotanques)
            .HasForeignKey(i => i.ProdutoId);

        modelBuilder.Entity<Isotanque>()
            .HasIndex(i => i.Codigo)
            .IsUnique();
    }
}
