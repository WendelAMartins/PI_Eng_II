using ChamadosCampo.Web.Models;

namespace ChamadosCampo.Web.Helpers;

public static class UiExtensions
{
    public static string Rotulo(this StatusChamado s) => s switch
    {
        StatusChamado.Solicitado => "Em análise",
        StatusChamado.Aceito => "Aceito",
        StatusChamado.Programado => "Programado",
        StatusChamado.EmAndamento => "Em andamento",
        StatusChamado.Encerrado => "Encerrado",
        StatusChamado.Recusado => "Recusado",
        _ => s.ToString()
    };

    public static string Classe(this StatusChamado s) => s switch
    {
        StatusChamado.Solicitado => "st-analise",
        StatusChamado.Aceito => "st-aceito",
        StatusChamado.Programado => "st-programado",
        StatusChamado.EmAndamento => "st-andamento",
        StatusChamado.Encerrado => "st-encerrado",
        StatusChamado.Recusado => "st-recusado",
        _ => ""
    };

    public static string Rotulo(this TipoProblema t) => t switch
    {
        TipoProblema.PlacaCaida => "Placa caída",
        TipoProblema.PlacaVirada => "Placa virada",
        TipoProblema.FaixaApagada => "Faixa apagada",
        TipoProblema.SemaforoComDefeito => "Semáforo com defeito",
        _ => "Outro"
    };

    public static string Titulo(this Chamado c) =>
        c.NumeroOs is int os ? $"OS {os:D4}" : $"Chamado #{c.Id}";
}
