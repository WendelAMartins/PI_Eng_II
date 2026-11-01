using ChamadosCampo.Web.Models;
using ChamadosCampo.Web.Services;
using ChamadosCampo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChamadosCampo.Web.Controllers;

public class ChamadosController : Controller
{
    private readonly ChamadoService _service;
    private readonly IWebHostEnvironment _env;

    public ChamadosController(ChamadoService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    public async Task<IActionResult> Index(string? busca)
    {
        ViewBag.Busca = busca;
        var lista = string.IsNullOrWhiteSpace(busca)
            ? new List<Chamado>()
            : await _service.BuscarAsync(busca);
        return View(lista);
    }

    public static bool TentarLerCoordenada(string? texto, double limite, out double? valor)
    {
        valor = null;
        if (string.IsNullOrWhiteSpace(texto)) return true;
        var ok = double.TryParse(texto.Trim().Replace(',', '.'),
            System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var n);
        if (!ok || Math.Abs(n) > limite) return false;
        valor = n;
        return true;
    }

    public IActionResult Create() => View(new NovoChamadoViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovoChamadoViewModel vm)
    {
        if (vm.Foto != null)
        {
            var erro = ChamadoService.ValidarFoto(vm.Foto.FileName, vm.Foto.Length);
            if (erro != null) ModelState.AddModelError(nameof(vm.Foto), erro);
        }
        if (!TentarLerCoordenada(vm.Latitude, 90, out var lat))
            ModelState.AddModelError(nameof(vm.Latitude), "Latitude inválida (use valor entre -90 e 90).");
        if (!TentarLerCoordenada(vm.Longitude, 180, out var lon))
            ModelState.AddModelError(nameof(vm.Longitude), "Longitude inválida (use valor entre -180 e 180).");
        if (!ModelState.IsValid) return View(vm);

        var chamado = new Chamado
        {
            Solicitante = vm.Solicitante.Trim(),
            Tipo = vm.Tipo!.Value,
            Endereco = vm.Endereco.Trim(),
            Descricao = vm.Descricao.Trim(),
            Latitude = lat,
            Longitude = lon
        };

        var resultado = await _service.AbrirAsync(chamado);
        if (resultado.Duplicado)
        {
            TempData["Aviso"] = resultado.Chamado.NumeroOs is int os
                ? $"Já existe a OS {os} para este problema neste local. Acompanhe por ela."
                : "Já existe um chamado em análise para este problema neste local.";
            return RedirectToAction(nameof(Details), new { id = resultado.Chamado.Id });
        }

        if (vm.Foto != null)
        {
            var pasta = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(pasta);
            var nome = $"{resultado.Chamado.Id}-{Guid.NewGuid():N}{Path.GetExtension(vm.Foto.FileName).ToLowerInvariant()}";
            await using (var fs = System.IO.File.Create(Path.Combine(pasta, nome)))
                await vm.Foto.CopyToAsync(fs);
            await _service.SalvarFotoAsync(resultado.Chamado.Id, $"/uploads/{nome}");
        }

        TempData["Sucesso"] = "Chamado enviado. A chefia vai analisar.";
        return RedirectToAction(nameof(Details), new { id = resultado.Chamado.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await _service.ObterAsync(id);
        return c == null ? NotFound() : View(c);
    }
}
