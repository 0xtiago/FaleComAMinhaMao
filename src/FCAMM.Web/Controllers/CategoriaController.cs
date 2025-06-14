using FCAMM.Core.Data;
using FCAMM.Core.Models;
using FCAMM.Core.Services;
using FCAMM.Web.ViewModels.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

[Route("admin/categorias")]
[Authorize(Roles = "Admin,Moderador")]
public class CategoriaController : Controller
{
    private readonly AppDbContext _context;
    private readonly ISluggerService _sluggerService;
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(
        AppDbContext context,
        ISluggerService sluggerService,
        ILogger<CategoriaController> logger)
    {
        _context = context;
        _sluggerService = sluggerService;
        _logger = logger;
    }

    #region LISTAGEM

    // GET: /admin/categorias
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, bool? ativo, int pagina = 1)
    {
        const int itensPorPagina = 10;

        var query = _context.Categorias.AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(c => c.Nome.Contains(busca) || c.Descricao!.Contains(busca));
            ViewData["Busca"] = busca;
        }

        if (ativo.HasValue)
        {
            query = query.Where(c => c.Ativo == ativo.Value);
            ViewData["Ativo"] = ativo;
        }

        // Incluir contagem de posts
        query = query.Include(c => c.Posts);

        // Ordenação
        query = query.OrderBy(c => c.Nome);

        // Paginação
        var totalItens = await query.CountAsync();
        var categorias = await query
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        ViewData["PaginaAtual"] = pagina;
        ViewData["TotalPaginas"] = (int)Math.Ceiling((double)totalItens / itensPorPagina);
        ViewData["TotalItens"] = totalItens;

        return View(categorias);
    }

    #endregion

    #region CRIAR

    // GET: /admin/categorias/criar
    [HttpGet("criar")]
    public IActionResult Criar()
    {
        return View();
    }

    // POST: /admin/categorias/criar
    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarCategoriaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Gerar slug único
            var slug = await GenerateUniqueSlugAsync(model.Nome);

            var categoria = new CategoriaModel
            {
                Nome = model.Nome,
                Descricao = model.Descricao,
                Slug = slug,
                Ativo = model.Ativo,
                DataCriacao = DateTime.UtcNow
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoria criada: {Nome} por {Usuario}", model.Nome, User.Identity?.Name);
            TempData["Success"] = $"Categoria '{model.Nome}' criada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria: {Nome}", model.Nome);
            ModelState.AddModelError(string.Empty, "Erro ao criar categoria. Tente novamente.");
        }

        return View(model);
    }

    #endregion

    #region EDITAR

    // GET: /admin/categorias/editar/{id}
    [HttpGet("editar/{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var model = new EditarCategoriaViewModel
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Slug = categoria.Slug,
            Ativo = categoria.Ativo
        };

        return View(model);
    }

    // POST: /admin/categorias/editar/{id}
    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarCategoriaViewModel model)
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
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "Categoria não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Se o nome mudou, gerar novo slug
            if (categoria.Nome != model.Nome)
            {
                model.Slug = await GenerateUniqueSlugAsync(model.Nome, id);
            }

            categoria.Nome = model.Nome;
            categoria.Descricao = model.Descricao;
            categoria.Slug = model.Slug;
            categoria.Ativo = model.Ativo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoria editada: {Nome} por {Usuario}", model.Nome, User.Identity?.Name);
            TempData["Success"] = $"Categoria '{model.Nome}' atualizada com sucesso!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao editar categoria: {Id}", id);
            ModelState.AddModelError(string.Empty, "Erro ao atualizar categoria. Tente novamente.");
        }

        return View(model);
    }

    #endregion

    #region EXCLUIR

    // GET: /admin/categorias/excluir/{id}
    [HttpGet("excluir/{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Posts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            TempData["Error"] = "Categoria não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        return View(categoria);
    }

    // POST: /admin/categorias/excluir/{id}
    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            var categoria = await _context.Categorias
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                TempData["Error"] = "Categoria não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (categoria.Posts.Any())
            {
                TempData["Error"] = "Não é possível excluir uma categoria que possui posts associados.";
                return RedirectToAction(nameof(Excluir), new { id });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoria excluída: {Nome} por {Usuario}", categoria.Nome, User.Identity?.Name);
            TempData["Success"] = $"Categoria '{categoria.Nome}' excluída com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir categoria: {Id}", id);
            TempData["Error"] = "Erro ao excluir categoria. Tente novamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region TOGGLE ATIVO

    // POST: /admin/categorias/toggle-ativo/{id}
    [HttpPost("toggle-ativo/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAtivo(int id)
    {
        try
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return Json(new { success = false, message = "Categoria não encontrada." });
            }

            categoria.Ativo = !categoria.Ativo;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Status da categoria alterado: {Nome} - Ativo: {Ativo} por {Usuario}", 
                categoria.Nome, categoria.Ativo, User.Identity?.Name);

            return Json(new { 
                success = true, 
                ativo = categoria.Ativo,
                message = $"Categoria {(categoria.Ativo ? "ativada" : "desativada")} com sucesso!" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status da categoria: {Id}", id);
            return Json(new { success = false, message = "Erro ao alterar status da categoria." });
        }
    }

    #endregion

    #region MÉTODOS AUXILIARES

    private async Task<string> GenerateUniqueSlugAsync(string nome, int? excludeId = null)
    {
        return _sluggerService.GenerateUniqueSlug(nome, async slug =>
        {
            var query = _context.Categorias.Where(c => c.Slug == slug);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        });
    }

    #endregion
}