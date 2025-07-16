// ViewModels/VeiculoIndexViewModel.cs
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;
using Sisloc.Services;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para listagem de veículos com filtros e paginação.
    /// </summary>
    public class VeiculoIndexViewModel
    {
        public PageList<Veiculo> Veiculos { get; set; } = new PageList<Veiculo>(new List<Veiculo>(), 0, 1, 10);

        // Filtros
        public string? FiltroTexto { get; set; }
        public CategoriaVeiculo? CategoriaFiltro { get; set; }
        public StatusVeiculo? StatusFiltro { get; set; }

        // Paginação
        public int PaginaAtual { get; set; } = 1;
        public int ItensPorPagina { get; set; } = 10;

        // Estatísticas
        public VeiculoEstatisticasDto? Estatisticas { get; set; }

        // Para construção de URLs com filtros
        public string ObterUrlComFiltros(int pagina)
        {
            var parametros = new List<string>();

            if (!string.IsNullOrWhiteSpace(FiltroTexto))
                parametros.Add($"filtroTexto={Uri.EscapeDataString(FiltroTexto)}");

            if (CategoriaFiltro.HasValue)
                parametros.Add($"categoria={CategoriaFiltro}");

            if (StatusFiltro.HasValue)
                parametros.Add($"status={StatusFiltro}");

            parametros.Add($"pageIndex={pagina}");
            parametros.Add($"pageSize={ItensPorPagina}");

            return parametros.Any() ? "?" + string.Join("&", parametros) : "";
        }
    }
}