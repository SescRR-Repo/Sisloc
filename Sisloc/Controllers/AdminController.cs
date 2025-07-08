using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.Controllers
{
    public class AdminController : Controller
    {
        private readonly SislocDbContext _context;

        public AdminController(SislocDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard administrativo
        public async Task<IActionResult> Index(StatusAgendamento? status = null, DateTime? dataInicio = null, DateTime? dataFim = null, CategoriaVeiculo? categoria = null)
        {
            var query = _context.Agendamentos
                .Include(a => a.VeiculoAlocado)
                .Include(a => a.MotoristaAlocado)
                .AsQueryable();

            // Filtros
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(a => a.DataPartida.Date >= dataInicio.Value.Date);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(a => a.DataPartida.Date <= dataFim.Value.Date);
            }

            if (categoria.HasValue)
            {
                query = query.Where(a => a.CategoriaVeiculo == categoria.Value);
            }

            var agendamentos = await query
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();

            // Preparar ViewBags para os filtros
            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(StatusAgendamento))
                    .Cast<StatusAgendamento>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text", status);

            ViewBag.CategoriasList = new SelectList(
                Enum.GetValues(typeof(CategoriaVeiculo))
                    .Cast<CategoriaVeiculo>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text", categoria);

            // Manter os valores dos filtros
            ViewBag.StatusSelecionado = status;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.CategoriaSelecionada = categoria;

            return View(agendamentos);
        }

        // GET: Detalhes do agendamento para aprovação/reprovação
        public async Task<IActionResult> Detalhes(int id)
        {
            var agendamento = await _context.Agendamentos
                .Include(a => a.VeiculoAlocado)
                .Include(a => a.MotoristaAlocado)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (agendamento == null)
            {
                return NotFound();
            }

            // Buscar veículos disponíveis da categoria
            var veiculosDisponiveis = await ObterVeiculosDisponiveis(agendamento);
            ViewBag.VeiculosDisponiveis = new SelectList(veiculosDisponiveis, "Id", "DisplayText");

            // Buscar motoristas disponíveis
            var motoristasDisponiveis = await ObterMotoristasDisponiveis(agendamento);
            ViewBag.MotoristasDisponiveis = new SelectList(motoristasDisponiveis, "Id", "NomeCompleto");

            return View(agendamento);
        }

        // POST: Aprovar agendamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id, int? veiculoId, int? motoristaId, string? observacoesAdmin)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento == null)
            {
                return NotFound();
            }

            if (agendamento.Status != StatusAgendamento.Pendente)
            {
                TempData["ErrorMessage"] = "Só é possível aprovar agendamentos pendentes.";
                return RedirectToAction("Detalhes", new { id });
            }

            // Validar se veículo foi selecionado
            if (!veiculoId.HasValue)
            {
                TempData["ErrorMessage"] = "É necessário selecionar um veículo.";
                return RedirectToAction("Detalhes", new { id });
            }

            // Validar se motorista foi selecionado quando necessário
            if (agendamento.PrecisaMotorista && !motoristaId.HasValue)
            {
                TempData["ErrorMessage"] = "É necessário selecionar um motorista para este agendamento.";
                return RedirectToAction("Detalhes", new { id });
            }

            // Atualizar o agendamento
            agendamento.Status = StatusAgendamento.Aprovado;
            agendamento.VeiculoAlocadoId = veiculoId.Value;
            agendamento.MotoristaAlocadoId = motoristaId;
            agendamento.ObservacoesAdmin = observacoesAdmin;

            // Atualizar status do veículo
            var veiculo = await _context.Veiculos.FindAsync(veiculoId.Value);
            if (veiculo != null)
            {
                veiculo.Status = StatusVeiculo.Reservado;
            }

            // Atualizar status do motorista se alocado
            if (motoristaId.HasValue)
            {
                var motorista = await _context.Motoristas.FindAsync(motoristaId.Value);
                if (motorista != null)
                {
                    motorista.Status = StatusMotorista.Ocupado;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Agendamento aprovado com sucesso!";
            return RedirectToAction("Index");
        }

        // POST: Reprovar agendamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reprovar(int id, string observacoesAdmin)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento == null)
            {
                return NotFound();
            }

            if (agendamento.Status != StatusAgendamento.Pendente)
            {
                TempData["ErrorMessage"] = "Só é possível reprovar agendamentos pendentes.";
                return RedirectToAction("Detalhes", new { id });
            }

            if (string.IsNullOrWhiteSpace(observacoesAdmin))
            {
                TempData["ErrorMessage"] = "É obrigatório informar o motivo da reprovação.";
                return RedirectToAction("Detalhes", new { id });
            }

            // Atualizar o agendamento
            agendamento.Status = StatusAgendamento.Rejeitado;
            agendamento.ObservacoesAdmin = observacoesAdmin;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Agendamento reprovado.";
            return RedirectToAction("Index");
        }

        // Métodos auxiliares
        private async Task<List<object>> ObterVeiculosDisponiveis(Agendamento agendamento)
        {
            var veiculos = await _context.Veiculos
                .Where(v => v.Categoria == agendamento.CategoriaVeiculo)
                .ToListAsync();

            var veiculosDisponiveis = new List<object>();

            foreach (var veiculo in veiculos)
            {
                // Verificar se tem conflito de horário
                var temConflito = await _context.Agendamentos
                    .Where(a => a.VeiculoAlocadoId == veiculo.Id &&
                               a.Id != agendamento.Id &&
                               a.Status != StatusAgendamento.Rejeitado &&
                               a.Status != StatusAgendamento.Concluido)
                    .AnyAsync(a =>
                        (agendamento.DataPartida >= a.DataPartida && agendamento.DataPartida < a.DataChegada) ||
                        (agendamento.DataChegada > a.DataPartida && agendamento.DataChegada <= a.DataChegada) ||
                        (agendamento.DataPartida <= a.DataPartida && agendamento.DataChegada >= a.DataChegada)
                    );

                if (!temConflito)
                {
                    veiculosDisponiveis.Add(new
                    {
                        Id = veiculo.Id,
                        DisplayText = $"{veiculo.Modelo} - {veiculo.Placa} (Cap: {veiculo.CapacidadePassageiros})"
                    });
                }
            }

            return veiculosDisponiveis;
        }

        private async Task<List<Motorista>> ObterMotoristasDisponiveis(Agendamento agendamento)
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.Status == StatusMotorista.Disponivel)
                .ToListAsync();

            var motoristasDisponiveis = new List<Motorista>();

            foreach (var motorista in motoristas)
            {
                // Verificar se tem conflito de horário
                var temConflito = await _context.Agendamentos
                    .Where(a => a.MotoristaAlocadoId == motorista.Id &&
                               a.Id != agendamento.Id &&
                               a.Status != StatusAgendamento.Rejeitado &&
                               a.Status != StatusAgendamento.Concluido)
                    .AnyAsync(a =>
                        (agendamento.DataPartida >= a.DataPartida && agendamento.DataPartida < a.DataChegada) ||
                        (agendamento.DataChegada > a.DataPartida && agendamento.DataChegada <= a.DataChegada) ||
                        (agendamento.DataPartida <= a.DataPartida && agendamento.DataChegada >= a.DataChegada)
                    );

                if (!temConflito)
                {
                    motoristasDisponiveis.Add(motorista);
                }
            }

            return motoristasDisponiveis;
        }
    }
}