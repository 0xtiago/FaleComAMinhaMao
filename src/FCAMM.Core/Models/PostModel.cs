using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FCAMM.Core.Enums;

namespace FCAMM.Core.Models;

public class PostModel
{
    public int Id { get; set; }
        
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;
        
    [StringLength(250)]
    public string Slug { get; set; } = string.Empty;
        
    [StringLength(300)]
    public string? Resumo { get; set; }
        
    [Required]
    [Column(TypeName = "text")]
    public string Conteudo { get; set; } = string.Empty;
        
    public StatusPostEnum Status { get; set; } = StatusPostEnum.Rascunho;
        
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataPublicacao { get; set; }
        
    // Relacionamento N:1 com Usuario (Autor)
    [ForeignKey("Autor")]
    public string AutorId { get; set; } = string.Empty;
    public virtual UsuarioModel Autor { get; set; } = null!;
        
    // Relacionamento N:1 com Usuario (Aprovador) - NOVO
    [ForeignKey("AprovadoPor")]
    public string? AprovadoPorId { get; set; }
    public virtual UsuarioModel? AprovadoPor { get; set; }
    
    // Data e observações da aprovação - NOVO
    public DateTime? DataAprovacao { get; set; }
    
    [StringLength(500)]
    public string? ObservacoesAprovacao { get; set; }
    
    // Relacionamento N:1 com Categoria
    [ForeignKey("Categoria")]
    public int CategoriaId { get; set; }
    public virtual CategoriaModel Categoria { get; set; } = null!;
        
    // Relacionamento N:N com Tags
    public virtual ICollection<PostTagModel> PostTags { get; set; } = new List<PostTagModel>();
}