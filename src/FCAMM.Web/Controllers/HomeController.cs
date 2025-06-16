using FCAMM.Core.Data;
using FCAMM.Core.Enums;
using FCAMM.Core.Models;
using FCAMM.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Posts recentes aprovados (últimos 10)
        var postsRecentes = await _context.Posts
            .Include(p => p.Categoria)
            .Where(p => p.Status == StatusPostEnum.Aprovado)
            .OrderByDescending(p => p.DataAprovacao)
            .Take(10)
            .ToListAsync();

        // Categorias com contagem de posts aprovados
        var categorias = await _context.Categorias
            .Where(c => c.Ativo)
            .Select(c => new CategoriaComContagem
            {
                Id = c.Id,
                Nome = c.Nome,
                Slug = c.Slug,
                PostsCount = c.Posts.Count(p => p.Status == StatusPostEnum.Aprovado)
            })
            .Where(c => c.PostsCount > 0)
            .OrderByDescending(c => c.PostsCount)
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            PostsRecentes = postsRecentes,
            Categorias = categorias,
            TagsPopulares = new List<TagModel>(),
            AutoresDestaque = new List<AutorComContagem>()
        };

        return View(viewModel);
    }

    // GET: /post/{slug} - REMOVIDO: agora é responsabilidade do PostsController
    // Esta rota foi movida para PostsController.Post()

    // GET: /categoria/{slug}
    [HttpGet("categoria/{slug}")]
    public async Task<IActionResult> Categoria(string slug, int pagina = 1)
    {
        const int itensPorPagina = 6;

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Ativo);

        if (categoria == null)
        {
            return NotFound();
        }

        var query = _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.CategoriaId == categoria.Id && p.Status == StatusPostEnum.Aprovado)
            .OrderByDescending(p => p.DataPublicacao);

        var totalItens = await query.CountAsync();
        var posts = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        ViewBag.Categoria = categoria;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
        ViewBag.TotalItens = totalItens;

        return View("CategoriaPost", posts);
    }

    // GET: /tag/{slug}
    [HttpGet("tag/{slug}")]
    public async Task<IActionResult> Tag(string slug, int pagina = 1)
    {
        const int itensPorPagina = 6;

        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Slug == slug);

        if (tag == null)
        {
            return NotFound();
        }

        var query = _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.PostTags.Any(pt => pt.TagId == tag.Id) && p.Status == StatusPostEnum.Aprovado)
            .OrderByDescending(p => p.DataPublicacao);

        var totalItens = await query.CountAsync();
        var posts = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        ViewBag.Tag = tag;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
        ViewBag.TotalItens = totalItens;

        return View("TagPost", posts);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}