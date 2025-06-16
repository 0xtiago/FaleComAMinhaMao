using FCAMM.Core.Data;
using FCAMM.Core.Enums;
using FCAMM.Core.Models;
using FCAMM.Web.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

[Route("admin/posts")]
[Authorize(Roles = "Admin,Moderador")]
public class PostAdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<UsuarioModel> _userManager;
    private readonly ILogger<PostAdminController> _logger;

    public PostAdminController(
        AppDbContext context,
        UserManager<UsuarioModel> userManager,
        ILogger<PostAdminController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region LISTAGEM E MODERAÇÃO

    // GET: /admin/posts
    [HttpGet]
    public async Task<IActionResult> Index(
        string? busca,
        StatusPostEnum? status,
        int? categoria,
        int? tag,
        string? autor,
        DateTime? dataInicio,
        DateTime? dataFim,
        string ordenacao = "data-desc",
        int pagina = 1)
    {
        const int itensPorPagina = 10;

        // Carregar estatísticas para o painel
        ViewBag.TotalPosts = await _context.Posts.CountAsync();
        ViewBag.PostsAprovados = await _context.Posts.CountAsync(p => p.Status == StatusPostEnum.Aprovado);
        ViewBag.PostsPendentes = await _context.Posts.CountAsync(p => p.Status == StatusPostEnum.AguardandoAprovacao);
        ViewBag.PostsRejeitados = await _context.Posts.CountAsync(p => p.Status == StatusPostEnum.Rejeitado);

        var query = _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .AsQueryable();

        // Se não há filtros aplicados, filtrar por "Aguardando Aprovação" por padrão
        if (string.IsNullOrEmpty(busca) && !status.HasValue && !categoria.HasValue && 
            string.IsNullOrEmpty(autor) && !dataInicio.HasValue && !dataFim.HasValue)
        {
            status = StatusPostEnum.AguardandoAprovacao;
        }

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(p => p.Titulo.Contains(busca) || 
                                   p.Resumo!.Contains(busca) || 
                                   p.Conteudo.Contains(busca));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (categoria.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoria.Value);
        }

        if (tag.HasValue)
        {
            query = query.Where(p => p.PostTags.Any(pt => pt.TagId == tag.Value));
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            query = query.Where(p => p.Autor.Nome.Contains(autor));
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(p => p.DataCriacao >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(p => p.DataCriacao <= dataFim.Value.AddDays(1));
        }

        // Aplicar ordenação
        query = ordenacao switch
        {
            "titulo-asc" => query.OrderBy(p => p.Titulo),
            "titulo-desc" => query.OrderByDescending(p => p.Titulo),
            "data-asc" => query.OrderBy(p => p.DataCriacao),
            "data-desc" => query.OrderByDescending(p => p.DataCriacao),
            "status-asc" => query.OrderBy(p => p.Status),
            "status-desc" => query.OrderByDescending(p => p.Status),
            "aguardando" => query.Where(p => p.Status == StatusPostEnum.AguardandoAprovacao).OrderBy(p => p.DataCriacao),
            _ => query.OrderByDescending(p => p.DataCriacao)
        };

        // Paginação
        var totalItens = await query.CountAsync();
        var posts = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        // Dados para filtros
        var categorias = await _context.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var tags = await _context.Tags
            .OrderBy(t => t.Nome)
            .ToListAsync();

        var viewModel = new ListarPostsViewModel
        {
            Posts = posts,
            Busca = busca,
            StatusFiltro = status,
            CategoriaFiltro = categoria,
            TagFiltro = tag,
            AutorFiltro = autor,
            DataInicioFiltro = dataInicio,
            DataFimFiltro = dataFim,
            OrdenacaoFiltro = ordenacao,
            PaginaAtual = pagina,
            TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina),
            TotalItens = totalItens,
            ItensPorPagina = itensPorPagina,
            Categorias = categorias,
            Tags = tags
        };

        return View("~/Views/Admin/Posts/Index.cshtml", viewModel);
    }

    #endregion

    #region DETALHES

    // GET: /admin/posts/detalhes/{id}
    [HttpGet("detalhes/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Include(p => p.AprovadoPor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var postsRelacionados = await _context.Posts
            .Include(p => p.Autor)
            .Where(p => p.Id != id && p.Status == StatusPostEnum.Aprovado)
            .Where(p => p.CategoriaId == post.CategoriaId || 
                       p.PostTags.Any(pt => post.PostTags.Select(x => x.TagId).Contains(pt.TagId)))
            .OrderByDescending(p => p.DataPublicacao)
            .Take(5)
            .ToListAsync();

        var viewModel = new PostDetalhesViewModel
        {
            Post = post,
            PodeEditar = true, // Admin/Moderador pode editar qualquer post
            PodeModerar = true,
            PodeExcluir = User.IsInRole("Admin"), // Apenas Admin pode excluir
            PostsRelacionados = postsRelacionados
        };

        return View("~/Views/Admin/Posts/Details.cshtml", viewModel);
    }

    #endregion

    #region MODERAR

    // GET: /admin/posts/moderar/{id}
    [HttpGet("moderar/{id:int}")]
    public async Task<IActionResult> Moderate(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ModerarPostViewModel
        {
            Id = post.Id,
            Titulo = post.Titulo,
            Slug = post.Slug,
            Resumo = post.Resumo,
            Conteudo = post.Conteudo,
            Status = post.Status,
            DataCriacao = post.DataCriacao,
            AutorNome = post.Autor.Nome,
            CategoriaNome = post.Categoria.Nome,
            NovoStatus = post.Status,
            ObservacoesAprovacao = post.ObservacoesAprovacao,
            DataPublicacao = post.DataPublicacao
        };

        return View("~/Views/Admin/Posts/Moderate.cshtml", model);
    }

    // POST: /admin/posts/moderar/{id}
    [HttpPost("moderar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int id, ModerarPostViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Dados inválidos.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Posts/Moderate.cshtml", model);
        }

        try
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Post não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            
            post.Status = model.NovoStatus;
            post.ObservacoesAprovacao = model.ObservacoesAprovacao;
            post.AprovadoPorId = userId;
            post.DataAprovacao = DateTime.UtcNow;
            
            if (model.NovoStatus == StatusPostEnum.Aprovado)
            {
                post.DataPublicacao = model.DataPublicacao ?? DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Post moderado: {Titulo} - Status: {Status} por {Usuario}", 
                post.Titulo, model.NovoStatus, User.Identity?.Name);
            
            TempData["Success"] = $"Post '{post.Titulo}' moderado com sucesso!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao moderar post: {Id}", id);
            ModelState.AddModelError(string.Empty, "Erro ao moderar post. Tente novamente.");
        }

        return View("~/Views/Admin/Posts/Moderate.cshtml", model);
    }

    #endregion

    #region EXCLUIR (Apenas Admin)

    // GET: /admin/posts/excluir/{id}
    [HttpGet("excluir/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
            .ThenInclude(pt => pt.TagModel)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Posts/Delete.cshtml", post);
    }

    // POST: /admin/posts/excluir/{id}
    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        try
        {
            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                TempData["Error"] = "Post não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Remover tags associadas
            _context.PostTags.RemoveRange(post.PostTags);
            
            // Remover post
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post excluído: {Titulo} por {Usuario}", post.Titulo, User.Identity?.Name);
            TempData["Success"] = $"Post '{post.Titulo}' excluído com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir post: {Id}", id);
            TempData["Error"] = "Erro ao excluir post. Tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region ESTATÍSTICAS

    // GET: /admin/posts/estatisticas
    [HttpGet("estatisticas")]
    public async Task<IActionResult> Statistics()
    {
        var hoje = DateTime.Today;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        var posts = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .ToListAsync();

        var viewModel = new EstatisticasPostViewModel
        {
            TotalPosts = posts.Count,
            PostsRascunho = posts.Count(p => p.Status == StatusPostEnum.Rascunho),
            PostsAguardandoAprovacao = posts.Count(p => p.Status == StatusPostEnum.AguardandoAprovacao),
            PostsAprovados = posts.Count(p => p.Status == StatusPostEnum.Aprovado),
            PostsRejeitados = posts.Count(p => p.Status == StatusPostEnum.Rejeitado),
            PostsArquivados = posts.Count(p => p.Status == StatusPostEnum.Arquivado),
            
            PostsHoje = posts.Count(p => p.DataCriacao.Date == hoje),
            PostsEstaSemana = posts.Count(p => p.DataCriacao >= inicioSemana),
            PostsEsteMes = posts.Count(p => p.DataCriacao >= inicioMes),
            
            TopCategorias = posts
                .GroupBy(p => p.Categoria.Nome)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value),
                
            TopTags = posts
                .SelectMany(p => p.PostTags.Select(pt => pt.TagModel.Nome))
                .GroupBy(nome => nome)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value),
                
            TopAutores = posts
                .GroupBy(p => p.Autor.Nome)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value)
        };

        return View("~/Views/Admin/Posts/Statistics.cshtml", viewModel);
    }

    #endregion

    #region AÇÕES RÁPIDAS

    // POST: /admin/posts/alterar-status/{id}
    [HttpPost("alterar-status/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, StatusPostEnum novoStatus)
    {
        try
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post não encontrado." });
            }

            var userId = _userManager.GetUserId(User);
            post.Status = novoStatus;
            post.AprovadoPorId = userId;
            post.DataAprovacao = DateTime.UtcNow;

            if (novoStatus == StatusPostEnum.Aprovado && !post.DataPublicacao.HasValue)
            {
                post.DataPublicacao = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Status do post alterado: {Titulo} - {Status} por {Usuario}", 
                post.Titulo, novoStatus, User.Identity?.Name);

            return Json(new { 
                success = true, 
                status = novoStatus.ToString(),
                message = $"Status alterado para {novoStatus} com sucesso!" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status do post: {Id}", id);
            return Json(new { success = false, message = "Erro ao alterar status do post." });
        }
    }

    // GET: /admin/posts/aguardando-aprovacao
    [HttpGet("aguardando-aprovacao")]
    public async Task<IActionResult> PendingApproval()
    {
        var postsAguardando = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.Status == StatusPostEnum.AguardandoAprovacao)
            .OrderBy(p => p.DataCriacao)
            .ToListAsync();

        var viewModel = new ListarPostsViewModel
        {
            Posts = postsAguardando,
            StatusFiltro = StatusPostEnum.AguardandoAprovacao,
            TotalItens = postsAguardando.Count,
            PaginaAtual = 1,
            TotalPaginas = 1,
            ItensPorPagina = postsAguardando.Count
        };

        return View("~/Views/Admin/Posts/PendingApproval.cshtml", viewModel);
    }

    // POST: /admin/posts/aprovar-lote
    [HttpPost("aprovar-lote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkApprove(int[] postIds)
    {
        try
        {
            if (postIds == null || !postIds.Any())
            {
                return Json(new { success = false, message = "Nenhum post selecionado." });
            }

            var posts = await _context.Posts
                .Where(p => postIds.Contains(p.Id))
                .ToListAsync();

            var userId = _userManager.GetUserId(User);
            var aprovados = 0;

            foreach (var post in posts)
            {
                if (post.Status == StatusPostEnum.AguardandoAprovacao)
                {
                    post.Status = StatusPostEnum.Aprovado;
                    post.AprovadoPorId = userId;
                    post.DataAprovacao = DateTime.UtcNow;
                    
                    if (!post.DataPublicacao.HasValue)
                    {
                        post.DataPublicacao = DateTime.UtcNow;
                    }
                    
                    aprovados++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Aprovação em lote: {Quantidade} posts aprovados por {Usuario}", 
                aprovados, User.Identity?.Name);

            return Json(new { 
                success = true, 
                message = $"{aprovados} posts aprovados com sucesso!" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na aprovação em lote");
            return Json(new { success = false, message = "Erro ao aprovar posts em lote." });
        }
    }

    #endregion
}