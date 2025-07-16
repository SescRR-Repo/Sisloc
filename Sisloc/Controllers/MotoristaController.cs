// Controllers/MotoristaController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Services;
using Sisloc.ViewModels;

namespace Sisloc.Controllers
{
    public class MotoristasController : Controller
    {
        private readonly IMotoristaService _motoristaService;
        private readonly SislocDbContext _context;

        public MotoristasController(IMotoristaService motoristaService, SislocDbContext context)
        {
            _motoristaService = motoristaService;
            _context = context;
        }

        // GET: Motoristas
        public async Task<IActionResult> Index(
            string? filtroTexto = null,
            string? categoriaCnh = null,
            StatusMotorista? status = null,
            bool? documentacaoVencida = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
            {
                var motoristas = await _motoristaService.ObterMotoristasPaginadosAsync(
                    pageIndex, pageSize, filtroTexto, categoriaCnh, status, documentacaoVencida);

                var estatisticas = await _motoristaService.ObterEstatisticasAsync();
                var alertas = await _motoristaService.ObterAlertasVencimentoAsync();

                var viewModel = new MotoristaIndexViewModel
                {
                    Motoristas = motoristas,
                    FiltroTexto = filtroTexto,
                    CategoriaCnhFiltro = categoriaCnh,
                    StatusFiltro = status,
                    DocumentacaoVencidaFiltro = documentacaoVencida,
                    PaginaAtual = pageIndex,
                    ItensPorPagina = pageSize,
                    Estatisticas = estatisticas,
                    AlertasVencimento = alertas
                };

                // Preparar ViewBags para dropdowns
                PrepararViewBags(categoriaCnh, status);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar motoristas: " + ex.Message;
                return View(new MotoristaIndexViewModel());
            }
        }

        // GET: Motoristas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var motorista = await _motoristaService.ObterPorIdAsync(id);
                if (motorista == null)
                    return NotFound();

                // Buscar agendamentos relacionados
                var agendamentos = await _context.Agendamentos
                    .Where(a => a.MotoristaAlocadoId == id)
                    .Include(a => a.VeiculoAlocado)
                    .OrderByDescending(a => a.DataCriacao)
                    .ToListAsync();

                var agendamentosAtivos = agendamentos.Where(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento ||
                    a.Status == StatusAgendamento.Pendente).ToList();

                // Verificar alertas de vencimento
                var cnhVenceEm = (motorista.VencimentoCnh - DateTime.Now).Days;
                var exameToxVenceEm = motorista.DataExameToxicologico.AddYears(2) > DateTime.Now ?
                    (motorista.DataExameToxicologico.AddYears(2) - DateTime.Now).Days : -1;

                var viewModel = new MotoristaDetailsViewModel
                {
                    Motorista = motorista,
                    AgendamentosAtivos = agendamentosAtivos,
                    HistoricoAgendamentos = agendamentos.Take(10).ToList(),
                    TotalAgendamentos = agendamentos.Count,
                    AgendamentosEsteAno = agendamentos.Count(a => a.DataCriacao.Year == DateTime.Now.Year),
                    AgendamentosEsteMes = agendamentos.Count(a =>
                        a.DataCriacao.Year == DateTime.Now.Year &&
                        a.DataCriacao.Month == DateTime.Now.Month),
                    UltimaUtilizacao = agendamentos
                        .Where(a => a.Status == StatusAgendamento.Concluido)
                        .OrderByDescending(a => a.DataChegada)
                        .FirstOrDefault()?.DataChegada,
                    CnhVenceEm = cnhVenceEm,
                    ExameToxicologicoVenceEm = exameToxVenceEm,
                    TemAlertaVencimento = cnhVenceEm <= 30 || exameToxVenceEm <= 60
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar detalhes: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Motoristas/Create
        public IActionResult Create()
        {
            PrepararViewBagsCreate();
            return View(new MotoristaCreateViewModel());
        }

        // POST: Motoristas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotoristaCreateViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var motorista = new Motorista
                    {
                        NomeCompleto = viewModel.NomeCompleto,
                        NumeroCnh = viewModel.NumeroCnh,
                        VencimentoCnh = viewModel.VencimentoCnh,
                        CategoriaCnh = viewModel.CategoriaCnh,
                        Telefone = viewModel.Telefone,
                        DataExameToxicologico = viewModel.DataExameToxicologico,
                        Status = viewModel.Status,
                        Observacoes = viewModel.Observacoes
                    };

                    await _motoristaService.CriarAsync(motorista);

                    TempData["SuccessMessage"] = "Motorista cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro interno: " + ex.Message);
            }

