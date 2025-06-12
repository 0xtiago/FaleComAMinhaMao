using System.ComponentModel.DataAnnotations.Schema;

namespace FCAMM.Core.Models;

public class PostTagModel
{
    [ForeignKey("Post")]
    public int PostId { get; set; }
    public virtual PostModel PostModel { get; set; } = null!;
        
    [ForeignKey("Tag")]
    public int TagId { get; set; }
    public virtual TagModel TagModel { get; set; } = null!;
}