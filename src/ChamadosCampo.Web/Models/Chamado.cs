namespace ChamadosCampo.Web.Models;

public enum TipoProblema
{
    PlacaCaida,
    PlacaVirada,
    FaixaApagada,
    SemaforoComDefeito,
    Outro
}

public enum StatusChamado
{
    Solicitado,
    Aceito,
    Programado,
    EmAndamento,
    Encerrado,
    Recusado
}

public class Chamado
{
    public int Id { get; set; }
    public TipoProblema Tipo { get; set; }
    public string Descricao { get; set; } = "";
    public string Endereco { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FotoPath { get; set; }
    public string Solicitante { get; set; } = "";
    public StatusChamado Status { get; set; } = StatusChamado.Solicitado;
    public int? NumeroOs { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateOnly? DataProgramada { get; set; }
    public string? MotivoRecusa { get; set; }
    public List<HistoricoChamado> Historico { get; set; } = new();
}

public class HistoricoChamado
{
    public int Id { get; set; }
    public int ChamadoId { get; set; }
    public StatusChamado Status { get; set; }
    public string Observacao { get; set; } = "";
    public DateTime Em { get; set; } = DateTime.UtcNow;
}
