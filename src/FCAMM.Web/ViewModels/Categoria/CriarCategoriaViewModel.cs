using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Categoria;

public class CriarCategoriaViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Nome da Categoria")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;
}

