using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Categoria;

public class EditarCategoriaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Nome da Categoria")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Required]
    [StringLength(150, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Slug")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "O slug deve conter apenas letras minúsculas, números e hífens")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;
}