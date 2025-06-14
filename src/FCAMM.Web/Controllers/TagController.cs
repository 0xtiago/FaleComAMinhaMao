using FCAMM.Core.Data;
using FCAMM.Core.Models;
using FCAMM.Core.Services;
using FCAMM.Web.ViewModels.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

[Route("admin/tags")]
[Authorize(Roles = "Admin,Moderador")]
public class TagController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISluggerService _sluggerService;
    private readonly ILogger<TagController> _logger;

    public TagController(
        AppDbContext context,
        ISluggerService sluggerService,
        ILogger<TagController> logger)
    {
        _context = context;
        _sluggerService = sluggerService;
        _logger = logger;
    }

    #region LISTAGEM

    // GET: /admin/tags
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        const int itensPorPagina = 15;

        var query = _context.Tags.AsQueryable();

        // Filtro de busca
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(t => t.Nome.Contains(busca));
            ViewData["Busca"] = busca;
        }

        // Incluir contagem de posts
        query = query.Include(t => t.PostTags);

        // Ordenação por nome
        query = query.OrderBy(t => t.Nome);

        // Paginação
        var totalItens = await query.CountAsync();
        var tags = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        ViewData["PaginaAtual"] = pagina;
        ViewData["TotalPaginas"] = (int)Math.Ceiling((double)totalItens / itensPorPagina);
        ViewData["TotalItens"] = totalItens;

        return View(tags);
    }

    #endregion

    #region CRIAR

    // GET: /admin/tags/criar
    [HttpGet("criar")]
    public IActionResult Criar()
    {
        return View();
    }

    // POST: /admin/tags/criar
    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarTagViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Verificar se já existe uma tag com esse nome
            var tagExistente = await _context.Tags
                .AnyAsync(t => t.Nome.ToLower() == model.Nome.ToLower());

            if (tagExistente)
            {
                ModelState.AddModelError(nameof(model.Nome), "Já existe uma tag com este nome.");
                return View(model);
            }

            // Gerar slug único
            var slug = await GenerateUniqueSlugAsync(model.Nome);

            var tag = new TagModel
            {
                Nome = model.Nome.Trim(),
                Slug = slug,
                DataCriacao = DateTime.UtcNow
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tag criada: {Nome} por {Usuario}", model.Nome, User.Identity?.Name);
            TempData["Success"] = $"Tag '{model.Nome}' criada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tag: {Nome}", model.Nome);
            ModelState.AddModelError(string.Empty, "Erro ao criar tag. Tente novamente.");
        }

        return View(model);
    }

    #endregion

    #region EDITAR

    // GET: /admin/tags/editar/{id}
    [HttpGet("editar/{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
        {
            TempData["Error"] = "Tag não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var model = new EditarTagViewModel
        {
            Id = tag.Id,
            Nome = tag.Nome,
            Slug = tag.Slug
        };

        return View(model);
    }

    // POST: /admin/tags/editar/{id}
    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarTagViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Dados inválidos.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                TempData["Error"] = "Tag não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar se já existe outra tag com esse nome
            var tagExistente = await _context.Tags
                .AnyAsync(t => t.Nome.ToLower() == model.Nome.ToLower() && t.Id != id);

            if (tagExistente)
            {
                ModelState.AddModelError(nameof(model.Nome), "Já existe uma tag com este nome.");
                return View(model);
            }

            // Se o nome mudou, gerar novo slug
            if (tag.Nome != model.Nome)
            {
                model.Slug = await GenerateUniqueSlugAsync(model.Nome, id);
            }

            tag.Nome = model.Nome.Trim();
            tag.Slug = model.Slug;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tag editada: {Nome} por {Usuario}", model.Nome, User.Identity?.Name);
            TempData["Success"] = $"Tag '{model.Nome}' atualizada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao editar tag: {Id}", id);
            ModelState.AddModelError(string.Empty, "Erro ao atualizar tag. Tente novamente.");
        }

        return View(model);
    }

    #endregion

    #region EXCLUIR

    // GET: /admin/tags/excluir/{id}
    [HttpGet("excluir/{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var tag = await _context.Tags
            .Include(t => t.PostTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
        {
            TempData["Error"] = "Tag não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        return View(tag);
    }

    // POST: /admin/tags/excluir/{id}
    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            var tag = await _context.Tags
                .Include(t => t.PostTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                TempData["Error"] = "Tag não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (tag.PostTags.Any())
            {
                TempData["Error"] = "Não é possível excluir uma tag que está sendo usada em posts.";
                return RedirectToAction(nameof(Excluir), new { id });
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tag excluída: {Nome} por {Usuario}", tag.Nome, User.Identity?.Name);
            TempData["Success"] = $"Tag '{tag.Nome}' excluída com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tag: {Id}", id);
            TempData["Error"] = "Erro ao excluir tag. Tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region API PARA AUTOCOMPLETE

    // GET: /admin/tags/buscar-json
    [HttpGet("buscar-json")]
    public async Task<IActionResult> BuscarJson(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo) || termo.Length < 2)
        {
            return Json(new List<object>());
        }

        var tags = await _context.Tags
            .Where(t => t.Nome.Contains(termo))
            .OrderBy(t => t.Nome)
            .Take(10)
            .Select(t => new { id = t.Id, nome = t.Nome })
            .ToListAsync();

        return Json(tags);
    }

    #endregion

    #region MÉTODOS AUXILIARES

    private async Task<string> GenerateUniqueSlugAsync(string nome, int? excludeId = null)
    {
        return _sluggerService.GenerateUniqueSlug(nome, async slug =>
        {
            var query = _context.Tags.Where(t => t.Slug == slug);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        });
    }

    #endregion
}