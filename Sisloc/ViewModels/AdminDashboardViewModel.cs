using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<Agendamento> Agendamentos { get; set; } = new();

        // Filtros
        public StatusAgendamento? StatusFiltro { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public CategoriaVeiculo? CategoriaFiltro { get; set; }

        // Estatísticas
        public int TotalPendentes { get; set; }
        public int TotalAprovados { get; set; }
        public int TotalRejeitados { get; set; }
        public int TotalHoje { get; set; }
    }
}