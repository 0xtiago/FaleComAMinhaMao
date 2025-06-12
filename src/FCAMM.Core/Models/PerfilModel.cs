using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCAMM.Core.Models;

public class PerfilModel
{
    [Key]
    public int Id { get; set; }
        
    [StringLength(200)]
    public string? Website { get; set; }
        
    [StringLength(100)]
    public string? Twitter { get; set; }
        
    [StringLength(100)]
    public string? LinkedIn { get; set; }
        
    [StringLength(1000)]
    public string? Sobre { get; set; }
        
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        
    
    [ForeignKey("Usuario")]
    public string UsuarioId { get; set; } = string.Empty;
    public virtual UsuarioModel UsuarioModel { get; set; } = null!;
}