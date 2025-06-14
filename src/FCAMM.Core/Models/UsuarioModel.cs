using System.ComponentModel.DataAnnotations;
using FCAMM.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace FCAMM.Core.Models;

public class UsuarioModel : IdentityUser
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = string.Empty;
        
    public RolesEnum Role { get; set; } = RolesEnum.Autor;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
        
    // Relacionamento 1:1 com PerfilModel
    public virtual PerfilModel? Perfil { get; set; }
        
    // Relacionamentos com Posts
    public virtual ICollection<PostModel> Posts { get; set; } = new List<PostModel>();
    public virtual ICollection<PostModel> PostsAprovados { get; set; } = new List<PostModel>();
}