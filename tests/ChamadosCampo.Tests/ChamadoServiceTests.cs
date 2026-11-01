using ChamadosCampo.Web.Data;
using ChamadosCampo.Web.Models;
using ChamadosCampo.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChamadosCampo.Tests;

public class ChamadoServiceTests : IDisposable
{
    private readonly SqliteConnection _conn = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly ChamadoService _svc;

    public ChamadoServiceTests()
    {
        _conn.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options);
        _db.Database.EnsureCreated();
        _svc = new ChamadoService(_db);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private static Chamado Novo(string endereco = "Rua A, 1", TipoProblema tipo = TipoProblema.PlacaCaida) => new()
    {
        Solicitante = "Ana", Tipo = tipo, Endereco = endereco, Descricao = "Problema de teste"
    };

    [Fact]
    public async Task Abrir_CriaChamadoSolicitadoComHistorico()
    {
        var r = await _svc.AbrirAsync(Novo());
        Assert.False(r.Duplicado);
        Assert.Equal(StatusChamado.Solicitado, r.Chamado.Status);
        Assert.Null(r.Chamado.NumeroOs);
        Assert.Single(r.Chamado.Historico);
    }

    [Theory]
    [InlineData("", "Rua A", "desc")]
    [InlineData("Ana", "  ", "desc")]
    [InlineData("Ana", "Rua A", "")]
    public async Task Abrir_CamposObrigatorios_Falha(string sol, string end, string desc)
    {
        var c = new Chamado { Solicitante = sol, Endereco = end, Descricao = desc };
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.AbrirAsync(c));
    }

    [Fact]
    public async Task Abrir_MesmoTipoEEnderecoAberto_RetornaExistente()
    {
        var primeiro = await _svc.AbrirAsync(Novo("Rua A,   1"));
        var segundo = await _svc.AbrirAsync(Novo("rua a, 1"));
        Assert.True(segundo.Duplicado);
        Assert.Equal(primeiro.Chamado.Id, segundo.Chamado.Id);
        Assert.Equal(1, await _db.Chamados.CountAsync());
    }

    [Fact]
    public async Task Abrir_TipoDiferenteNoMesmoEndereco_NaoEhDuplicado()
    {
        await _svc.AbrirAsync(Novo());
        var r = await _svc.AbrirAsync(Novo(tipo: TipoProblema.FaixaApagada));
        Assert.False(r.Duplicado);
    }

    [Fact]
    public async Task Abrir_AposEncerrado_PermiteNovoChamado()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        await _svc.IniciarAsync(c.Id);
        await _svc.EncerrarAsync(c.Id);
        var r = await _svc.AbrirAsync(Novo());
        Assert.False(r.Duplicado);
    }

    [Fact]
    public async Task Aceitar_GeraNumeroOsSequencial()
    {
        var a = (await _svc.AbrirAsync(Novo("Rua A, 1"))).Chamado;
        var b = (await _svc.AbrirAsync(Novo("Rua B, 2"))).Chamado;
        await _svc.AceitarAsync(a.Id);
        await _svc.AceitarAsync(b.Id);
        Assert.Equal(1, (await _svc.ObterAsync(a.Id))!.NumeroOs);
        Assert.Equal(2, (await _svc.ObterAsync(b.Id))!.NumeroOs);
    }

    [Fact]
    public async Task Aceitar_DuasVezes_Falha()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.AceitarAsync(c.Id));
    }

    [Fact]
    public async Task Recusar_ExigeMotivoERegistra()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.RecusarAsync(c.Id, " "));
        await _svc.RecusarAsync(c.Id, "Fora da área");
        var lido = (await _svc.ObterAsync(c.Id))!;
        Assert.Equal(StatusChamado.Recusado, lido.Status);
        Assert.Equal("Fora da área", lido.MotivoRecusa);
        Assert.Null(lido.NumeroOs);
    }

    [Fact]
    public async Task FluxoCompleto_PercorreTodosOsStatus()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        await _svc.ProgramarAsync(c.Id, new DateOnly(2026, 12, 1));
        await _svc.IniciarAsync(c.Id);
        await _svc.EncerrarAsync(c.Id);
        var lido = (await _svc.ObterAsync(c.Id))!;
        Assert.Equal(StatusChamado.Encerrado, lido.Status);
        Assert.Equal(new DateOnly(2026, 12, 1), lido.DataProgramada);
        Assert.Equal(5, lido.Historico.Count);
    }

    [Fact]
    public async Task Encerrar_SemIniciar_Falha()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.EncerrarAsync(c.Id));
    }

    [Fact]
    public async Task Acao_EmChamadoInexistente_Falha()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.AceitarAsync(999));
    }

    [Fact]
    public async Task ListarPorSolicitante_FiltraEOrdena()
    {
        await _svc.AbrirAsync(Novo("Rua A, 1"));
        await _svc.AbrirAsync(new Chamado { Solicitante = "Bia", Endereco = "Rua C", Descricao = "x" });
        var lista = await _svc.ListarPorSolicitanteAsync("Ana");
        Assert.Single(lista);
    }

    [Theory]
    [InlineData("foto.jpg", 1000, null)]
    [InlineData("foto.PNG", 1000, null)]
    [InlineData("foto.gif", 1000, "Use foto JPG ou PNG.")]
    [InlineData("foto.jpg", 0, "Arquivo de foto vazio.")]
    [InlineData("foto.jpg", ChamadoService.TamanhoMaxFoto + 1, "Foto maior que 5 MB.")]
    public void ValidarFoto_Regras(string nome, long tam, string? esperado)
    {
        Assert.Equal(esperado, ChamadoService.ValidarFoto(nome, tam));
    }

    [Fact]
    public async Task Abrir_EnderecoComAbreviacao_EhDuplicado()
    {
        await _svc.AbrirAsync(Novo("Av. Paulista, 1000 - Bela Vista"));
        var r = await _svc.AbrirAsync(Novo("Avenida Paulista 1000 Bela Vista"));
        Assert.True(r.Duplicado);
    }

    [Fact]
    public async Task Buscar_PorNomeSemDiferenciarMaiusculas()
    {
        await _svc.AbrirAsync(new Chamado { Solicitante = "Rodrigo Alves", Endereco = "Rua X", Descricao = "x" });
        Assert.Single(await _svc.BuscarAsync("rodrigo alves"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("OS 1")]
    [InlineData("os-0001")]
    [InlineData("#1")]
    public async Task Buscar_PorNumeroDaOs(string termo)
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        Assert.Single(await _svc.BuscarAsync(termo));
    }

    [Fact]
    public async Task Programar_DataPassada_Falha()
    {
        var c = (await _svc.AbrirAsync(Novo())).Chamado;
        await _svc.AceitarAsync(c.Id);
        var ontem = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.ProgramarAsync(c.Id, ontem));
    }

    [Theory]
    [InlineData("-23.5615", true, -23.5615)]
    [InlineData("-23,5615", true, -23.5615)]
    [InlineData("", true, null)]
    [InlineData("abc", false, null)]
    [InlineData("95", false, null)]
    public void Coordenada_AceitaPontoOuVirgula(string texto, bool ok, double? esperado)
    {
        Assert.Equal(ok, Web.Controllers.ChamadosController.TentarLerCoordenada(texto, 90, out var valor));
        Assert.Equal(esperado, valor);
    }
}
