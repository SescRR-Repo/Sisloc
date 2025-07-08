using System.ComponentModel.DataAnnotations;

namespace Sisloc.Models.Enums
{
    public enum StatusMotorista
    {
        [Display(Name = "Disponível")]
        Disponivel = 1,
        [Display(Name = "Ocupado")]
        Ocupado = 2,
        [Display(Name = "Folga")]
        Folga = 3,
        [Display(Name = "Irregular")]
        Irregular = 4
    }
}