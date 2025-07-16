// Services/MotoristaService.cs
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;
using System.Text.RegularExpressions;

namespace Sisloc.Services
{
    /// <summary>
    /// Implementação dos serviços de motoristas.
    /// </summary>
    public class MotoristaService : IMotoristaService
    {
        private readonly SislocDbContext _context;

        public MotoristaService(SislocDbContext context)
        {
            _context = context;
        }

        public async Task<PageList<Motorista>> ObterMotoristasPaginadosAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? filtroTexto = null,
            string? categoriaCnh = null,
            StatusMotorista? status = null,
            bool? documentacaoVencida = null)
        {
            var query = _context.Motoristas.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                filtroTexto = filtroTexto.Trim().ToUpper();
                query = query.Where(m =>
                    m.NomeCompleto.ToUpper().Contains(filtroTexto) ||
                    m.NumeroCnh.ToUpper().Contains(filtroTexto) ||
                    m.Telefone.Contains(filtroTexto));
            }

            if (!string.IsNullOrWhiteSpace(categoriaCnh))
            {
                query = query.Where(m => m.CategoriaCnh == categoriaCnh);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            if (documentacaoVencida.HasValue)
            {
                var hoje = DateTime.Now;
                if (documentacaoVencida.Value)
                {
                    // Documentação vencida ou próxima ao vencimento
                    query = query.Where(m =>
                        m.VencimentoCnh <= hoje.AddDays(30) ||
                        m.DataExameToxicologico.AddYears(2) <= hoje.AddDays(60));
                }
                else
                {
                    // Documentação OK
                    query = query.Where(m =>
                        m.VencimentoCnh > hoje.AddDays(30) &&
                        m.DataExameToxicologico.AddYears(2) > hoje.AddDays(60));
                }
            }

            // Ordenação padrão
            query = query.OrderBy(m => m.NomeCompleto);

            return await PageList<Motorista>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<Motorista?> ObterPorIdAsync(int id)
        {
            return await _context.Motoristas
                .Include(m => m.Agendamentos.Where(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento))
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Motorista?> ObterPorCnhAsync(string numeroCnh)
        {
            return await _context.Motoristas
                .FirstOrDefaultAsync(m => m.NumeroCnh == numeroCnh);
        }

        public async Task<Motorista> CriarAsync(Motorista motorista)
        {
            // Validações de negócio
            await ValidarMotoristaAsync(motorista);

            // Normalizar dados
            motorista.NomeCompleto = NormalizarNome(motorista.NomeCompleto);
            motorista.NumeroCnh = NormalizarCnh(motorista.NumeroCnh);
            motorista.CategoriaCnh = motorista.CategoriaCnh.ToUpper();
            motorista.Telefone = NormalizarTelefone(motorista.Telefone);

            // Definir status baseado na documentação
            motorista.Status = CalcularStatusPorDocumentacao(motorista);

            _context.Motoristas.Add(motorista);
            await _context.SaveChangesAsync();

            return motorista;
        }

        public async Task<Motorista> AtualizarAsync(Motorista motorista)
        {
            var motoristaExistente = await _context.Motoristas.FindAsync(motorista.Id);
            if (motoristaExistente == null)
                throw new InvalidOperationException("Motorista não encontrado.");

            // Validações de negócio
            await ValidarMotoristaAsync(motorista, motorista.Id);

            // Atualizar propriedades
            motoristaExistente.NomeCompleto = NormalizarNome(motorista.NomeCompleto);
            motoristaExistente.NumeroCnh = NormalizarCnh(motorista.NumeroCnh);
            motoristaExistente.VencimentoCnh = motorista.VencimentoCnh;
            motoristaExistente.CategoriaCnh = motorista.CategoriaCnh.ToUpper();
            motoristaExistente.Telefone = NormalizarTelefone(motorista.Telefone);
            motoristaExistente.DataExameToxicologico = motorista.DataExameToxicologico;
            motoristaExistente.Observacoes = motorista.Observacoes?.Trim();

            // Atualizar status se não foi definido manualmente
            if (motorista.Status == StatusMotorista.Disponivel || motorista.Status == StatusMotorista.Irregular)
            {
                motoristaExistente.Status = CalcularStatusPorDocumentacao(motorista);
            }
            else
            {
                motoristaExistente.Status = motorista.Status;
            }

            await _context.SaveChangesAsync();
            return motoristaExistente;
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var motorista = await _context.Motoristas.FindAsync(id);
            if (motorista == null)
                return false;

            // Verificar se pode ser removido
            if (!await PodeSerRemovidoAsync(id))
                throw new InvalidOperationException("Não é possível remover motorista com agendamentos ativos.");

            // Soft delete - alterar status ao invés de remover
            motorista.Status = StatusMotorista.Irregular;
            motorista.Observacoes = $"[REMOVIDO EM {DateTime.Now:dd/MM/yyyy}] " + motorista.Observacoes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CnhExisteAsync(string numeroCnh, int? idExcluir = null)
        {
            numeroCnh = NormalizarCnh(numeroCnh);

            var query = _context.Motoristas.Where(m => m.NumeroCnh == numeroCnh);

            if (idExcluir.HasValue)
                query = query.Where(m => m.Id != idExcluir.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> PodeSerRemovidoAsync(int id)
        {
            return !await _context.Agendamentos.AnyAsync(a =>
                a.MotoristaAlocadoId == id &&
                (a.Status == StatusAgendamento.Aprovado ||
                 a.Status == StatusAgendamento.EmAndamento ||
                 a.Status == StatusAgendamento.Pendente));
        }

        public async Task<List<Motorista>> ObterDisponivelPorCategoriaAsync(
            string categoriaCnh,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.CategoriaCnh == categoriaCnh.ToUpper() &&
                           m.Status != StatusMotorista.Irregular &&
                           m.VencimentoCnh > DateTime.Now &&
                           m.DataExameToxicologico.AddYears(2) > DateTime.Now)
                .ToListAsync();

            var motoristasDisponiveis = new List<Motorista>();

            foreach (var motorista in motoristas)
            {
                var temConflito = await _context.Agendamentos
                    .Where(a => a.MotoristaAlocadoId == motorista.Id &&
                               (a.Status == StatusAgendamento.Aprovado ||
                                a.Status == StatusAgendamento.EmAndamento))
                    .AnyAsync(a => dataInicio < a.DataChegada && dataFim > a.DataPartida);

                if (!temConflito)
                    motoristasDisponiveis.Add(motorista);
            }

            return motoristasDisponiveis;
        }

        public async Task<MotoristaEstatisticasDto> ObterEstatisticasAsync()
        {
            var motoristas = await _context.Motoristas.ToListAsync();
            var hoje = DateTime.Now;

            var estatisticas = new MotoristaEstatisticasDto
            {
                TotalMotoristas = motoristas.Count,
                TotalDisponiveis = motoristas.Count(m => m.Status == StatusMotorista.Disponivel),
                TotalOcupados = motoristas.Count(m => m.Status == StatusMotorista.Ocupado),
                TotalFolga = motoristas.Count(m => m.Status == StatusMotorista.Folga),
                TotalIrregulares = motoristas.Count(m => m.Status == StatusMotorista.Irregular)
            };

            // Estatísticas de documentação
            foreach (var motorista in motoristas)
            {
                var cnhVenceEm = (motorista.VencimentoCnh - hoje).Days;
                var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - hoje).Days;

                if (cnhVenceEm < 0 || exameToxVenceEm < 0)
                {
                    estatisticas.ComDocumentacaoVencida++;
                }
                else if (cnhVenceEm <= 30 || exameToxVenceEm <= 60)
                {
                    estatisticas.ComAlertaVencimento++;
                }
                else
                {
                    estatisticas.ComDocumentacaoOk++;
                }
            }

            // Estatísticas por categoria CNH
            var categorias = new[] { "A", "B", "C", "D", "E", "AB", "AC", "AD", "AE" };
            foreach (var categoria in categorias)
            {
                estatisticas.PorCategoriaCnh[categoria] = motoristas.Count(m => m.CategoriaCnh == categoria);
            }

            return estatisticas;
        }

        public async Task<MotoristaAlertasDto> ObterAlertasVencimentoAsync()
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.Status != StatusMotorista.Irregular)
                .ToListAsync();

            var hoje = DateTime.Now;
            var alertas = new MotoristaAlertasDto();

            foreach (var motorista in motoristas)
            {
                var cnhVenceEm = (motorista.VencimentoCnh - hoje).Days;
                var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - hoje).Days;

                var motoristaDto = new MotoristaVencimentoDto
                {
                    Id = motorista.Id,
                    NomeCompleto = motorista.NomeCompleto,
                    NumeroCnh = motorista.NumeroCnh,
                    CategoriaCnh = motorista.CategoriaCnh,
                    VencimentoCnh = motorista.VencimentoCnh,
                    DataExameToxicologico = motorista.DataExameToxicologico,
                    Status = motorista.Status
                };

                if (cnhVenceEm < 0 || exameToxVenceEm < 0)
                {
                    alertas.DocumentacaoVencida++;
                    alertas.MotoristasComAlerta.Add(motoristaDto);
                }
                else if (cnhVenceEm <= 30)
                {
                    alertas.CnhVencendoEm30Dias++;
                    alertas.MotoristasComAlerta.Add(motoristaDto);
                }
                else if (exameToxVenceEm <= 60)
                {
                    alertas.ExameToxicologicoVencendoEm60Dias++;
                    alertas.MotoristasComAlerta.Add(motoristaDto);
                }
            }

            return alertas;
        }

