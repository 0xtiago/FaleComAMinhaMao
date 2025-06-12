using System.ComponentModel.DataAnnotations;

namespace FCAMM.Core.Models;

public class TagModel
{
    public int Id { get; set; }
        
    [Required]
    [StringLength(50)]
    public string Nome { get; set; } = string.Empty;
        
    [StringLength(100)]
    public string Slug { get; set; } = string.Empty;
        
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
    
    public virtual ICollection<PostTagModel> PostTags { get; set; } = new List<PostTagModel>();
}