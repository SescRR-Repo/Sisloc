using System.ComponentModel.DataAnnotations;

namespace Sisloc.Models.Enums
{
    public enum StatusAgendamento
    {
        [Display(Name = "Pendente")]
        Pendente = 1,
        [Display(Name = "Aprovado")]
        Aprovado = 2,
        [Display(Name = "Rejeitado")]
        Rejeitado = 3,
        [Display(Name = "Em Andamento")]
        EmAndamento = 4,
        [Display(Name = "Concluído")]
        Concluido = 5
    }
}