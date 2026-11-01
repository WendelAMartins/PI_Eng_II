using System.ComponentModel.DataAnnotations;
using ChamadosCampo.Web.Models;

namespace ChamadosCampo.Web.ViewModels;

public class NovoChamadoViewModel
{
    [Required(ErrorMessage = "Informe seu nome."), StringLength(80)]
    public string Solicitante { get; set; } = "";

    [Required(ErrorMessage = "Escolha o tipo de problema.")]
    public TipoProblema? Tipo { get; set; }

    [Required(ErrorMessage = "Informe o endereço."), StringLength(200)]
    public string Endereco { get; set; } = "";

    [Required(ErrorMessage = "Descreva o problema."), StringLength(500)]
    public string Descricao { get; set; } = "";

    // Texto, para aceitar ponto ou vírgula como separador decimal.
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }

    public IFormFile? Foto { get; set; }
}
