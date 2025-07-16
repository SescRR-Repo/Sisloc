// ViewModels/MotoristaIndexViewModel.cs
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;
using Sisloc.Services;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para listagem de motoristas com filtros e paginação.
    /// </summary>
    public class MotoristaIndexViewModel
    {
        public PageList<Motorista> Motoristas { get; set; } = new PageList<Motorista>(new List<Motorista>(), 0, 1, 10);

        // Filtros
        public string? FiltroTexto { get; set; }
        public string? CategoriaCnhFiltro { get; set; }
        public StatusMotorista? StatusFiltro { get; set; }
        public bool? DocumentacaoVencidaFiltro { get; set; }

        // Paginação
        public int PaginaAtual { get; set; } = 1;
        public int ItensPorPagina { get; set; } = 10;

        // Estatísticas
        public MotoristaEstatisticasDto? Estatisticas { get; set; }

        // Alertas
        public MotoristaAlertasDto? AlertasVencimento { get; set; }

        // Para construção de URLs com filtros
        public string ObterUrlComFiltros(int pagina)
        {
            var parametros = new List<string>();

            if (!string.IsNullOrWhiteSpace(FiltroTexto))
                parametros.Add($"filtroTexto={Uri.EscapeDataString(FiltroTexto)}");

            if (!string.IsNullOrWhiteSpace(CategoriaCnhFiltro))
                parametros.Add($"categoriaCnh={Uri.EscapeDataString(CategoriaCnhFiltro)}");

            if (StatusFiltro.HasValue)
                parametros.Add($"status={StatusFiltro}");

            if (DocumentacaoVencidaFiltro.HasValue)
                parametros.Add($"documentacaoVencida={DocumentacaoVencidaFiltro.Value.ToString().ToLower()}");

            parametros.Add($"pageIndex={pagina}");
            parametros.Add($"pageSize={ItensPorPagina}");

            return parametros.Any() ? "?" + string.Join("&", parametros) : "";
        }

        // Propriedades auxiliares para exibição
        public bool TemAlertas => AlertasVencimento?.TemAlertas ?? false;
        public int TotalAlertas => (AlertasVencimento?.CnhVencendoEm30Dias ?? 0) +
                                  (AlertasVencimento?.ExameToxicologicoVencendoEm60Dias ?? 0) +
                                  (AlertasVencimento?.DocumentacaoVencida ?? 0);
    }
}