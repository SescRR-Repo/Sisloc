using System.ComponentModel.DataAnnotations;

namespace Sisloc.ViewModels
{
    public class ConsultaViewModel
    {
        [Required(ErrorMessage = "O protocolo é obrigatório")]
        [Display(Name = "Número do Protocolo")]
        [StringLength(20, ErrorMessage = "Protocolo inválido")]
        public string Protocolo { get; set; } = string.Empty;
    }
}