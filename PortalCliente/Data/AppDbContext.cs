using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PortalCliente.Models;

namespace PortalCliente.Data;

public class AppDbContext : IdentityDbContext<UsuarioAplicacao>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Container> Containers { get; set; }
    public DbSet<ConfiguracaoContato> ConfiguracoesContato { get; set; }
    public DbSet<ObservacaoIsotank> ObservacoesIsotank { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ConfiguracaoContato>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedNever();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Evita exceção quando o modelo tem alterações ainda não refletidas no snapshot/migrations (ex.: ConfiguracaoContato já aplicada via migration anterior).
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}