using ChamadosCampo.Web.Models;
using ChamadosCampo.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChamadosCampo.Web.Controllers;

public class PainelController : Controller
{
    private readonly ChamadoService _service;

    public PainelController(ChamadoService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        ViewBag.Pendentes = await _service.ListarPorStatusAsync(StatusChamado.Solicitado);
        ViewBag.Aceitos = await _service.ListarPorStatusAsync(StatusChamado.Aceito);
        ViewBag.Programados = await _service.ListarPorStatusAsync(StatusChamado.Programado);
        ViewBag.EmAndamento = await _service.ListarPorStatusAsync(StatusChamado.EmAndamento);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Aceitar(int id) => Executar(() => _service.AceitarAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Recusar(int id, string motivo) => Executar(() => _service.RecusarAsync(id, motivo));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Programar(int id, DateOnly data) => Executar(() => _service.ProgramarAsync(id, data));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Iniciar(int id) => Executar(() => _service.IniciarAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Encerrar(int id) => Executar(() => _service.EncerrarAsync(id));

    private async Task<IActionResult> Executar(Func<Task> acao)
    {
        try { await acao(); }
        catch (Exception e) when (e is InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            TempData["Erro"] = e.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
