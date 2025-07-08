using System.ComponentModel.DataAnnotations;
using Sisloc.Models;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    public class AgendamentoCreateViewModel
    {
        [Required(ErrorMessage = "A data de partida é obrigatória")]
        [Display(Name = "Data e Hora de Partida")]
        public DateTime DataPartida { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "A data de chegada é obrigatória")]
        [Display(Name = "Data e Hora de Chegada")]
        public DateTime DataChegada { get; set; } = DateTime.Now.AddDays(1).AddHours(4);

        [Required(ErrorMessage = "O nome do solicitante é obrigatório")]
        [Display(Name = "Nome do Solicitante")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string NomeSolicitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade de pessoas é obrigatória")]
        [Display(Name = "Quantidade de Pessoas")]
        [Range(1, 50, ErrorMessage = "A quantidade deve ser entre 1 e 50 pessoas")]
        public int QuantidadePessoas { get; set; } = 1;

        [Required(ErrorMessage = "O destino é obrigatório")]
        [Display(Name = "Destino")]
        [StringLength(200, ErrorMessage = "O destino deve ter no máximo 200 caracteres")]
        public string Destino { get; set; } = string.Empty;

        [Display(Name = "Descrição/Objetivo da viagem")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Selecione a categoria do veículo")]
        [Display(Name = "Categoria do Veículo")]
        public CategoriaVeiculo CategoriaVeiculo { get; set; }

        [Display(Name = "Precisa de motorista?")]
        public bool PrecisaMotorista { get; set; }
    }
}