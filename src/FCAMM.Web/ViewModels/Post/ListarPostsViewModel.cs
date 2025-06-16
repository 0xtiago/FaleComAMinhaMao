using FCAMM.Core.Enums;
using FCAMM.Core.Models;

namespace FCAMM.Web.ViewModels.Post;

public class ListarPostsViewModel
{
    public IEnumerable<PostModel> Posts { get; set; } = new List<PostModel>();
    public string? Busca { get; set; }
    public StatusPostEnum? StatusFiltro { get; set; }
    public int? CategoriaFiltro { get; set; }
    public int? TagFiltro { get; set; }
    public string? AutorFiltro { get; set; }
    public DateTime? DataInicioFiltro { get; set; }
    public DateTime? DataFimFiltro { get; set; }
    public string OrdenacaoFiltro { get; set; } = "data-desc";
    
    // Paginação
    public int PaginaAtual { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
    public int ItensPorPagina { get; set; } = 10;
    
    // Dados para filtros
    public IEnumerable<CategoriaModel> Categorias { get; set; } = new List<CategoriaModel>();
    public IEnumerable<TagModel> Tags { get; set; } = new List<TagModel>();
}