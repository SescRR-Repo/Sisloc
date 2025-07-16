// Services/VeiculoService.cs
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Helpers;
using System.Text.RegularExpressions;

namespace Sisloc.Services
{
    /// <summary>
    /// Implementação dos serviços de veículos.
    /// </summary>
    public class VeiculoService : IVeiculoService
    {
        private readonly SislocDbContext _context;

        public VeiculoService(SislocDbContext context)
        {
            _context = context;
        }

        public async Task<PageList<Veiculo>> ObterVeiculosPaginadosAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? filtroTexto = null,
            CategoriaVeiculo? categoria = null,
            StatusVeiculo? status = null)
        {
            var query = _context.Veiculos.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                filtroTexto = filtroTexto.Trim().ToUpper();
                query = query.Where(v =>
                    v.Placa.ToUpper().Contains(filtroTexto) ||
                    v.Modelo.ToUpper().Contains(filtroTexto));
            }

            if (categoria.HasValue)
            {
                query = query.Where(v => v.Categoria == categoria.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            // Ordenação padrão
            query = query.OrderBy(v => v.Categoria).ThenBy(v => v.Modelo);

            return await PageList<Veiculo>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<Veiculo?> ObterPorIdAsync(int id)
        {
            return await _context.Veiculos
                .Include(v => v.Agendamentos.Where(a =>
                    a.Status == StatusAgendamento.Aprovado ||
                    a.Status == StatusAgendamento.EmAndamento))
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
        {
            return await _context.Veiculos
                .FirstOrDefaultAsync(v => v.Placa == placa.ToUpper());
        }

        public async Task<Veiculo> CriarAsync(Veiculo veiculo)
        {
            // Validações de negócio
            await ValidarVeiculoAsync(veiculo);

            // Normalizar dados
            veiculo.Placa = NormalizarPlaca(veiculo.Placa);
            veiculo.Modelo = veiculo.Modelo.Trim();

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return veiculo;
        }

        public async Task<Veiculo> AtualizarAsync(Veiculo veiculo)
        {
            var veiculoExistente = await _context.Veiculos.FindAsync(veiculo.Id);
            if (veiculoExistente == null)
                throw new InvalidOperationException("Veículo não encontrado.");

            // Validações de negócio
            await ValidarVeiculoAsync(veiculo, veiculo.Id);

            // Atualizar propriedades
            veiculoExistente.Placa = NormalizarPlaca(veiculo.Placa);
            veiculoExistente.Modelo = veiculo.Modelo.Trim();
            veiculoExistente.Categoria = veiculo.Categoria;
            veiculoExistente.CapacidadePassageiros = veiculo.CapacidadePassageiros;
            veiculoExistente.Status = veiculo.Status;
            veiculoExistente.Observacoes = veiculo.Observacoes?.Trim();

            await _context.SaveChangesAsync();
            return veiculoExistente;
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return false;

            // Verificar se pode ser removido
            if (!await PodeSerRemovidoAsync(id))
                throw new InvalidOperationException("Não é possível remover veículo com agendamentos ativos.");

            // Soft delete - alterar status ao invés de remover
            veiculo.Status = StatusVeiculo.Manutencao;
            veiculo.Observacoes = $"[REMOVIDO EM {DateTime.Now:dd/MM/yyyy}] " + veiculo.Observacoes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PlacaExisteAsync(string placa, int? idExcluir = null)
        {
            placa = NormalizarPlaca(placa);

            var query = _context.Veiculos.Where(v => v.Placa == placa);

            if (idExcluir.HasValue)
                query = query.Where(v => v.Id != idExcluir.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> PodeSerRemovidoAsync(int id)
        {
            return !await _context.Agendamentos.AnyAsync(a =>
                a.VeiculoAlocadoId == id &&
                (a.Status == StatusAgendamento.Aprovado ||
                 a.Status == StatusAgendamento.EmAndamento ||
                 a.Status == StatusAgendamento.Pendente));
        }

        public async Task<List<Veiculo>> ObterDisponivelPorCategoriaAsync(
            CategoriaVeiculo categoria,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var veiculos = await _context.Veiculos
                .Where(v => v.Categoria == categoria && v.Status != StatusVeiculo.Manutencao)
                .ToListAsync();

            var veiculosDisponiveis = new List<Veiculo>();

            foreach (var veiculo in veiculos)
            {
                var temConflito = await _context.Agendamentos
                    .Where(a => a.VeiculoAlocadoId == veiculo.Id &&
                               (a.Status == StatusAgendamento.Aprovado ||
                                a.Status == StatusAgendamento.EmAndamento))
                    .AnyAsync(a => dataInicio < a.DataChegada && dataFim > a.DataPartida);

                if (!temConflito)
                    veiculosDisponiveis.Add(veiculo);
            }

            return veiculosDisponiveis;
        }

        public async Task<VeiculoEstatisticasDto> ObterEstatisticasAsync()
        {
            var veiculos = await _context.Veiculos.ToListAsync();

            var estatisticas = new VeiculoEstatisticasDto
            {
                TotalVeiculos = veiculos.Count,
                TotalDisponiveis = veiculos.Count(v => v.Status == StatusVeiculo.Disponivel),
                TotalEmUso = veiculos.Count(v => v.Status == StatusVeiculo.EmUso),
                TotalManutencao = veiculos.Count(v => v.Status == StatusVeiculo.Manutencao),
                TotalReservados = veiculos.Count(v => v.Status == StatusVeiculo.Reservado)
            };

            // Estatísticas por categoria
            foreach (CategoriaVeiculo categoria in Enum.GetValues<CategoriaVeiculo>())
            {
                estatisticas.PorCategoria[categoria] = veiculos.Count(v => v.Categoria == categoria);
            }

            return estatisticas;
        }

        #region Métodos Privados

        private async Task ValidarVeiculoAsync(Veiculo veiculo, int? idExcluir = null)
        {
            // Validar placa
            if (!ValidarFormatoPlaca(veiculo.Placa))
                throw new ArgumentException("Formato de placa inválido. Use o formato ABC-1234 ou ABC1D23.");

            // Verificar se placa já existe
            if (await PlacaExisteAsync(veiculo.Placa, idExcluir))
                throw new ArgumentException("Já existe um veículo cadastrado com esta placa.");

            // Validar modelo
            if (string.IsNullOrWhiteSpace(veiculo.Modelo))
                throw new ArgumentException("Modelo é obrigatório.");

            // Validar capacidade
            if (veiculo.CapacidadePassageiros < 1 || veiculo.CapacidadePassageiros > 50)
                throw new ArgumentException("Capacidade deve ser entre 1 e 50 passageiros.");
        }

        private static bool ValidarFormatoPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return false;

            placa = placa.Replace("-", "").Replace(" ", "").ToUpper();

            // Formato antigo: ABC1234
            var formatoAntigo = Regex.IsMatch(placa, @"^[A-Z]{3}\d{4}$");

            // Formato Mercosul: ABC1D23
            var formatoMercosul = Regex.IsMatch(placa, @"^[A-Z]{3}\d[A-Z]\d{2}$");

            return formatoAntigo || formatoMercosul;
        }

        private static string NormalizarPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return string.Empty;

            // Remove espaços e hífens, converte para maiúsculo
            placa = placa.Replace("-", "").Replace(" ", "").ToUpper();

            // Adiciona hífen no formato correto se for formato antigo
            if (Regex.IsMatch(placa, @"^[A-Z]{3}\d{4}$"))
            {
                return $"{placa.Substring(0, 3)}-{placa.Substring(3, 4)}";
            }

            // Adiciona hífen no formato Mercosul
            if (Regex.IsMatch(placa, @"^[A-Z]{3}\d[A-Z]\d{2}$"))
            {
                return $"{placa.Substring(0, 3)}-{placa.Substring(3, 4)}";
            }

            return placa;
        }

        #endregion
    }
}