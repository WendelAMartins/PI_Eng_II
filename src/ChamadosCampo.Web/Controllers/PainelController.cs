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
    public Task<IActionResult> Aceitar(int id) => Executar(id, () => _service.AceitarAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Recusar(int id, string motivo) => Executar(id, () => _service.RecusarAsync(id, motivo));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Programar(int id, DateOnly data) => Executar(id, () => _service.ProgramarAsync(id, data));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Iniciar(int id) => Executar(id, () => _service.IniciarAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Encerrar(int id) => Executar(id, () => _service.EncerrarAsync(id));

    private async Task<IActionResult> Executar(int id, Func<Task> acao)
    {
        try { await acao(); }
        catch (Exception e) when (e is InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            TempData["ErroId"] = id;
            TempData["ErroCartao"] = e.Message;
        }
        return RedirectToAction(nameof(Index), null, $"c{id}");
    }
}
