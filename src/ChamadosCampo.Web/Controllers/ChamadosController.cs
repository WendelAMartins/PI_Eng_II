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

    public async Task<IActionResult> Index(string? solicitante)
    {
        ViewBag.Solicitante = solicitante;
        var lista = string.IsNullOrWhiteSpace(solicitante)
            ? new List<Chamado>()
            : await _service.ListarPorSolicitanteAsync(solicitante.Trim());
        return View(lista);
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
        if (!ModelState.IsValid) return View(vm);

        var chamado = new Chamado
        {
            Solicitante = vm.Solicitante.Trim(),
            Tipo = vm.Tipo!.Value,
            Endereco = vm.Endereco.Trim(),
            Descricao = vm.Descricao.Trim(),
            Latitude = vm.Latitude,
            Longitude = vm.Longitude
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
