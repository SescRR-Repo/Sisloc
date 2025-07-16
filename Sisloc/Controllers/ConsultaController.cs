// Controllers/ConsultaController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;

namespace Sisloc.Controllers
{
    public class ConsultaController : Controller
    {
        private readonly SislocDbContext _context;

        public ConsultaController(SislocDbContext context)
        {
            _context = context;
        }

        // GET: Página de consulta
        public IActionResult Index()
        {
            return View();
        }

        // POST: Buscar por protocolo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buscar(string protocolo)
        {
            if (string.IsNullOrWhiteSpace(protocolo))
            {
                ModelState.AddModelError("", "Por favor, informe o número do protocolo.");
                return View("Index");
            }

            var agendamento = await _context.Agendamentos
                .Include(a => a.VeiculoAlocado)
                .Include(a => a.MotoristaAlocado)
                .FirstOrDefaultAsync(a => a.Protocolo == protocolo.Trim());

            if (agendamento == null)
            {
                ModelState.AddModelError("", "Protocolo não encontrado. Verifique o número e tente novamente.");
                return View("Index");
            }

            return View("Resultado", agendamento);
        }

        // GET: Consultar diretamente por protocolo via URL
        [HttpGet]
        public async Task<IActionResult> Protocolo(string protocolo)
        {
            if (string.IsNullOrWhiteSpace(protocolo))
            {
                return RedirectToAction("Index");
            }

            var agendamento = await _context.Agendamentos
                .Include(a => a.VeiculoAlocado)
                .Include(a => a.MotoristaAlocado)
                .FirstOrDefaultAsync(a => a.Protocolo == protocolo);

            if (agendamento == null)
            {
                TempData["ErrorMessage"] = "Protocolo não encontrado.";
                return RedirectToAction("Index");
            }

            return View("Resultado", agendamento);
        }
    }
}