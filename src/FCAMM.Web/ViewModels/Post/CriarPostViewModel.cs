using System.ComponentModel.DataAnnotations;
using FCAMM.Core.Enums;

namespace FCAMM.Web.ViewModels.Post;

public class CriarPostViewModel
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "O {0} deve ter no máximo {1} caracteres")]
    [Display(Name = "Resumo")]
    public string? Resumo { get; set; }

    [Required(ErrorMessage = "Conteúdo é obrigatório")]
    [Display(Name = "Conteúdo")]
    public string Conteudo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Categoria é obrigatória")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    [Display(Name = "Tags")]
    public List<int> TagIds { get; set; } = new();

    [Display(Name = "Status")]
    public StatusPostEnum Status { get; set; } = StatusPostEnum.Rascunho;

    [Display(Name = "Data de Publicação")]
    public DateTime? DataPublicacao { get; set; }
}