        public async Task<List<MotoristaVencimentoDto>> ObterMotoristasComVencimentoAsync(int diasAlerta = 60)
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.Status != StatusMotorista.Irregular)
                .OrderBy(m => m.NomeCompleto)
                .ToListAsync();

            var hoje = DateTime.Now;
            var resultado = new List<MotoristaVencimentoDto>();

            foreach (var motorista in motoristas)
            {
                var cnhVenceEm = (motorista.VencimentoCnh - hoje).Days;
                var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - hoje).Days;

                if (cnhVenceEm <= diasAlerta || exameToxVenceEm <= diasAlerta)
                {
                    resultado.Add(new MotoristaVencimentoDto
                    {
                        Id = motorista.Id,
                        NomeCompleto = motorista.NomeCompleto,
                        NumeroCnh = motorista.NumeroCnh,
                        CategoriaCnh = motorista.CategoriaCnh,
                        VencimentoCnh = motorista.VencimentoCnh,
                        DataExameToxicologico = motorista.DataExameToxicologico,
                        Status = motorista.Status
                    });
                }
            }

            return resultado.OrderBy(m => m.DiasParaVencimentoCnh).ThenBy(m => m.DiasParaVencimentoExameTox).ToList();
        }

        public async Task<bool> EstaDisponivelAsync(int motoristaId, DateTime dataInicio, DateTime dataFim)
        {
            // Verificar se o motorista existe e tem status adequado
            var motorista = await _context.Motoristas.FindAsync(motoristaId);
            if (motorista == null || motorista.Status == StatusMotorista.Irregular)
                return false;

            // Verificar documentação válida
            if (motorista.VencimentoCnh <= DateTime.Now ||
                motorista.DataExameToxicologico.AddYears(2) <= DateTime.Now)
                return false;

            // Verificar conflitos de agendamento
            return !await _context.Agendamentos
                .Where(a => a.MotoristaAlocadoId == motoristaId &&
                           (a.Status == StatusAgendamento.Aprovado ||
                            a.Status == StatusAgendamento.EmAndamento))
                .AnyAsync(a => dataInicio < a.DataChegada && dataFim > a.DataPartida);
        }

        public async Task<List<Motorista>> ObterPorStatusDocumentacaoAsync(StatusDocumentacao statusDocumentacao)
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.Status != StatusMotorista.Irregular)
                .ToListAsync();

            var hoje = DateTime.Now;
            var resultado = new List<Motorista>();

            foreach (var motorista in motoristas)
            {
                var cnhVenceEm = (motorista.VencimentoCnh - hoje).Days;
                var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - hoje).Days;

                var status = GetStatusDocumentacao(cnhVenceEm, exameToxVenceEm);
                if (status == statusDocumentacao)
                {
                    resultado.Add(motorista);
                }
            }

            return resultado.OrderBy(m => m.NomeCompleto).ToList();
        }

        public async Task AtualizarStatusPorDocumentacaoAsync()
        {
            var motoristas = await _context.Motoristas
                .Where(m => m.Status != StatusMotorista.Folga && m.Status != StatusMotorista.Ocupado)
                .ToListAsync();

            foreach (var motorista in motoristas)
            {
                var novoStatus = CalcularStatusPorDocumentacao(motorista);
                if (motorista.Status != novoStatus)
                {
                    motorista.Status = novoStatus;
                }
            }

            await _context.SaveChangesAsync();
        }

        #region Métodos Privados

        private async Task ValidarMotoristaAsync(Motorista motorista, int? idExcluir = null)
        {
            // Validar nome
            if (string.IsNullOrWhiteSpace(motorista.NomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório.");

            // Validar CNH
            if (!ValidarFormatoCnh(motorista.NumeroCnh))
                throw new ArgumentException("Formato de CNH inválido. Deve conter 11 dígitos.");

            // Verificar se CNH já existe
            if (await CnhExisteAsync(motorista.NumeroCnh, idExcluir))
                throw new ArgumentException("Já existe um motorista cadastrado com esta CNH.");

            // Validar categoria CNH
            if (!ValidarCategoriaCnh(motorista.CategoriaCnh))
                throw new ArgumentException("Categoria de CNH inválida.");

            // Validar vencimento CNH
            if (motorista.VencimentoCnh <= DateTime.Now.AddDays(-1))
                throw new ArgumentException("CNH não pode estar vencida.");

            // Validar data do exame toxicológico
            if (motorista.DataExameToxicologico > DateTime.Now)
                throw new ArgumentException("Data do exame toxicológico não pode ser futura.");

            // Validar telefone
            if (!ValidarFormatoTelefone(motorista.Telefone))
                throw new ArgumentException("Formato de telefone inválido.");
        }

        private static bool ValidarFormatoCnh(string numeroCnh)
        {
            if (string.IsNullOrWhiteSpace(numeroCnh))
                return false;

            // Remove espaços e caracteres especiais
            numeroCnh = Regex.Replace(numeroCnh, @"\D", "");

            // CNH deve ter 11 dígitos
            return numeroCnh.Length == 11 && numeroCnh.All(char.IsDigit);
        }

        private static bool ValidarCategoriaCnh(string categoria)
        {
            var categoriasValidas = new[] { "A", "B", "C", "D", "E", "AB", "AC", "AD", "AE" };
            return categoriasValidas.Contains(categoria?.ToUpper());
        }

        private static bool ValidarFormatoTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return false;

            // Remove espaços e caracteres especiais
            var numeroLimpo = Regex.Replace(telefone, @"\D", "");

            // Telefone deve ter 10 ou 11 dígitos (com ou sem celular)
            return numeroLimpo.Length == 10 || numeroLimpo.Length == 11;
        }

        private static string NormalizarNome(string nome)
        {
            return nome?.Trim().ToTitleCase() ?? string.Empty;
        }

        private static string NormalizarCnh(string cnh)
        {
            if (string.IsNullOrWhiteSpace(cnh))
                return string.Empty;

            // Remove tudo que não é dígito
            return Regex.Replace(cnh, @"\D", "");
        }

        private static string NormalizarTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            // Remove tudo que não é dígito
            var numeroLimpo = Regex.Replace(telefone, @"\D", "");

            // Formata o telefone
            if (numeroLimpo.Length == 11)
                return $"({numeroLimpo.Substring(0, 2)}) {numeroLimpo.Substring(2, 5)}-{numeroLimpo.Substring(7, 4)}";
            else if (numeroLimpo.Length == 10)
                return $"({numeroLimpo.Substring(0, 2)}) {numeroLimpo.Substring(2, 4)}-{numeroLimpo.Substring(6, 4)}";

            return telefone;
        }

        private static StatusMotorista CalcularStatusPorDocumentacao(Motorista motorista)
        {
            var hoje = DateTime.Now;
            var cnhVenceEm = (motorista.VencimentoCnh - hoje).Days;
            var exameToxVenceEm = (motorista.DataExameToxicologico.AddYears(2) - hoje).Days;

            // Se alguma documentação estiver vencida
            if (cnhVenceEm < 0 || exameToxVenceEm < 0)
                return StatusMotorista.Irregular;

            // Se documentação está OK
            return StatusMotorista.Disponivel;
        }

        private static StatusDocumentacao GetStatusDocumentacao(int cnhVenceEm, int exameToxVenceEm)
        {
            if (cnhVenceEm < 0 || exameToxVenceEm < 0)
                return StatusDocumentacao.Vencida;

            if (cnhVenceEm <= 30 || exameToxVenceEm <= 60)
                return StatusDocumentacao.ProximaVencimento;

            return StatusDocumentacao.Ok;
        }

        #endregion
    }

    /// <summary>
    /// Extensão para converter string para Title Case.
    /// </summary>
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var words = input.ToLower().Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }
            return string.Join(" ", words);
        }
    }
}