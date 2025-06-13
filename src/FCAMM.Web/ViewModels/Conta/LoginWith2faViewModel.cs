using System.ComponentModel.DataAnnotations;

namespace FCAMM.Web.ViewModels.Conta;

public class LoginWith2faViewModel
{
    [Required]
    [StringLength(7, ErrorMessage = "O {0} deve ter no mínimo {2} e no máximo {1} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Código do Authenticator")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Lembrar desta máquina")]
    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }
}