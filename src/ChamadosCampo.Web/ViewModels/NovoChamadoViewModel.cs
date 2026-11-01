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

    [Range(-90, 90)] public double? Latitude { get; set; }
    [Range(-180, 180)] public double? Longitude { get; set; }

    public IFormFile? Foto { get; set; }
}
