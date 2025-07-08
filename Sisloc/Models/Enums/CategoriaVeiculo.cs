using System.ComponentModel.DataAnnotations;

namespace Sisloc.Models.Enums
{
    public enum CategoriaVeiculo
    {
        [Display(Name = "Hatch")]
        Hatch = 1,
        [Display(Name = "Sedan")]
        Sedan = 2,
        [Display(Name = "Caminhonete")]
        Caminhonete = 3,
        [Display(Name = "Pickup")]
        Pickup = 4,
        [Display(Name = "Caminhão")]
        Caminhao = 5
    }
}