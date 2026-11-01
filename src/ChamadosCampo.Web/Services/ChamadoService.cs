using ChamadosCampo.Web.Data;
using ChamadosCampo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ChamadosCampo.Web.Services;

public record ResultadoAbertura(Chamado Chamado, bool Duplicado);

public class ChamadoService
{
    public const long TamanhoMaxFoto = 5 * 1024 * 1024;
    private static readonly string[] ExtensoesFoto = [".jpg", ".jpeg", ".png"];

    private readonly AppDbContext _db;

    public ChamadoService(AppDbContext db) => _db = db;

    public static string? ValidarFoto(string nomeArquivo, long tamanho)
    {
        if (tamanho <= 0) return "Arquivo de foto vazio.";
        if (tamanho > TamanhoMaxFoto) return "Foto maior que 5 MB.";
        var ext = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        return ExtensoesFoto.Contains(ext) ? null : "Use foto JPG ou PNG.";
    }

    private static readonly Dictionary<string, string> Abreviaturas = new()
    {
        ["av"] = "avenida", ["r"] = "rua", ["tv"] = "travessa", ["trav"] = "travessa",
        ["pc"] = "praca", ["pca"] = "praca", ["al"] = "alameda", ["rod"] = "rodovia", ["est"] = "estrada"
    };

    public static string NormalizarEndereco(string endereco)
    {
        var decomposto = endereco.Normalize(System.Text.NormalizationForm.FormD);
        var semAcento = new string(decomposto
            .Where(ch => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ')
            .ToArray());
        var palavras = semAcento.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => Abreviaturas.TryGetValue(p, out var inteira) ? inteira : p);
        return string.Join(' ', palavras);
    }

    public async Task<ResultadoAbertura> AbrirAsync(Chamado novo)
    {
        if (string.IsNullOrWhiteSpace(novo.Solicitante)) throw new ArgumentException("Solicitante obrigatório.");
        if (string.IsNullOrWhiteSpace(novo.Endereco)) throw new ArgumentException("Endereço obrigatório.");
        if (string.IsNullOrWhiteSpace(novo.Descricao)) throw new ArgumentException("Descrição obrigatória.");

        var chave = NormalizarEndereco(novo.Endereco);
        var abertos = await _db.Chamados
            .Where(c => c.Tipo == novo.Tipo && c.Status != StatusChamado.Encerrado && c.Status != StatusChamado.Recusado)
            .ToListAsync();
        var existente = abertos.FirstOrDefault(c => NormalizarEndereco(c.Endereco) == chave);
        if (existente != null) return new ResultadoAbertura(existente, true);

        novo.Status = StatusChamado.Solicitado;
        novo.CriadoEm = DateTime.UtcNow;
        novo.Historico.Add(new HistoricoChamado { Status = StatusChamado.Solicitado, Observacao = "Chamado aberto." });
        _db.Chamados.Add(novo);
        await _db.SaveChangesAsync();
        return new ResultadoAbertura(novo, false);
    }

    public async Task SalvarFotoAsync(int id, string caminho)
    {
        var c = await ObterOuFalharAsync(id);
        c.FotoPath = caminho;
        await _db.SaveChangesAsync();
    }

    public Task<Chamado?> ObterAsync(int id) =>
        _db.Chamados.Include(c => c.Historico).FirstOrDefaultAsync(c => c.Id == id);

    public Task<List<Chamado>> ListarPorSolicitanteAsync(string solicitante)
    {
        var nome = solicitante.Trim().ToLower();
        return _db.Chamados.Where(c => c.Solicitante.ToLower() == nome).OrderByDescending(c => c.CriadoEm).ToListAsync();
    }

    // Aceita nome do solicitante (sem diferenciar maiúsculas) ou número da OS ("4", "OS 4", "OS-0004").
    public Task<List<Chamado>> BuscarAsync(string termo)
    {
        var limpo = termo.Trim();
        var digitos = limpo.StartsWith("os", StringComparison.OrdinalIgnoreCase) ? limpo[2..].Trim(' ', '-', '#') : limpo.TrimStart('#');
        if (int.TryParse(digitos, out var numero))
            return _db.Chamados.Where(c => c.NumeroOs == numero).OrderByDescending(c => c.CriadoEm).ToListAsync();
        return ListarPorSolicitanteAsync(limpo);
    }

    public Task<List<Chamado>> ListarPorStatusAsync(StatusChamado status) =>
        _db.Chamados.Where(c => c.Status == status).OrderBy(c => c.CriadoEm).ToListAsync();

    public async Task AceitarAsync(int id)
    {
        var c = await ObterOuFalharAsync(id);
        Exigir(c, StatusChamado.Solicitado);
        var maior = await _db.Chamados.MaxAsync(x => x.NumeroOs) ?? 0;
        c.NumeroOs = maior + 1;
        Mudar(c, StatusChamado.Aceito, $"Aceito como OS {c.NumeroOs}.");
        await _db.SaveChangesAsync();
    }

    public async Task RecusarAsync(int id, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ArgumentException("Informe o motivo da recusa.");
        var c = await ObterOuFalharAsync(id);
        Exigir(c, StatusChamado.Solicitado);
        c.MotivoRecusa = motivo.Trim();
        Mudar(c, StatusChamado.Recusado, $"Recusado: {c.MotivoRecusa}");
        await _db.SaveChangesAsync();
    }

    public async Task ProgramarAsync(int id, DateOnly data)
    {
        if (data < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("A data programada não pode ser anterior a hoje.");
        var c = await ObterOuFalharAsync(id);
        Exigir(c, StatusChamado.Aceito, StatusChamado.Programado);
        c.DataProgramada = data;
        Mudar(c, StatusChamado.Programado, $"Programado para {data:dd/MM/yyyy}.");
        await _db.SaveChangesAsync();
    }

    public async Task IniciarAsync(int id)
    {
        var c = await ObterOuFalharAsync(id);
        Exigir(c, StatusChamado.Aceito, StatusChamado.Programado);
        Mudar(c, StatusChamado.EmAndamento, "Serviço iniciado.");
        await _db.SaveChangesAsync();
    }

    public async Task EncerrarAsync(int id)
    {
        var c = await ObterOuFalharAsync(id);
        Exigir(c, StatusChamado.EmAndamento);
        Mudar(c, StatusChamado.Encerrado, "Serviço encerrado.");
        await _db.SaveChangesAsync();
    }

    private async Task<Chamado> ObterOuFalharAsync(int id) =>
        await ObterAsync(id) ?? throw new KeyNotFoundException($"Chamado {id} não encontrado.");

    private static void Exigir(Chamado c, params StatusChamado[] permitidos)
    {
        if (!permitidos.Contains(c.Status))
            throw new InvalidOperationException($"Transição inválida a partir de {c.Status}.");
    }

    private static void Mudar(Chamado c, StatusChamado novo, string obs)
    {
        c.Status = novo;
        c.Historico.Add(new HistoricoChamado { ChamadoId = c.Id, Status = novo, Observacao = obs });
    }
}
