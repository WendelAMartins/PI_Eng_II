using ChamadosCampo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ChamadosCampo.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<HistoricoChamado> Historicos => Set<HistoricoChamado>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Chamado>(e =>
        {
            e.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            e.Property(x => x.Endereco).HasMaxLength(200).IsRequired();
            e.Property(x => x.Solicitante).HasMaxLength(80).IsRequired();
            e.Property(x => x.MotivoRecusa).HasMaxLength(300);
            e.HasIndex(x => x.NumeroOs).IsUnique();
            e.HasMany(x => x.Historico).WithOne().HasForeignKey(h => h.ChamadoId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public static class DadosIniciais
{
    public static void Popular(AppDbContext db)
    {
        if (db.Chamados.Any()) return;
        db.Chamados.AddRange(
            new Chamado { Tipo = TipoProblema.PlacaCaida, Descricao = "Placa de pare caída no cruzamento.", Endereco = "Rua Exemplo, 100", Solicitante = "Usuário Exemplo", Status = StatusChamado.Aceito, NumeroOs = 1 },
            new Chamado { Tipo = TipoProblema.FaixaApagada, Descricao = "Faixa de pedestre apagada.", Endereco = "Av. Modelo, 250", Solicitante = "Usuário Exemplo" });
        db.SaveChanges();
    }
}
