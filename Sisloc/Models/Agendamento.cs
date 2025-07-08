using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sisloc.Models.Enums;

namespace Sisloc.Models
{
    public class Agendamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Protocolo")]
        [StringLength(20)]
        public string Protocolo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Data e Hora de Partida")]
        public DateTime DataPartida { get; set; }

        [Required]
        [Display(Name = "Data e Hora de Chegada")]
        public DateTime DataChegada { get; set; }

        [Required]
        [Display(Name = "Nome do Solicitante")]
        [StringLength(100)]
        public string NomeSolicitante { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantidade de Pessoas")]
        [Range(1, 50, ErrorMessage = "A quantidade deve ser entre 1 e 50 pessoas")]
        public int QuantidadePessoas { get; set; }

        [Required]
        [Display(Name = "Destino")]
        [StringLength(200)]
        public string Destino { get; set; } = string.Empty;

        [Display(Name = "Descrição/Objetivo")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required]
        [Display(Name = "Categoria do Veículo")]
        public CategoriaVeiculo CategoriaVeiculo { get; set; }

        [Display(Name = "Precisa de Motorista")]
        public bool PrecisaMotorista { get; set; }

        [Display(Name = "Status")]
        public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Relacionamentos
        [Display(Name = "Veículo Alocado")]
        public int? VeiculoAlocadoId { get; set; }
        [ForeignKey("VeiculoAlocadoId")]
        public virtual Veiculo? VeiculoAlocado { get; set; }

        [Display(Name = "Motorista Alocado")]
        public int? MotoristaAlocadoId { get; set; }
        [ForeignKey("MotoristaAlocadoId")]
        public virtual Motorista? MotoristaAlocado { get; set; }

        [Display(Name = "Observações Administrativas")]
        [StringLength(1000)]
        public string? ObservacoesAdmin { get; set; }
    }
}