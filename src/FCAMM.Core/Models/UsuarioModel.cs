using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FCAMM.Core.Models;

public class UsuarioModel : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;
        
    [StringLength(500)]
    public string? Bio { get; set; }
        
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
    public bool Ativo { get; set; } = true;
    
    public virtual PerfilModel? Perfil { get; set; }
    
    public virtual ICollection<PostModel> Posts { get; set; } = new List<PostModel>();
}