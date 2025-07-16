// ViewModels/MotoristaCreateViewModel.cs
using System.ComponentModel.DataAnnotations;
using Sisloc.Models.Enums;

namespace Sisloc.ViewModels
{
    /// <summary>
    /// ViewModel para criação de motoristas.
    /// </summary>
    public class MotoristaCreateViewModel
    {
        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número da CNH é obrigatório")]
        [Display(Name = "Número da CNH")]
        [StringLength(20, ErrorMessage = "O número da CNH deve ter no máximo 20 caracteres")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CNH deve conter exatamente 11 dígitos")]
        public string NumeroCnh { get; set; } = string.Empty;

        [Required(ErrorMessage = "O vencimento da CNH é obrigatório")]
        [Display(Name = "Vencimento da CNH")]
        [DataType(DataType.Date)]
        public DateTime VencimentoCnh { get; set; } = DateTime.Now.AddYears(5);

        [Required(ErrorMessage = "A categoria da CNH é obrigatória")]
        [Display(Name = "Categoria da CNH")]
        [StringLength(5, ErrorMessage = "A categoria deve ter no máximo 5 caracteres")]
        public string CategoriaCnh { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Display(Name = "Telefone")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data do exame toxicológico é obrigatória")]
        [Display(Name = "Data do Exame Toxicológico")]
        [DataType(DataType.Date)]
        public DateTime DataExameToxicologico { get; set; } = DateTime.Now.AddMonths(-6);

        [Display(Name = "Status")]
        public StatusMotorista Status { get; set; } = StatusMotorista.Disponivel;

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        // Propriedades auxiliares para validação
        public int DiasParaVencimentoCnh => (VencimentoCnh - DateTime.Now).Days;
        public int DiasParaVencimentoExameTox => (DataExameToxicologico.AddYears(2) - DateTime.Now).Days;
        public bool TemAlertaVencimento => DiasParaVencimentoCnh <= 30 || DiasParaVencimentoExameTox <= 60;

        public string GetAlertaVencimento()
        {
            if (DiasParaVencimentoCnh < 0)
                return "CNH vencida!";
            if (DiasParaVencimentoExameTox < 0)
                return "Exame toxicológico vencido!";
            if (DiasParaVencimentoCnh <= 30)
                return $"CNH vence em {DiasParaVencimentoCnh} dias";
            if (DiasParaVencimentoExameTox <= 60)
                return $"Exame toxicológico vence em {DiasParaVencimentoExameTox} dias";
            return "";
        }

        public string GetClasseAlerta()
        {
            if (DiasParaVencimentoCnh < 0 || DiasParaVencimentoExameTox < 0)
                return "danger";
            if (DiasParaVencimentoCnh <= 30 || DiasParaVencimentoExameTox <= 60)
                return "warning";
            return "success";
        }
    }
}