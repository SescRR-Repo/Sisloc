using System.ComponentModel.DataAnnotations;
using Sisloc.Models;

namespace Sisloc.ViewModels
{
    public class AprovacaoViewModel
    {
        public Agendamento Agendamento { get; set; } = new();

        [Display(Name = "Veículo")]
        public int? VeiculoSelecionadoId { get; set; }

        [Display(Name = "Motorista")]
        public int? MotoristaSelecionadoId { get; set; }

        [Display(Name = "Observações Administrativas")]
        [StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres")]
        public string? ObservacoesAdmin { get; set; }

        public List<VeiculoDisponivelViewModel> VeiculosDisponiveis { get; set; } = new();
        public List<Motorista> MotoristasDisponiveis { get; set; } = new();
    }

    public class VeiculoDisponivelViewModel
    {
        public int Id { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int CapacidadePassageiros { get; set; }
        public string DisplayText => $"{Modelo} - {Placa} (Cap: {CapacidadePassageiros})";
    }
}