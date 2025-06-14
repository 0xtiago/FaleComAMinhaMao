using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Conta;

public class EditarPerfilViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Biografia")]
    public string? Bio { get; set; }

    [StringLength(200, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Website")]
    [Url(ErrorMessage = "Digite uma URL válida (ex: https://exemplo.com)")]
    public string? Website { get; set; }

    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Twitter")]
    [RegularExpression(@"^@?[A-Za-z0-9_]{1,15}$", ErrorMessage = "Digite um usuário Twitter válido (ex: @usuario)")]
    public string? Twitter { get; set; }

    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "LinkedIn")]
    public string? LinkedIn { get; set; }

    [StringLength(1000, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Sobre Mim")]
    public string? Sobre { get; set; }

    [StringLength(100, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Especialidade")]
    public string? Especialidade { get; set; }

    // Campos apenas para exibição (não editáveis)
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Total de Textos")]
    public int TotalTextos { get; set; } = 0;

    [Display(Name = "Total de Visualizações")]
    public int TotalVisualizacoes { get; set; } = 0;
}