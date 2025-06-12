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
        
    
    [ForeignKey("Categoria")]
    public int CategoriaId { get; set; }
    public virtual CategoriaModel Categoria { get; set; } = null!;
        
    
    public virtual ICollection<PostTagModel> PostTags { get; set; } = new List<PostTagModel>();
}