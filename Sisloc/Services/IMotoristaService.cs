// Services/IMotoristaService.cs
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;

namespace Sisloc.Services
{
    /// <summary>
    /// Interface para operações relacionadas a motoristas.
    /// </summary>
    public interface IMotoristaService
    {
        /// <summary>
        /// Obtém lista paginada de motoristas com filtros opcionais.
        /// </summary>
        Task<PageList<Motorista>> ObterMotoristasPaginadosAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? filtroTexto = null,
            string? categoriaCnh = null,
            StatusMotorista? status = null,
            bool? documentacaoVencida = null);

        /// <summary>
        /// Obtém motorista por ID.
        /// </summary>
        Task<Motorista?> ObterPorIdAsync(int id);

        /// <summary>
        /// Obtém motorista por número da CNH.
        /// </summary>
        Task<Motorista?> ObterPorCnhAsync(string numeroCnh);

        /// <summary>
        /// Cria novo motorista.
        /// </summary>
        Task<Motorista> CriarAsync(Motorista motorista);

        /// <summary>
        /// Atualiza motorista existente.
        /// </summary>
        Task<Motorista> AtualizarAsync(Motorista motorista);

        /// <summary>
        /// Remove motorista (soft delete alterando status).
        /// </summary>
        Task<bool> RemoverAsync(int id);

        /// <summary>
        /// Verifica se CNH já existe (para validação).
        /// </summary>
        Task<bool> CnhExisteAsync(string numeroCnh, int? idExcluir = null);

        /// <summary>
        /// Verifica se motorista pode ser removido (não tem agendamentos ativos).
        /// </summary>
        Task<bool> PodeSerRemovidoAsync(int id);

        /// <summary>
        /// Obtém motoristas disponíveis por categoria CNH e período.
        /// </summary>
        Task<List<Motorista>> ObterDisponivelPorCategoriaAsync(
            string categoriaCnh,
            DateTime dataInicio,
            DateTime dataFim);

        /// <summary>
        /// Obtém estatísticas de motoristas para dashboard.
        /// </summary>
        Task<MotoristaEstatisticasDto> ObterEstatisticasAsync();

        /// <summary>
        /// Obtém alertas de vencimento de documentação.
        /// </summary>
        Task<MotoristaAlertasDto> ObterAlertasVencimentoAsync();

        /// <summary>
        /// Obtém motoristas com documentação próxima ao vencimento.
        /// </summary>
        Task<List<MotoristaVencimentoDto>> ObterMotoristasComVencimentoAsync(int diasAlerta = 60);

        /// <summary>
        /// Verifica se motorista está disponível em um período específico.
        /// </summary>
        Task<bool> EstaDisponivelAsync(int motoristaId, DateTime dataInicio, DateTime dataFim);

        /// <summary>
        /// Obtém motoristas por status de documentação.
        /// </summary>
        Task<List<Motorista>> ObterPorStatusDocumentacaoAsync(StatusDocumentacao statusDocumentacao);

        /// <summary>
        /// Atualiza status do motorista automaticamente baseado na documentação.
        /// </summary>
        Task AtualizarStatusPorDocumentacaoAsync();
    }

    /// <summary>
    /// DTO para estatísticas de motoristas.
    /// </summary>
    public class MotoristaEstatisticasDto
    {
        public int TotalMotoristas { get; set; }
        public int TotalDisponiveis { get; set; }
        public int TotalOcupados { get; set; }
        public int TotalFolga { get; set; }
        public int TotalIrregulares { get; set; }
        public int ComDocumentacaoOk { get; set; }
        public int ComDocumentacaoVencida { get; set; }
        public int ComAlertaVencimento { get; set; }
        public Dictionary<string, int> PorCategoriaCnh { get; set; } = new();
    }

    /// <summary>
    /// DTO para alertas de vencimento.
    /// </summary>
    public class MotoristaAlertasDto
    {
        public int CnhVencendoEm30Dias { get; set; }
        public int ExameToxicologicoVencendoEm60Dias { get; set; }
        public int DocumentacaoVencida { get; set; }
        public List<MotoristaVencimentoDto> MotoristasComAlerta { get; set; } = new();
        public bool TemAlertas => CnhVencendoEm30Dias > 0 || ExameToxicologicoVencendoEm60Dias > 0 || DocumentacaoVencida > 0;
    }

    /// <summary>
    /// DTO para informações de vencimento de motorista.
    /// </summary>
    public class MotoristaVencimentoDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string NumeroCnh { get; set; } = string.Empty;
        public string CategoriaCnh { get; set; } = string.Empty;
        public DateTime VencimentoCnh { get; set; }
        public DateTime DataExameToxicologico { get; set; }
        public StatusMotorista Status { get; set; }

        // Propriedades calculadas
        public int DiasParaVencimentoCnh => (VencimentoCnh - DateTime.Now).Days;
        public int DiasParaVencimentoExameTox => (DataExameToxicologico.AddYears(2) - DateTime.Now).Days;
        public bool CnhVencida => DiasParaVencimentoCnh < 0;
        public bool ExameToxVencido => DiasParaVencimentoExameTox < 0;
        public bool CnhVenceEm30Dias => DiasParaVencimentoCnh <= 30 && DiasParaVencimentoCnh >= 0;
        public bool ExameToxVenceEm60Dias => DiasParaVencimentoExameTox <= 60 && DiasParaVencimentoExameTox >= 0;
        public StatusDocumentacao StatusDocumentacao => GetStatusDocumentacao();

        public string TipoAlerta
        {
            get
            {
                if (CnhVencida || ExameToxVencido) return "Vencido";
                if (CnhVenceEm30Dias) return "CNH vence em breve";
                if (ExameToxVenceEm60Dias) return "Exame toxicológico vence em breve";
                return "OK";
            }
        }

        public string ClasseAlerta
        {
            get
            {
                if (CnhVencida || ExameToxVencido) return "danger";
                if (CnhVenceEm30Dias || ExameToxVenceEm60Dias) return "warning";
                return "success";
            }
        }

        private StatusDocumentacao GetStatusDocumentacao()
        {
            if (CnhVencida || ExameToxVencido)
                return StatusDocumentacao.Vencida;

            if (CnhVenceEm30Dias || ExameToxVenceEm60Dias)
                return StatusDocumentacao.ProximaVencimento;

            return StatusDocumentacao.Ok;
        }
    }

    /// <summary>
    /// Enum para status da documentação.
    /// </summary>
    public enum StatusDocumentacao
    {
        Ok,
        ProximaVencimento,
        Vencida
    }
}