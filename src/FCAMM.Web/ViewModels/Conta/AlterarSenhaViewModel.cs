using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Conta;

public class AlterarSenhaViewModel
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha Atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} e no máximo {1} caracteres", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Nova Senha")]
    [Compare("NovaSenha", ErrorMessage = "A nova senha e confirmação não coincidem")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}