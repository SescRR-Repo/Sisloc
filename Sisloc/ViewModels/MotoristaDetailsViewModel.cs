// ViewModels/MotoristaDetailsViewModel.cs
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para detalhes de motoristas.
    /// </summary>
    public class MotoristaDetailsViewModel
    {
        public Motorista Motorista { get; set; } = new();

        // Agendamentos relacionados
        public List<Agendamento> AgendamentosAtivos { get; set; } = new();
        public List<Agendamento> HistoricoAgendamentos { get; set; } = new();

        // Estatísticas do motorista
        public int TotalAgendamentos { get; set; }
        public int AgendamentosEsteAno { get; set; }
        public int AgendamentosEsteMes { get; set; }
        public DateTime? UltimaUtilizacao { get; set; }

        // Alertas de vencimento
        public int CnhVenceEm { get; set; }
        public int ExameToxicologicoVenceEm { get; set; }
        public bool TemAlertaVencimento { get; set; }

        // Status e disponibilidade
        public bool EstaDisponivel => Motorista.Status == StatusMotorista.Disponivel && !DocumentacaoVencida;
        public bool PodeSerEditado => !AgendamentosAtivos.Any();
        public bool PodeSerRemovido => !AgendamentosAtivos.Any() &&
            !HistoricoAgendamentos.Any(a => a.Status == StatusAgendamento.Pendente);

        // Propriedades calculadas para documentação
        public bool CnhVencida => CnhVenceEm < 0;
        public bool ExameToxVencido => ExameToxicologicoVenceEm < 0;
        public bool DocumentacaoVencida => CnhVencida || ExameToxVencido;
        public bool CnhProximaVencimento => CnhVenceEm <= 30 && CnhVenceEm >= 0;
        public bool ExameToxProximoVencimento => ExameToxicologicoVenceEm <= 60 && ExameToxicologicoVenceEm >= 0;

        public string StatusDocumentacao
        {
            get
            {
                if (DocumentacaoVencida) return "Vencida";
                if (CnhProximaVencimento || ExameToxProximoVencimento) return "Próxima ao Vencimento";
                return "OK";
            }
        }

        public string ClasseStatusDocumentacao
        {
            get
            {
                if (DocumentacaoVencida) return "danger";
                if (CnhProximaVencimento || ExameToxProximoVencimento) return "warning";
                return "success";
            }
        }

        public string ProximaDisponibilidade
        {
            get
            {
                if (DocumentacaoVencida)
                    return "Indisponível - Documentação vencida";

                var proximoFim = AgendamentosAtivos
                    .Where(a => a.Status == StatusAgendamento.Aprovado || a.Status == StatusAgendamento.EmAndamento)
                    .OrderBy(a => a.DataChegada)
                    .FirstOrDefault()?.DataChegada;

                if (proximoFim.HasValue)
                    return proximoFim.Value.ToString("dd/MM/yyyy HH:mm");

                return EstaDisponivel ? "Disponível" : Motorista.Status.ToString();
            }
        }

        public List<string> GetAlertasDocumentacao()
        {
            var alertas = new List<string>();

            if (CnhVencida)
                alertas.Add($"CNH vencida há {Math.Abs(CnhVenceEm)} dias");
            else if (CnhProximaVencimento)
                alertas.Add($"CNH vence em {CnhVenceEm} dias");

            if (ExameToxVencido)
                alertas.Add($"Exame toxicológico vencido há {Math.Abs(ExameToxicologicoVenceEm)} dias");
            else if (ExameToxProximoVencimento)
                alertas.Add($"Exame toxicológico vence em {ExameToxicologicoVenceEm} dias");

            return alertas;
        }

        public string GetTextoUltimaUtilizacao()
        {
            if (!UltimaUtilizacao.HasValue)
                return "Nenhuma utilização registrada";

            var diasDesdeUltima = (DateTime.Now - UltimaUtilizacao.Value).Days;

            if (diasDesdeUltima == 0)
                return "Hoje";
            else if (diasDesdeUltima == 1)
                return "Ontem";
            else if (diasDesdeUltima <= 7)
                return $"Há {diasDesdeUltima} dias";
            else if (diasDesdeUltima <= 30)
                return $"Há {diasDesdeUltima / 7} semana(s)";
            else
                return UltimaUtilizacao.Value.ToString("dd/MM/yyyy");
        }

        // Informações de categoria CNH
        public List<string> GetCategoriasCompativeis()
        {
            return Motorista.CategoriaCnh switch
            {
                "A" => new List<string> { "Motocicletas" },
                "B" => new List<string> { "Carros de passeio", "Hatch", "Sedan" },
                "C" => new List<string> { "Caminhões pequenos", "Caminhonete" },
                "D" => new List<string> { "Ônibus", "Micro-ônibus" },
                "E" => new List<string> { "Caminhões grandes", "Caminhão" },
                "AB" => new List<string> { "Motocicletas", "Carros de passeio", "Hatch", "Sedan" },
                "AC" => new List<string> { "Motocicletas", "Carros de passeio", "Caminhões pequenos", "Hatch", "Sedan", "Caminhonete" },
                "AD" => new List<string> { "Motocicletas", "Carros de passeio", "Ônibus", "Hatch", "Sedan" },
                "AE" => new List<string> { "Motocicletas", "Carros de passeio", "Caminhões", "Hatch", "Sedan", "Caminhonete", "Caminhão" },
                _ => new List<string> { "Categoria não reconhecida" }
            };
        }

        public bool PodeConduzirCategoria(CategoriaVeiculo categoria)
        {
            return categoria switch
            {
                CategoriaVeiculo.Hatch => new[] { "B", "AB", "AC", "AD", "AE" }.Contains(Motorista.CategoriaCnh),
                CategoriaVeiculo.Sedan => new[] { "B", "AB", "AC", "AD", "AE" }.Contains(Motorista.CategoriaCnh),
                CategoriaVeiculo.Caminhonete => new[] { "C", "AC", "AE" }.Contains(Motorista.CategoriaCnh),
                CategoriaVeiculo.Pickup => new[] { "C", "AC", "AE" }.Contains(Motorista.CategoriaCnh),
                CategoriaVeiculo.Caminhao => new[] { "E", "AE" }.Contains(Motorista.CategoriaCnh),
                _ => false
            };
        }
    }
}