            PrepararViewBagsCreate();
            return View(viewModel);
        }

        // GET: Motoristas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var motorista = await _motoristaService.ObterPorIdAsync(id);
                if (motorista == null)
                    return NotFound();

                var agendamentosAtivos = motorista.Agendamentos?.Count(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento ||
                    a.Status == StatusAgendamento.Pendente) ?? 0;

                var proximoAgendamento = motorista.Agendamentos?
                    .Where(a => a.Status == StatusAgendamento.Aprovado || a.Status == StatusAgendamento.EmAndamento)
                    .OrderBy(a => a.DataPartida)
                    .FirstOrDefault()?.DataPartida;

                var viewModel = new MotoristaEditViewModel
                {
                    Id = motorista.Id,
                    NomeCompleto = motorista.NomeCompleto,
                    NumeroCnh = motorista.NumeroCnh,
                    VencimentoCnh = motorista.VencimentoCnh,
                    CategoriaCnh = motorista.CategoriaCnh,
                    Telefone = motorista.Telefone,
                    DataExameToxicologico = motorista.DataExameToxicologico,
                    Status = motorista.Status,
                    Observacoes = motorista.Observacoes,
                    TemAgendamentosAtivos = agendamentosAtivos > 0,
                    QuantidadeAgendamentos = agendamentosAtivos,
                    ProximoAgendamento = proximoAgendamento
                };

                PrepararViewBagsEdit();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar motorista para edição: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Motoristas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MotoristaEditViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var motorista = new Motorista
                    {
                        Id = viewModel.Id,
                        NomeCompleto = viewModel.NomeCompleto,
                        NumeroCnh = viewModel.NumeroCnh,
                        VencimentoCnh = viewModel.VencimentoCnh,
                        CategoriaCnh = viewModel.CategoriaCnh,
                        Telefone = viewModel.Telefone,
                        DataExameToxicologico = viewModel.DataExameToxicologico,
                        Status = viewModel.Status,
                        Observacoes = viewModel.Observacoes
                    };

                    await _motoristaService.AtualizarAsync(motorista);

                    TempData["SuccessMessage"] = "Motorista atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = viewModel.Id });
                }
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro interno: " + ex.Message);
            }

            PrepararViewBagsEdit();
            return View(viewModel);
        }

        // GET: Motoristas/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var motorista = await _motoristaService.ObterPorIdAsync(id);
                if (motorista == null)
                    return NotFound();

                // Verificar se pode ser removido
                var podeSerRemovido = await _motoristaService.PodeSerRemovidoAsync(id);
                ViewBag.PodeSerRemovido = podeSerRemovido;
                ViewBag.MotivoNaoRemocao = podeSerRemovido ? "" : "Este motorista possui agendamentos ativos e não pode ser removido.";

                return View(motorista);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar motorista: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Motoristas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _motoristaService.RemoverAsync(id);

                if (sucesso)
                {
                    TempData["SuccessMessage"] = "Motorista removido com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Motorista não encontrado.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao remover motorista: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Verificar se CNH existe
        [HttpPost]
        public async Task<IActionResult> VerificarCnh(string numeroCnh, int? id = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroCnh))
                    return Json(new { existe = false });

                var existe = await _motoristaService.CnhExisteAsync(numeroCnh, id);
                return Json(new { existe });
            }
            catch
            {
                return Json(new { existe = false });
            }
        }

        // AJAX: Obter motoristas disponíveis por categoria CNH
        [HttpGet]
        public async Task<IActionResult> ObterDisponiveis(string categoriaCnh, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var motoristas = await _motoristaService.ObterDisponivelPorCategoriaAsync(categoriaCnh, dataInicio, dataFim);

                var resultado = motoristas.Select(m => new {
                    id = m.Id,
                    texto = $"{m.NomeCompleto} - CNH {m.CategoriaCnh} ({m.Telefone})"
                });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { erro = ex.Message });
            }
        }

        // AJAX: Obter alertas de vencimento
        [HttpGet]
        public async Task<IActionResult> ObterAlertas()
        {
            try
            {
                var alertas = await _motoristaService.ObterAlertasVencimentoAsync();
                return Json(alertas);
            }
            catch (Exception ex)
            {
                return Json(new { erro = ex.Message });
            }
        }

        // GET: Relatório de documentação
        public async Task<IActionResult> RelatorioDocumentacao()
        {
            try
            {
                var motoristas = await _context.Motoristas
                    .OrderBy(m => m.NomeCompleto)
                    .ToListAsync();

                var relatorio = motoristas.Select(m => new
                {
                    Motorista = m,
                    CnhVenceEm = (m.VencimentoCnh - DateTime.Now).Days,
                    ExameToxVenceEm = (m.DataExameToxicologico.AddYears(2) - DateTime.Now).Days,
                    Situacao = GetSituacaoDocumentacao(m)
                }).ToList();

                ViewBag.TotalMotoristas = motoristas.Count;
                ViewBag.ComDocumentacaoOk = relatorio.Count(r => r.Situacao == "OK");
                ViewBag.ComAlerta = relatorio.Count(r => r.Situacao == "Alerta");
                ViewBag.ComVencimento = relatorio.Count(r => r.Situacao == "Vencido");

                return View(relatorio);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao gerar relatório: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #region Métodos Privados

        private void PrepararViewBags(string? categoriaSelecionada = null, StatusMotorista? statusSelecionado = null)
        {
            var categoriasCnh = new List<string> { "A", "B", "C", "D", "E", "AB", "AC", "AD", "AE" };

            ViewBag.CategoriasCnh = new SelectList(
                categoriasCnh.Select(c => new { Value = c, Text = $"Categoria {c}" }),
                "Value", "Text", categoriaSelecionada);

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusMotorista>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text", statusSelecionado);
        }

        private void PrepararViewBagsCreate()
        {
            var categoriasCnh = new List<string> { "A", "B", "C", "D", "E", "AB", "AC", "AD", "AE" };

            ViewBag.CategoriasCnh = new SelectList(
                categoriasCnh.Select(c => new { Value = c, Text = $"Categoria {c}" }),
                "Value", "Text");

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusMotorista>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text", StatusMotorista.Disponivel);
        }

        private void PrepararViewBagsEdit()
        {
            var categoriasCnh = new List<string> { "A", "B", "C", "D", "E", "AB", "AC", "AD", "AE" };

            ViewBag.CategoriasCnh = new SelectList(
                categoriasCnh.Select(c => new { Value = c, Text = $"Categoria {c}" }),
                "Value", "Text");

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusMotorista>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text");
        }

        private static string GetSituacaoDocumentacao(Motorista motorista)
        {
            var cnhVenceEm = (motorista.VencimentoCnh - DateTime.Now).Days;
            var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - DateTime.Now).Days;

            if (cnhVenceEm < 0 || exameToxVenceEm < 0)
                return "Vencido";

            if (cnhVenceEm <= 30 || exameToxVenceEm <= 60)
                return "Alerta";

            return "OK";
        }

        #endregion
    }
}