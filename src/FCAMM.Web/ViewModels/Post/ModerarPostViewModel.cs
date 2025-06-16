using System.ComponentModel.DataAnnotations;
using FCAMM.Core.Enums;

namespace FCAMM.Web.ViewModels.Post;

public class ModerarPostViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public StatusPostEnum Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string AutorNome { get; set; } = string.Empty;
    public string CategoriaNome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status é obrigatório")]
    [Display(Name = "Novo Status")]
    public StatusPostEnum NovoStatus { get; set; }

    [StringLength(500, ErrorMessage = "As observações devem ter no máximo {1} caracteres")]
    [Display(Name = "Observações da Moderação")]
    public string? ObservacoesAprovacao { get; set; }

    [Display(Name = "Data de Publicação")]
    public DateTime? DataPublicacao { get; set; }
}