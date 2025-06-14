using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Tag;

public class CriarTagViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(50, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Nome da Tag")]
    public string Nome { get; set; } = string.Empty;
}