// ViewModels/VeiculoEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para edição de veículos.
    /// </summary>
    public class VeiculoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória")]
        [Display(Name = "Placa")]
        [StringLength(8, ErrorMessage = "A placa deve ter no máximo 8 caracteres")]
        [RegularExpression(@"^[A-Z]{3}-?\d{4}$|^[A-Z]{3}-?\d[A-Z]\d{2}$",
            ErrorMessage = "Formato de placa inválido. Use ABC-1234 ou ABC-1D23")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório")]
        [Display(Name = "Modelo")]
        [StringLength(50, ErrorMessage = "O modelo deve ter no máximo 50 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione a categoria")]
        [Display(Name = "Categoria")]
        public CategoriaVeiculo Categoria { get; set; }

        [Required(ErrorMessage = "A capacidade é obrigatória")]
        [Display(Name = "Capacidade de Passageiros")]
        [Range(1, 50, ErrorMessage = "A capacidade deve ser entre 1 e 50 passageiros")]
        public int CapacidadePassageiros { get; set; }

        [Display(Name = "Status")]
        public StatusVeiculo Status { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        // Propriedades auxiliares para exibição
        public bool TemAgendamentosAtivos { get; set; }
        public int QuantidadeAgendamentos { get; set; }
        public DateTime? ProximoAgendamento { get; set; }
    }
}