// Controllers/AgendamentoController.cs
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
            PrepararViewBag();
            return View(new Agendamento());
        }

        // POST: Submissão do formulário - VERSÃO CORRIGIDA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agendamento agendamento)
        {
            Console.WriteLine("=== INÍCIO CREATE AGENDAMENTO ===");
            Console.WriteLine($"Solicitante: {agendamento.NomeSolicitante}");
            Console.WriteLine($"Categoria: {agendamento.CategoriaVeiculo}");
            Console.WriteLine($"Data Partida: {agendamento.DataPartida}");
            Console.WriteLine($"Data Chegada: {agendamento.DataChegada}");

            try
            {
                // CORREÇÃO: Gerar protocolo ANTES das validações
                if (string.IsNullOrEmpty(agendamento.Protocolo))
                {
                    agendamento.Protocolo = GerarProtocolo();
                    agendamento.DataCriacao = DateTime.Now;
                    agendamento.Status = StatusAgendamento.Pendente;
                    Console.WriteLine($"Protocolo gerado: {agendamento.Protocolo}");

                    // Limpar erro do protocolo do ModelState
                    ModelState.Remove("Protocolo");
                }

                // Validações customizadas
                Console.WriteLine("1. Validando datas...");
                if (!ValidarDatas(agendamento))
                {
                    Console.WriteLine("FALHOU na validação de datas");
                    PrepararViewBag();
                    return View("Index", agendamento);
                }

                Console.WriteLine("2. Validando disponibilidade...");
                if (!await ValidarDisponibilidade(agendamento))
                {
                    Console.WriteLine("FALHOU na validação de disponibilidade");
                    ModelState.AddModelError("", "Não há veículos disponíveis para a categoria e período solicitados.");
                    PrepararViewBag();
                    return View("Index", agendamento);
                }

                Console.WriteLine("3. Verificando ModelState...");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERROS DO MODELSTATE ===");
                    foreach (var modelError in ModelState)
                    {
                        var key = modelError.Key;
                        var errors = modelError.Value.Errors;
                        if (errors.Count > 0)
                        {
                            Console.WriteLine($"Campo: {key}");
                            foreach (var error in errors)
                            {
                                Console.WriteLine($"  - ERRO: {error.ErrorMessage}");
                            }
                        }
                    }
                    Console.WriteLine("=== FIM ERROS MODELSTATE ===");
                }

                if (ModelState.IsValid)
                {
                    Console.WriteLine("4. ModelState VÁLIDO - Salvando agendamento...");

                    _context.Agendamentos.Add(agendamento);
                    var result = await _context.SaveChangesAsync();
                    Console.WriteLine($"5. SaveChanges executado - Linhas afetadas: {result}");

                    // Redireciona para página de sucesso com o protocolo
                    TempData["Protocolo"] = agendamento.Protocolo;
                    TempData["SuccessMessage"] = "Agendamento criado com sucesso!";

                    Console.WriteLine("6. Redirecionando para Sucesso");
                    return RedirectToAction("Sucesso");
                }
                else
                {
                    Console.WriteLine("4. ModelState INVÁLIDO - Retornando formulário");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                ModelState.AddModelError("", "Ocorreu um erro interno. Tente novamente.");
            }

            PrepararViewBag();
            return View("Index", agendamento);
        }

        // Método helper para preparar ViewBag
        private void PrepararViewBag()
        {
            ViewBag.CategoriasVeiculo = new SelectList(
                Enum.GetValues(typeof(CategoriaVeiculo))
                    .Cast<CategoriaVeiculo>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text");
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
            Console.WriteLine("=== VALIDANDO DATAS ===");
            var hoje = DateTime.Now;
            var dataPartida = agendamento.DataPartida;
            var dataChegada = agendamento.DataChegada;

            Console.WriteLine($"Hoje: {hoje}");
            Console.WriteLine($"Data Partida: {dataPartida}");
            Console.WriteLine($"Data Chegada: {dataChegada}");

            // Data de partida não pode ser anterior a agora (considerando horário)
            if (dataPartida < hoje)
            {
                Console.WriteLine("ERRO: Data de partida anterior ao momento atual");
                ModelState.AddModelError("DataPartida", "A data de partida não pode ser anterior ao momento atual.");
                return false;
            }

            // Data de chegada deve ser posterior à de partida
            if (dataChegada <= dataPartida)
            {
                Console.WriteLine("ERRO: Data de chegada deve ser posterior à partida");
                ModelState.AddModelError("DataChegada", "O horário de chegada deve ser posterior ao horário de partida.");
                return false;
            }

            Console.WriteLine("Validação de datas OK");
            return true;
        }

        // Validação de disponibilidade - VERSÃO MELHORADA
        private async Task<bool> ValidarDisponibilidade(Agendamento novoAgendamento)
        {
            try
            {
                // Busca todos os veículos da categoria solicitada
                var veiculosDaCategoria = await _context.Veiculos
                    .Where(v => v.Categoria == novoAgendamento.CategoriaVeiculo &&
                               v.Status != StatusVeiculo.Manutencao)
                    .ToListAsync();

                Console.WriteLine($"Debug: Encontrados {veiculosDaCategoria.Count} veículos da categoria {novoAgendamento.CategoriaVeiculo}");

                if (!veiculosDaCategoria.Any())
                {
                    Console.WriteLine("Debug: Nenhum veículo disponível da categoria solicitada");
                    return false;
                }

                // Conta quantos veículos estão ocupados no período
                int veiculosOcupados = 0;

                foreach (var veiculo in veiculosDaCategoria)
                {
                    var conflitos = await _context.Agendamentos
                        .Where(a => a.VeiculoAlocadoId == veiculo.Id &&
                                   (a.Status == StatusAgendamento.Aprovado ||
                                    a.Status == StatusAgendamento.EmAndamento))
                        .Where(a =>
                            // Sobreposição de horários
                            novoAgendamento.DataPartida < a.DataChegada &&
                            novoAgendamento.DataChegada > a.DataPartida)
                        .CountAsync();

                    if (conflitos > 0)
                    {
                        veiculosOcupados++;
                        Console.WriteLine($"Debug: Veículo {veiculo.Placa} tem {conflitos} conflito(s)");
                    }
                }

                int veiculosDisponiveis = veiculosDaCategoria.Count - veiculosOcupados;
                Console.WriteLine($"Debug: {veiculosDisponiveis} veículos disponíveis de {veiculosDaCategoria.Count} total");

                return veiculosDisponiveis > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na validação de disponibilidade: {ex.Message}");
                return false; // Em caso de erro, rejeita por segurança
            }
        }

        // Geração de protocolo único
        private string GerarProtocolo()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(100, 999);
            return $"{timestamp}{random}";
        }
    }
}