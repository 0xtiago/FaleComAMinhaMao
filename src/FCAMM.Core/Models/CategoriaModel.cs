using System.ComponentModel.DataAnnotations;


namespace FCAMM.Core.Models;

public class CategoriaModel
{
    public int Id { get; set; }
        
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;
        
    [StringLength(500)]
    public string? Descricao { get; set; }
        
    [StringLength(150)]
    public string Slug { get; set; } = string.Empty;
        
    public bool Ativo { get; set; } = true;
        
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
    
    public virtual ICollection<PostModel> Posts { get; set; } = new List<PostModel>();
}