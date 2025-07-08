using System.ComponentModel.DataAnnotations;
using Sisloc.Models.Enums;

namespace Sisloc.Models
{
    public class Motorista
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome Completo")]
        [StringLength(100)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Número da CNH")]
        [StringLength(20)]
        public string NumeroCnh { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vencimento da CNH")]
        [DataType(DataType.Date)]
        public DateTime VencimentoCnh { get; set; }

        [Required]
        [Display(Name = "Categoria da CNH")]
        [StringLength(5)]
        public string CategoriaCnh { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Telefone")]
        [StringLength(15)]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Data do Exame Toxicológico")]
        [DataType(DataType.Date)]
        public DateTime DataExameToxicologico { get; set; }

        [Display(Name = "Status")]
        public StatusMotorista Status { get; set; } = StatusMotorista.Disponivel;

        [Display(Name = "Observações")]
        [StringLength(500)]
        public string? Observacoes { get; set; }

        // Relacionamentos
        public virtual ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    }
}