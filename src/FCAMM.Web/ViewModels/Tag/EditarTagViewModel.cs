using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Tag;

public class EditarTagViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(50, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Nome da Tag")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Slug")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "O slug deve conter apenas letras minúsculas, números e hífens")]
    public string Slug { get; set; } = string.Empty;
}