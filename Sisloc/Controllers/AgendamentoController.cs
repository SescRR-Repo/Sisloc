using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.Controllers
{
    public class AgendamentoController : Controller
    {
        private readonly SislocDbContext _context;

        public AgendamentoController(SislocDbContext context)
        {
            _context = context;
        }

        // GET: Formulário de agendamento
        public IActionResult Index()
        {
            // Prepara dados para os dropdowns
            ViewBag.CategoriasVeiculo = new SelectList(
                Enum.GetValues(typeof(CategoriaVeiculo))
                    .Cast<CategoriaVeiculo>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text");

            return View(new Agendamento());
        }

        // POST: Submissão do formulário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agendamento agendamento)
        {
            // Validações customizadas
            if (!ValidarDatas(agendamento))
            {
                return View("Index", agendamento);
            }

            if (!await ValidarDisponibilidade(agendamento))
            {
                ModelState.AddModelError("", "Não há veículos disponíveis para a categoria e período solicitados.");
                ViewBag.CategoriasVeiculo = new SelectList(
                    Enum.GetValues(typeof(CategoriaVeiculo))
                        .Cast<CategoriaVeiculo>()
                        .Select(e => new { Value = (int)e, Text = e.ToString() }),
                    "Value", "Text");
                return View("Index", agendamento);
            }

            if (ModelState.IsValid)
            {
                // Gera protocolo único
                agendamento.Protocolo = GerarProtocolo();
                agendamento.DataCriacao = DateTime.Now;
                agendamento.Status = StatusAgendamento.Pendente;

                _context.Agendamentos.Add(agendamento);
                await _context.SaveChangesAsync();

                // Redireciona para página de sucesso com o protocolo
                TempData["Protocolo"] = agendamento.Protocolo;
                TempData["SuccessMessage"] = "Agendamento criado com sucesso!";

                return RedirectToAction("Sucesso");
            }

            // Se chegou aqui, há erros de validação
            ViewBag.CategoriasVeiculo = new SelectList(
                Enum.GetValues(typeof(CategoriaVeiculo))
                    .Cast<CategoriaVeiculo>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text");

            return View("Index", agendamento);
        }

        // GET: Página de sucesso
        public IActionResult Sucesso()
        {
            ViewBag.Protocolo = TempData["Protocolo"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        // Validações de datas
        private bool ValidarDatas(Agendamento agendamento)
        {
            var hoje = DateTime.Now.Date;
            var dataPartida = agendamento.DataPartida.Date;
            var dataChegada = agendamento.DataChegada.Date;

            // Data de partida não pode ser anterior a hoje
            if (dataPartida < hoje)
            {
                ModelState.AddModelError("DataPartida", "A data de partida não pode ser anterior ao dia atual.");
                return false;
            }

            // Se é no mesmo dia, horário de chegada deve ser posterior ao de partida
            if (dataPartida == dataChegada)
            {
                if (agendamento.DataChegada <= agendamento.DataPartida)
                {
                    ModelState.AddModelError("DataChegada", "O horário de chegada deve ser posterior ao horário de partida.");
                    return false;
                }
            }
            // Se são dias diferentes, data de chegada deve ser posterior
            else if (dataChegada < dataPartida)
            {
                ModelState.AddModelError("DataChegada", "A data de chegada deve ser posterior à data de partida.");
                return false;
            }

            return true;
        }

        // Validação de disponibilidade
        private async Task<bool> ValidarDisponibilidade(Agendamento novoAgendamento)
        {
            // Busca veículos da categoria solicitada
            var veiculosDisponiveis = await _context.Veiculos
                .Where(v => v.Categoria == novoAgendamento.CategoriaVeiculo &&
                           v.Status == StatusVeiculo.Disponivel)
                .ToListAsync();

            if (!veiculosDisponiveis.Any())
            {
                return false;
            }

            // Verifica conflitos de horário para cada veículo
            foreach (var veiculo in veiculosDisponiveis)
            {
                var temConflito = await _context.Agendamentos
                    .Where(a => a.VeiculoAlocadoId == veiculo.Id &&
                               a.Status != StatusAgendamento.Rejeitado &&
                               a.Status != StatusAgendamento.Concluido)
                    .AnyAsync(a =>
                        (novoAgendamento.DataPartida >= a.DataPartida && novoAgendamento.DataPartida < a.DataChegada) ||
                        (novoAgendamento.DataChegada > a.DataPartida && novoAgendamento.DataChegada <= a.DataChegada) ||
                        (novoAgendamento.DataPartida <= a.DataPartida && novoAgendamento.DataChegada >= a.DataChegada)
                    );

                if (!temConflito)
                {
                    return true; // Pelo menos um veículo está disponível
                }
            }

            return false; // Todos os veículos têm conflito
        }

        // Geração de protocolo único
        private string GerarProtocolo()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
        }
    }
}