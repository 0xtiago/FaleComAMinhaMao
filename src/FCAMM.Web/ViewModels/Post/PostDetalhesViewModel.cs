using FCAMM.Core.Models;

namespace FCAMM.Web.ViewModels.Post;

public class PostDetalhesViewModel
{
    public PostModel Post { get; set; } = null!;
    public bool PodeEditar { get; set; }
    public bool PodeModerar { get; set; }
    public bool PodeExcluir { get; set; }
    public IEnumerable<PostModel> PostsRelacionados { get; set; } = new List<PostModel>();
}