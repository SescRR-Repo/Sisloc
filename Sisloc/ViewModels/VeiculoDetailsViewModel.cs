// ViewModels/VeiculoDetailsViewModel.cs
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para detalhes de veículos.
    /// </summary>
    public class VeiculoDetailsViewModel
    {
        public Veiculo Veiculo { get; set; } = new();

        // Agendamentos relacionados
        public List<Agendamento> AgendamentosAtivos { get; set; } = new();
        public List<Agendamento> HistoricoAgendamentos { get; set; } = new();

        // Estatísticas do veículo
        public int TotalAgendamentos { get; set; }
        public int AgendamentosEsteAno { get; set; }
        public int AgendamentosEsteMes { get; set; }
        public DateTime? UltimaUtilizacao { get; set; }

        // Status e disponibilidade
        public bool EstaDisponivel => Veiculo.Status == StatusVeiculo.Disponivel;
        public bool PodeSerEditado => !AgendamentosAtivos.Any();
        public bool PodeSerRemovido => !AgendamentosAtivos.Any() &&
            !HistoricoAgendamentos.Any(a => a.Status == StatusAgendamento.Pendente);

        public string ProximaDisponibilidade
        {
            get
            {
                var proximoFim = AgendamentosAtivos
                    .Where(a => a.Status == StatusAgendamento.Aprovado || a.Status == StatusAgendamento.EmAndamento)
                    .OrderBy(a => a.DataChegada)
                    .FirstOrDefault()?.DataChegada;

                return proximoFim?.ToString("dd/MM/yyyy HH:mm") ?? "Disponível";
            }
        }
    }
}