using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SislocDbContext context)
        {
            context.Database.EnsureCreated();

            // Verifica se já existem dados
            if (context.Agendamentos.Any())
            {
                return; // Banco já foi populado
            }

            // Aqui você pode adicionar dados iniciais adicionais se necessário ao banco de dados
            // Por exemplo, um agendamento de teste (apenas se for necessário):
            /* 
            var agendamentoTeste = new Agendamento
            {
                Protocolo = "20250104120000", // Protocolo fixo para teste
                DataPartida = new DateTime(2025, 7, 10, 8, 0, 0), // Data fixa
                DataChegada = new DateTime(2025, 7, 10, 12, 0, 0), // Data fixa
                NomeSolicitante = "Teste do Sistema",
                QuantidadePessoas = 2,
                Destino = "Centro da Cidade",
                Descricao = "Agendamento de teste para verificar o sistema",
                CategoriaVeiculo = CategoriaVeiculo.Hatch,
                PrecisaMotorista = true,
                Status = StatusAgendamento.Pendente,
                DataCriacao = new DateTime(2025, 7, 4, 10, 0, 0) // Data fixa
            };

            context.Agendamentos.Add(agendamentoTeste);
            context.SaveChanges();
            */
        }

        public static string GenerateProtocol()
        {
            // Gera um protocolo único baseado na data/hora atual
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}