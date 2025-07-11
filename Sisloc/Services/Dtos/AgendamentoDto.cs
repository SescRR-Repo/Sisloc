using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sisloc.Data;
using Sisloc.Models;
using Sisloc.Models.Enums;
using Sisloc.Services.Dtos;

namespace Sisloc.Services.Dtos
{
    /// <summary>
    /// Data transfer object para criação de um agendamento.
    /// </summary>
    public class AgendamentoDto
    {
        /// <summary>Data e hora de partida.</summary>
        public DateTime DataPartida { get; set; }

        /// <summary>Data e hora de chegada.</summary>
        public DateTime DataChegada { get; set; }

        /// <summary>Nome completo do solicitante.</summary>
        public string NomeSolicitante { get; set; } = string.Empty;

        /// <summary>Quantidade de pessoas no agendamento.</summary>
        public int QuantidadePessoas { get; set; }

        /// <summary>Destino da viagem.</summary>
        public string Destino { get; set; } = string.Empty;

        /// <summary>Descrição ou objetivo da viagem.</summary>
        public string? Descricao { get; set; }

        /// <summary>Categoria requerida do veículo.</summary>
        public CategoriaVeiculo CategoriaVeiculo { get; set; }

        /// <summary>Indica se é necessário motorista.</summary>
        public bool PrecisaMotorista { get; set; }
    }
}

namespace Sisloc.Services
{
    /// <summary>
    /// Contrato para operações de agendamento.
    /// </summary>
    public interface IAgendamentoService
    {
        /// <summary>
        /// Cria um novo agendamento no sistema.
        /// </summary>
        /// <param name="dto">Dados do agendamento.</param>
        /// <returns>Protocolo gerado do agendamento.</returns>
        Task<string> CriarAsync(AgendamentoDto dto);
    }

    /// <summary>
    /// Implementação dos serviços de agendamento.
    /// </summary>
    public class AgendamentoService : IAgendamentoService
    {
        private readonly SislocDbContext _context;

        public AgendamentoService(SislocDbContext context)
        {
            _context = context;
        }

        public async Task<string> CriarAsync(AgendamentoDto dto)
        {
            // 1. Validações de datas
            var hoje = DateTime.Now.Date;
            if (dto.DataPartida.Date < hoje)
                throw new ArgumentException("A data de partida não pode ser anterior ao dia atual.");
            if (dto.DataChegada < dto.DataPartida)
                throw new ArgumentException("A data e hora de chegada devem ser posteriores à partida.");

            // 2. Verificar disponibilidade de veículos na categoria
            var veiculos = await _context.Veiculos
                .Where(v => v.Categoria == dto.CategoriaVeiculo && v.Status == StatusVeiculo.Disponivel)
                .ToListAsync();

            bool disponivel = false;
            foreach (var v in veiculos)
            {
                var temConflito = await _context.Agendamentos
                    .Where(a => a.VeiculoAlocadoId == v.Id && a.Status != StatusAgendamento.Rejeitado && a.Status != StatusAgendamento.Concluido)
                    .AnyAsync(a =>
                        (dto.DataPartida >= a.DataPartida && dto.DataPartida < a.DataChegada) ||
                        (dto.DataChegada > a.DataPartida && dto.DataChegada <= a.DataChegada) ||
                        (dto.DataPartida <= a.DataPartida && dto.DataChegada >= a.DataChegada)
                    );

                if (!temConflito)
                {
                    disponivel = true;
                    break;
                }
            }

            if (!disponivel)
                throw new InvalidOperationException("Não há veículos disponíveis para o período e categoria selecionados.");

            // 3. Mapeamento para entidade e persistência
            var agendamento = new Agendamento
            {
                DataPartida = dto.DataPartida,
                DataChegada = dto.DataChegada,
                NomeSolicitante = dto.NomeSolicitante,
                QuantidadePessoas = dto.QuantidadePessoas,
                Destino = dto.Destino,
                Descricao = dto.Descricao,
                CategoriaVeiculo = dto.CategoriaVeiculo,
                PrecisaMotorista = dto.PrecisaMotorista,
                Status = StatusAgendamento.Pendente,
                DataCriacao = DateTime.Now
            };

            // Geração de protocolo
            agendamento.Protocolo = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            _context.Agendamentos.Add(agendamento);
            await _context.SaveChangesAsync();

            return agendamento.Protocolo;
        }
    }
}
