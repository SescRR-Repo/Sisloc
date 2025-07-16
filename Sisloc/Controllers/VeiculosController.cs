// Controllers/VeiculosController.cs
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
    public class VeiculosController : Controller
    {
        private readonly IVeiculoService _veiculoService;
        private readonly SislocDbContext _context;

        public VeiculosController(IVeiculoService veiculoService, SislocDbContext context)
        {
            _veiculoService = veiculoService;
            _context = context;
        }

        // GET: Veiculos
        public async Task<IActionResult> Index(
            string? filtroTexto = null,
            CategoriaVeiculo? categoria = null,
            StatusVeiculo? status = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
            {
                var veiculos = await _veiculoService.ObterVeiculosPaginadosAsync(
                    pageIndex, pageSize, filtroTexto, categoria, status);

                var estatisticas = await _veiculoService.ObterEstatisticasAsync();

                var viewModel = new VeiculoIndexViewModel
                {
                    Veiculos = veiculos,
                    FiltroTexto = filtroTexto,
                    CategoriaFiltro = categoria,
                    StatusFiltro = status,
                    PaginaAtual = pageIndex,
                    ItensPorPagina = pageSize,
                    Estatisticas = estatisticas
                };

                // Preparar ViewBags para dropdowns
                PrepararViewBags(categoria, status);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar veículos: " + ex.Message;
                return View(new VeiculoIndexViewModel());
            }
        }

        // GET: Veiculos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var veiculo = await _veiculoService.ObterPorIdAsync(id);
                if (veiculo == null)
                    return NotFound();

                // Buscar agendamentos relacionados
                var agendamentos = await _context.Agendamentos
                    .Where(a => a.VeiculoAlocadoId == id)
                    .OrderByDescending(a => a.DataCriacao)
                    .ToListAsync();

                var agendamentosAtivos = agendamentos.Where(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento ||
                    a.Status == StatusAgendamento.Pendente).ToList();

                var viewModel = new VeiculoDetailsViewModel
                {
                    Veiculo = veiculo,
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
                        .FirstOrDefault()?.DataChegada
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar detalhes: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Veiculos/Create
        public IActionResult Create()
        {
            PrepararViewBagsCreate();
            return View(new VeiculoCreateViewModel());
        }

        // POST: Veiculos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VeiculoCreateViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var veiculo = new Veiculo
                    {
                        Placa = viewModel.Placa,
                        Modelo = viewModel.Modelo,
                        Categoria = viewModel.Categoria,
                        CapacidadePassageiros = viewModel.CapacidadePassageiros,
                        Status = viewModel.Status,
                        Observacoes = viewModel.Observacoes
                    };

                    await _veiculoService.CriarAsync(veiculo);

                    TempData["SuccessMessage"] = "Veículo cadastrado com sucesso!";
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

        // GET: Veiculos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var veiculo = await _veiculoService.ObterPorIdAsync(id);
                if (veiculo == null)
                    return NotFound();

                var agendamentosAtivos = veiculo.Agendamentos?.Count(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento ||
                    a.Status == StatusAgendamento.Pendente) ?? 0;

                var proximoAgendamento = veiculo.Agendamentos?
                    .Where(a => a.Status == StatusAgendamento.Aprovado || a.Status == StatusAgendamento.EmAndamento)
                    .OrderBy(a => a.DataPartida)
                    .FirstOrDefault()?.DataPartida;

                var viewModel = new VeiculoEditViewModel
                {
                    Id = veiculo.Id,
                    Placa = veiculo.Placa,
                    Modelo = veiculo.Modelo,
                    Categoria = veiculo.Categoria,
                    CapacidadePassageiros = veiculo.CapacidadePassageiros,
                    Status = veiculo.Status,
                    Observacoes = veiculo.Observacoes,
                    TemAgendamentosAtivos = agendamentosAtivos > 0,
                    QuantidadeAgendamentos = agendamentosAtivos,
                    ProximoAgendamento = proximoAgendamento
                };

                PrepararViewBagsEdit();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar veículo para edição: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Veiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VeiculoEditViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var veiculo = new Veiculo
                    {
                        Id = viewModel.Id,
                        Placa = viewModel.Placa,
                        Modelo = viewModel.Modelo,
                        Categoria = viewModel.Categoria,
                        CapacidadePassageiros = viewModel.CapacidadePassageiros,
                        Status = viewModel.Status,
                        Observacoes = viewModel.Observacoes
                    };

                    await _veiculoService.AtualizarAsync(veiculo);

                    TempData["SuccessMessage"] = "Veículo atualizado com sucesso!";
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

        // GET: Veiculos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var veiculo = await _veiculoService.ObterPorIdAsync(id);
                if (veiculo == null)
                    return NotFound();

                // Verificar se pode ser removido
                var podeSerRemovido = await _veiculoService.PodeSerRemovidoAsync(id);
                ViewBag.PodeSerRemovido = podeSerRemovido;
                ViewBag.MotivoNaoRemocao = podeSerRemovido ? "" : "Este veículo possui agendamentos ativos e não pode ser removido.";

                return View(veiculo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar veículo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Veiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _veiculoService.RemoverAsync(id);

                if (sucesso)
                {
                    TempData["SuccessMessage"] = "Veículo removido com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Veículo não encontrado.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao remover veículo: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Verificar se placa existe
        [HttpPost]
        public async Task<IActionResult> VerificarPlaca(string placa, int? id = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return Json(new { existe = false });

                var existe = await _veiculoService.PlacaExisteAsync(placa, id);
                return Json(new { existe });
            }
            catch
            {
                return Json(new { existe = false });
            }
        }

        // AJAX: Obter veículos disponíveis por categoria
        [HttpGet]
        public async Task<IActionResult> ObterDisponiveis(CategoriaVeiculo categoria, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var veiculos = await _veiculoService.ObterDisponivelPorCategoriaAsync(categoria, dataInicio, dataFim);

                var resultado = veiculos.Select(v => new {
                    id = v.Id,
                    texto = $"{v.Modelo} - {v.Placa} (Cap: {v.CapacidadePassageiros})"
                });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { erro = ex.Message });
            }
        }

        // AJAX: Obter estatísticas para dashboard
        [HttpGet]
        public async Task<IActionResult> ObterEstatisticas()
        {
            try
            {
                var estatisticas = await _veiculoService.ObterEstatisticasAsync();
                return Json(estatisticas);
            }
            catch (Exception ex)
            {
                return Json(new { erro = ex.Message });
            }
        }

        #region Métodos Privados

        private void PrepararViewBags(CategoriaVeiculo? categoriaSelecionada = null, StatusVeiculo? statusSelecionado = null)
        {
            ViewBag.Categorias = new SelectList(
                Enum.GetValues<CategoriaVeiculo>()
                    .Select(c => new { Value = c, Text = c.ToString() }),
                "Value", "Text", categoriaSelecionada);

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusVeiculo>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text", statusSelecionado);
        }

        private void PrepararViewBagsCreate()
        {
            ViewBag.Categorias = new SelectList(
                Enum.GetValues<CategoriaVeiculo>()
                    .Select(c => new { Value = c, Text = c.ToString() }),
                "Value", "Text");

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusVeiculo>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text", StatusVeiculo.Disponivel);
        }

        private void PrepararViewBagsEdit()
        {
            ViewBag.Categorias = new SelectList(
                Enum.GetValues<CategoriaVeiculo>()
                    .Select(c => new { Value = c, Text = c.ToString() }),
                "Value", "Text");

            ViewBag.StatusList = new SelectList(
                Enum.GetValues<StatusVeiculo>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text");
        }

        #endregion
    }
}