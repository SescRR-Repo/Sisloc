// Services/IVeiculoService.cs
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;

namespace Sisloc.Services
{
    /// <summary>
    /// Interface para operações relacionadas a veículos.
    /// </summary>
    public interface IVeiculoService
    {
        /// <summary>
        /// Obtém lista paginada de veículos com filtros opcionais.
        /// </summary>
        Task<PageList<Veiculo>> ObterVeiculosPaginadosAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? filtroTexto = null,
            CategoriaVeiculo? categoria = null,
            StatusVeiculo? status = null);

        /// <summary>
        /// Obtém veículo por ID.
        /// </summary>
        Task<Veiculo?> ObterPorIdAsync(int id);

        /// <summary>
        /// Obtém veículo por placa.
        /// </summary>
        Task<Veiculo?> ObterPorPlacaAsync(string placa);

        /// <summary>
        /// Cria novo veículo.
        /// </summary>
        Task<Veiculo> CriarAsync(Veiculo veiculo);

        /// <summary>
        /// Atualiza veículo existente.
        /// </summary>
        Task<Veiculo> AtualizarAsync(Veiculo veiculo);

        /// <summary>
        /// Remove veículo (soft delete alterando status).
        /// </summary>
        Task<bool> RemoverAsync(int id);

        /// <summary>
        /// Verifica se placa já existe (para validação).
        /// </summary>
        Task<bool> PlacaExisteAsync(string placa, int? idExcluir = null);

        /// <summary>
        /// Verifica se veículo pode ser removido (não tem agendamentos ativos).
        /// </summary>
        Task<bool> PodeSerRemovidoAsync(int id);

        /// <summary>
        /// Obtém veículos disponíveis por categoria e período.
        /// </summary>
        Task<List<Veiculo>> ObterDisponivelPorCategoriaAsync(
            CategoriaVeiculo categoria,
            DateTime dataInicio,
            DateTime dataFim);

        /// <summary>
        /// Obtém estatísticas de veículos para dashboard.
        /// </summary>
        Task<VeiculoEstatisticasDto> ObterEstatisticasAsync();
    }

    /// <summary>
    /// DTO para estatísticas de veículos.
    /// </summary>
    public class VeiculoEstatisticasDto
    {
        public int TotalVeiculos { get; set; }
        public int TotalDisponiveis { get; set; }
        public int TotalEmUso { get; set; }
        public int TotalManutencao { get; set; }
        public int TotalReservados { get; set; }
        public Dictionary<CategoriaVeiculo, int> PorCategoria { get; set; } = new();
    }
}