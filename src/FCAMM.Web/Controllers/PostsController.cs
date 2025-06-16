using FCAMM.Core.Data;
using FCAMM.Core.Enums;
using FCAMM.Core.Models;
using FCAMM.Core.Services;
using FCAMM.Web.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

[Route("posts")]
public class PostsController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISluggerService _sluggerService;
    private readonly UserManager<UsuarioModel> _userManager;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        AppDbContext context,
        ISluggerService sluggerService,
        UserManager<UsuarioModel> userManager,
        ILogger<PostsController> logger)
    {
        _context = context;
        _sluggerService = sluggerService;
        _userManager = userManager;
        _logger = logger;
    }

    #region LISTA PÚBLICA DE POSTS

    // GET: /posts (Lista pública - todos os posts aprovados)
    [HttpGet]
    public async Task<IActionResult> Index(
        string? busca,
        int? categoria,
        string ordenacao = "data-desc",
        int pagina = 1)
    {
        const int itensPorPagina = 10;

        var query = _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Where(p => p.Status == StatusPostEnum.Aprovado) // Apenas posts aprovados
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(p => p.Titulo.Contains(busca) || 
                                   p.Resumo!.Contains(busca) || 
                                   p.Conteudo.Contains(busca));
        }

        if (categoria.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoria.Value);
        }

        // Aplicar ordenação
        query = ordenacao switch
        {
            "titulo-asc" => query.OrderBy(p => p.Titulo),
            "data-asc" => query.OrderBy(p => p.DataAprovacao),
            "data-desc" => query.OrderByDescending(p => p.DataAprovacao),
            _ => query.OrderByDescending(p => p.DataAprovacao)
        };

        // Paginação
        var totalItens = await query.CountAsync();
        var posts = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        // Dados para filtros
        var categorias = await _context.Categorias
            .Where(c => c.Ativo && c.Posts.Any(p => p.Status == StatusPostEnum.Aprovado))
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var viewModel = new ListarPostsViewModel
        {
            Posts = posts,
            Busca = busca,
            CategoriaFiltro = categoria,
            OrdenacaoFiltro = ordenacao,
            PaginaAtual = pagina,
            TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina),
            TotalItens = totalItens,
            ItensPorPagina = itensPorPagina,
            Categorias = categorias,
            Tags = new List<TagModel>()
        };

        return View(viewModel); // View padrão: Views/Posts/Index.cshtml
    }

    #endregion

    #region MEUS POSTS (ÁREA DO AUTOR)

    // GET: /posts/meus
    [HttpGet("meus")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> MeusPosts(
        string? busca,
        StatusPostEnum? status,
        int? categoria,
        int? tag,
        DateTime? dataInicio,
        DateTime? dataFim,
        string ordenacao = "data-desc",
        int pagina = 1)
    {
        const int itensPorPagina = 10;
        var userId = _userManager.GetUserId(User);

        var query = _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.AutorId == userId) // Apenas posts do usuário logado
            .AsQueryable();

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

        return View("MeusPosts", viewModel); // Views/Posts/MeusPosts.cshtml
    }

    #endregion

    #region CRIAR

    // GET: /posts/criar
    [HttpGet("criar")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Create()
    {
        await CarregarDadosFormularioAsync();
        return View(new CriarPostViewModel()); // Views/Posts/Create.cshtml
    }

    // POST: /posts/criar
    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Create(CriarPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CarregarDadosFormularioAsync();
            return View(model); // Views/Posts/Create.cshtml
        }

        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                await CarregarDadosFormularioAsync();
                return View(model); // Views/Posts/Create.cshtml
            }

            // Verificar se o usuário pode criar com o status selecionado
            if (!PodeUsarStatus(model.Status))
            {
                model.Status = StatusPostEnum.Rascunho;
            }

            // Gerar slug único
            var slug = await GenerateUniqueSlugAsync(model.Titulo);

            var post = new PostModel
            {
                Titulo = model.Titulo,
                Slug = slug,
                Resumo = model.Resumo,
                Conteudo = model.Conteudo,
                CategoriaId = model.CategoriaId,
                Status = model.Status,
                DataPublicacao = model.DataPublicacao,
                AutorId = userId,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Adicionar tags
            if (model.TagIds.Any())
            {
                var postTags = model.TagIds.Select(tagId => new PostTagModel
                {
                    PostId = post.Id,
                    TagId = tagId
                }).ToList();

                _context.PostTags.AddRange(postTags);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Post criado: {Titulo} por {Usuario}", model.Titulo, User.Identity?.Name);
            
            var mensagem = model.Status == StatusPostEnum.AguardandoAprovacao 
                ? $"Post '{model.Titulo}' enviado para aprovação!"
                : $"Post '{model.Titulo}' salvo como rascunho!";
            
            TempData["Success"] = mensagem;

            return RedirectToAction(nameof(MeusPosts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar post: {Titulo}", model.Titulo);
            ModelState.AddModelError(string.Empty, "Erro ao criar post. Tente novamente.");
            await CarregarDadosFormularioAsync();
        }

        return View("~/Views/Posts/Create.cshtml", model);
    }

    #endregion

    #region EDITAR

    // GET: /posts/editar/{id}
    [HttpGet("editar/{id:int}")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        var post = await _context.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado ou você não tem permissão para editá-lo.";
            return RedirectToAction(nameof(MeusPosts));
        }

        var model = new EditarPostViewModel
        {
            Id = post.Id,
            Titulo = post.Titulo,
            Slug = post.Slug,
            Resumo = post.Resumo,
            Conteudo = post.Conteudo,
            CategoriaId = post.CategoriaId,
            TagIds = post.PostTags.Select(pt => pt.TagId).ToList(),
            Status = post.Status,
            DataPublicacao = post.DataPublicacao,
            DataCriacao = post.DataCriacao,
            AutorId = post.AutorId
        };

        await CarregarDadosFormularioAsync();
        return View(model); // Views/Posts/Edit.cshtml
    }

    // POST: /posts/editar/{id}
    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Edit(int id, EditarPostViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Dados inválidos.";
            return RedirectToAction(nameof(MeusPosts));
        }

        if (!ModelState.IsValid)
        {
            await CarregarDadosFormularioAsync();
            return View(model); // Views/Posts/Edit.cshtml
        }

        try
        {
            var userId = _userManager.GetUserId(User);
            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

            if (post == null)
            {
                TempData["Error"] = "Post não encontrado ou você não tem permissão para editá-lo.";
                return RedirectToAction(nameof(MeusPosts));
            }

            // Verificar se pode alterar o status
            if (!PodeAlterarStatus(post.Status, model.Status))
            {
                model.Status = post.Status; // Manter status original
            }

            // Se o título mudou, gerar novo slug
            if (post.Titulo != model.Titulo)
            {
                post.Slug = await GenerateUniqueSlugAsync(model.Titulo, id);
            }

            post.Titulo = model.Titulo;
            post.Resumo = model.Resumo;
            post.Conteudo = model.Conteudo;
            post.CategoriaId = model.CategoriaId;
            post.Status = model.Status;
            post.DataPublicacao = model.DataPublicacao;
            post.DataAtualizacao = DateTime.UtcNow;

            // Atualizar tags
            _context.PostTags.RemoveRange(post.PostTags);
            if (model.TagIds.Any())
            {
                var novasPostTags = model.TagIds.Select(tagId => new PostTagModel
                {
                    PostId = post.Id,
                    TagId = tagId
                }).ToList();

                _context.PostTags.AddRange(novasPostTags);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Post editado: {Titulo} por {Usuario}", model.Titulo, User.Identity?.Name);
            TempData["Success"] = $"Post '{model.Titulo}' atualizado com sucesso!";

            return RedirectToAction(nameof(MeusPosts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao editar post: {Id}", id);
            ModelState.AddModelError(string.Empty, "Erro ao atualizar post. Tente novamente.");
            await CarregarDadosFormularioAsync();
        }

        return View("~/Views/Posts/Edit.cshtml", model);
    }

    #endregion

    #region DETALHES

    // GET: /posts/detalhes/{id}
    [HttpGet("detalhes/{id:int}")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Include(p => p.AprovadoPor)
            .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado ou você não tem permissão para visualizá-lo.";
            return RedirectToAction(nameof(MeusPosts));
        }

        var postsRelacionados = await _context.Posts
            .Include(p => p.Autor)
            .Where(p => p.Id != id && p.AutorId == userId)
            .Where(p => p.CategoriaId == post.CategoriaId || 
                       p.PostTags.Any(pt => post.PostTags.Select(x => x.TagId).Contains(pt.TagId)))
            .OrderByDescending(p => p.DataCriacao)
            .Take(5)
            .ToListAsync();

        var viewModel = new PostDetalhesViewModel
        {
            Post = post,
            PodeEditar = PodeEditarPost(post),
            PodeModerar = false, // Autores não podem moderar
            PodeExcluir = PodeExcluirPost(post),
            PostsRelacionados = postsRelacionados
        };

        return View(viewModel); // Views/Posts/Details.cshtml
    }

    #endregion

    #region VISUALIZAÇÃO PÚBLICA DE POST

    // GET: /posts/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == StatusPostEnum.Aprovado);

        if (post == null)
        {
            return NotFound();
        }

        // Posts relacionados (mesma categoria ou tags similares)
        var postsRelacionados = await _context.Posts
            .Include(p => p.Autor)
            .Where(p => p.Id != post.Id && p.Status == StatusPostEnum.Aprovado)
            .Where(p => p.CategoriaId == post.CategoriaId || 
                       p.PostTags.Any(pt => post.PostTags.Select(x => x.TagId).Contains(pt.TagId)))
            .OrderByDescending(p => p.DataAprovacao)
            .Take(4)
            .ToListAsync();

        ViewBag.PostsRelacionados = postsRelacionados;

        return View("Post", post); // Views/Posts/Post.cshtml
    }

    #endregion

    #region EXCLUIR

    // GET: /posts/excluir/{id}
    [HttpGet("excluir/{id:int}")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var post = await _context.Posts
            .Include(p => p.Autor)
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
            .ThenInclude(pt => pt.TagModel)
            .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

        if (post == null)
        {
            TempData["Error"] = "Post não encontrado ou você não tem permissão para excluí-lo.";
            return RedirectToAction(nameof(MeusPosts));
        }

        if (!PodeExcluirPost(post))
        {
            TempData["Error"] = "Você só pode excluir posts em rascunho.";
            return RedirectToAction(nameof(MeusPosts));
        }

        return View(post); // Views/Posts/Delete.cshtml
    }

    // POST: /posts/excluir/{id}
    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

            if (post == null)
            {
                TempData["Error"] = "Post não encontrado ou você não tem permissão para excluí-lo.";
                return RedirectToAction(nameof(MeusPosts));
            }

            if (!PodeExcluirPost(post))
            {
                TempData["Error"] = "Você só pode excluir posts em rascunho.";
                return RedirectToAction(nameof(MeusPosts));
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

        return RedirectToAction(nameof(MeusPosts));
    }

    #endregion

    #region AÇÕES ESPECIAIS

    // GET: /posts/rascunhos
    [HttpGet("rascunhos")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Drafts()
    {
        var userId = _userManager.GetUserId(User);
        var rascunhos = await _context.Posts
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.AutorId == userId && p.Status == StatusPostEnum.Rascunho)
            .OrderByDescending(p => p.DataAtualizacao)
            .ToListAsync();

        var viewModel = new ListarPostsViewModel
        {
            Posts = rascunhos,
            StatusFiltro = StatusPostEnum.Rascunho,
            TotalItens = rascunhos.Count,
            PaginaAtual = 1,
            TotalPaginas = 1,
            ItensPorPagina = rascunhos.Count
        };

        return View(viewModel); // Views/Posts/Drafts.cshtml
    }

    // GET: /posts/publicados
    [HttpGet("publicados")]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Published()
    {
        var userId = _userManager.GetUserId(User);
        var publicados = await _context.Posts
            .Include(p => p.Categoria)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.TagModel)
            .Where(p => p.AutorId == userId && p.Status == StatusPostEnum.Aprovado)
            .OrderByDescending(p => p.DataAprovacao)
            .ToListAsync();

        var viewModel = new ListarPostsViewModel
        {
            Posts = publicados,
            StatusFiltro = StatusPostEnum.Aprovado,
            TotalItens = publicados.Count,
            PaginaAtual = 1,
            TotalPaginas = 1,
            ItensPorPagina = publicados.Count
        };

        return View(viewModel); // Views/Posts/Published.cshtml
    }

    // POST: /posts/duplicar/{id}
    [HttpPost("duplicar/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> Duplicate(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            var post = await _context.Posts
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

            if (post == null)
            {
                TempData["Error"] = "Post não encontrado ou você não tem permissão para duplicá-lo.";
                return RedirectToAction(nameof(MeusPosts));
            }

            var slug = await GenerateUniqueSlugAsync($"{post.Titulo} - Copia");

            var novoPost = new PostModel
            {
                Titulo = $"{post.Titulo} - Cópia",
                Slug = slug,
                Resumo = post.Resumo,
                Conteudo = post.Conteudo,
                CategoriaId = post.CategoriaId,
                Status = StatusPostEnum.Rascunho,
                AutorId = userId!,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Posts.Add(novoPost);
            await _context.SaveChangesAsync();

            // Duplicar tags
            if (post.PostTags.Any())
            {
                var novasPostTags = post.PostTags.Select(pt => new PostTagModel
                {
                    PostId = novoPost.Id,
                    TagId = pt.TagId
                }).ToList();

                _context.PostTags.AddRange(novasPostTags);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Post duplicado: {Titulo} por {Usuario}", post.Titulo, User.Identity?.Name);
            TempData["Success"] = $"Post '{post.Titulo}' duplicado com sucesso!";

            return RedirectToAction(nameof(Edit), new { id = novoPost.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao duplicar post: {Id}", id);
            TempData["Error"] = "Erro ao duplicar post. Tente novamente.";
        }

        return RedirectToAction(nameof(MeusPosts));
    }

    // POST: /posts/enviar-aprovacao/{id}
    [HttpPost("enviar-aprovacao/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Moderador,Autor")]
    public async Task<IActionResult> SubmitForApproval(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id && p.AutorId == userId);

            if (post == null)
            {
                return Json(new { success = false, message = "Post não encontrado." });
            }

            if (post.Status != StatusPostEnum.Rascunho)
            {
                return Json(new { success = false, message = "Apenas rascunhos podem ser enviados para aprovação." });
            }

            post.Status = StatusPostEnum.AguardandoAprovacao;
            post.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Post enviado para aprovação: {Titulo} por {Usuario}", 
                post.Titulo, User.Identity?.Name);

            return Json(new { 
                success = true, 
                message = $"Post '{post.Titulo}' enviado para aprovação!" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar post para aprovação: {Id}", id);
            return Json(new { success = false, message = "Erro ao enviar post para aprovação." });
        }
    }

    #endregion

    #region MÉTODOS AUXILIARES

    private async Task CarregarDadosFormularioAsync()
    {
        ViewBag.Categorias = await _context.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        ViewBag.Tags = await _context.Tags
            .OrderBy(t => t.Nome)
            .ToListAsync();
    }

    private async Task<string> GenerateUniqueSlugAsync(string titulo, int? excludeId = null)
    {
        return _sluggerService.GenerateUniqueSlug(titulo, async slug =>
        {
            var query = _context.Posts.Where(p => p.Slug == slug);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        });
    }

    private bool PodeEditarPost(PostModel post)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Moderador"))
            return true;

        var userId = _userManager.GetUserId(User);
        return post.AutorId == userId && 
               (post.Status == StatusPostEnum.Rascunho || 
                post.Status == StatusPostEnum.Rejeitado);
    }

    private bool PodeExcluirPost(PostModel post)
    {
        if (User.IsInRole("Admin"))
            return true;

        var userId = _userManager.GetUserId(User);
        return post.AutorId == userId && post.Status == StatusPostEnum.Rascunho;
    }

    private bool PodeUsarStatus(StatusPostEnum status)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Moderador"))
            return true;

        return status == StatusPostEnum.Rascunho || 
               status == StatusPostEnum.AguardandoAprovacao;
    }

    private bool PodeAlterarStatus(StatusPostEnum statusAtual, StatusPostEnum novoStatus)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Moderador"))
            return true;

        // Autores podem alterar apenas de/para Rascunho e AguardandoAprovacao
        var statusesPermitidos = new[] { 
            StatusPostEnum.Rascunho, 
            StatusPostEnum.AguardandoAprovacao 
        };

        return statusesPermitidos.Contains(statusAtual) && 
               statusesPermitidos.Contains(novoStatus);
    }

    #endregion
}