using FCAMM.Core.Models;

namespace FCAMM.Web.ViewModels.Home;

public class HomeViewModel
{
    public IEnumerable<PostModel> PostsRecentes { get; set; } = new List<PostModel>();
    public IEnumerable<CategoriaComContagem> Categorias { get; set; } = new List<CategoriaComContagem>();
    public IEnumerable<TagModel> TagsPopulares { get; set; } = new List<TagModel>();
    public IEnumerable<AutorComContagem> AutoresDestaque { get; set; } = new List<AutorComContagem>();
}

public class CategoriaComContagem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostsCount { get; set; }
}

public class AutorComContagem
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int PostsCount { get; set; }
}