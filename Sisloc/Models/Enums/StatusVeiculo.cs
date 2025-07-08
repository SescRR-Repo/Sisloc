using System.ComponentModel.DataAnnotations;

namespace Sisloc.Models.Enums
{
    public enum StatusVeiculo
    {
        [Display(Name = "Disponível")]
        Disponivel = 1,
        [Display(Name = "Reservado")]
        Reservado = 2,
        [Display(Name = "Em Uso")]
        EmUso = 3,
        [Display(Name = "Manutenção")]
        Manutencao = 4
    }
}