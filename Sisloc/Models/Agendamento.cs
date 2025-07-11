// Models/Agendamento.cs
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

        [Required(ErrorMessage = "A data de partida é obrigatória")]
        [Display(Name = "Data e Hora de Partida")]
        public DateTime DataPartida { get; set; }

        [Required(ErrorMessage = "A data de chegada é obrigatória")]
        [Display(Name = "Data e Hora de Chegada")]
        public DateTime DataChegada { get; set; }

        [Required(ErrorMessage = "O nome do solicitante é obrigatório")]
        [Display(Name = "Nome do Solicitante")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string NomeSolicitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade de pessoas é obrigatória")]
        [Display(Name = "Quantidade de Pessoas")]
        [Range(1, 50, ErrorMessage = "A quantidade deve ser entre 1 e 50 pessoas")]
        public int QuantidadePessoas { get; set; }

        [Required(ErrorMessage = "O destino é obrigatório")]
        [Display(Name = "Destino")]
        [StringLength(200, ErrorMessage = "O destino deve ter no máximo 200 caracteres")]
        public string Destino { get; set; } = string.Empty;

        [Display(Name = "Descrição/Objetivo")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Selecione a categoria do veículo")]
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