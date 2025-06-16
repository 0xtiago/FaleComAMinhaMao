namespace FCAMM.Web.ViewModels.Post;

public class EstatisticasPostViewModel
{
    public int TotalPosts { get; set; }
    public int PostsRascunho { get; set; }
    public int PostsAguardandoAprovacao { get; set; }
    public int PostsAprovados { get; set; }
    public int PostsRejeitados { get; set; }
    public int PostsArquivados { get; set; }
    
    // Estatísticas por período
    public int PostsHoje { get; set; }
    public int PostsEstaSemana { get; set; }
    public int PostsEsteMes { get; set; }
    
    // Top categorias e tags
    public Dictionary<string, int> TopCategorias { get; set; } = new();
    public Dictionary<string, int> TopTags { get; set; } = new();
    
    // Autores mais ativos
    public Dictionary<string, int> TopAutores { get; set; } = new();
}