using System.ComponentModel.DataAnnotations;
using Sisloc.Models.Enums;

namespace Sisloc.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Placa")]
        [StringLength(8)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Modelo")]
        [StringLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Categoria")]
        public CategoriaVeiculo Categoria { get; set; }

        [Required]
        [Display(Name = "Capacidade de Passageiros")]
        [Range(1, 50, ErrorMessage = "A capacidade deve ser entre 1 e 50 passageiros")]
        public int CapacidadePassageiros { get; set; }

        [Display(Name = "Status")]
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        [Display(Name = "Observações")]
        [StringLength(500)]
        public string? Observacoes { get; set; }

        // Relacionamentos
        public virtual ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    }
}