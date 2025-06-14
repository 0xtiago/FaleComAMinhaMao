using FCAMM.Blazor.Components.Account.Pages;
using FCAMM.Core.Data;
using FCAMM.Core.Enums;
using FCAMM.Core.Models;
using FCAMM.Web.ViewModels.Conta;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Web.Controllers;

[Route("conta")]
public class ContaController : Controller
{
    private readonly UserManager<UsuarioModel> _userManager;
    private readonly SignInManager<UsuarioModel> _signInManager;
    private readonly AppDbContext _context;
    private readonly ILogger<ContaController> _logger;


    public ContaController(
        UserManager<UsuarioModel> userManager,
        SignInManager<UsuarioModel> signInManager,
        AppDbContext context,
        ILogger<ContaController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }


    [HttpGet("registrar")]
    [AllowAnonymous]
    public IActionResult Registrar(string? returnUrl = null)
    {
        //Aqui ele retornará para a pagina anterior apos o registro
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new UsuarioModel
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    Nome = model.NomeCompleto,
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true,
                    Role = RolesEnum.Autor
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // CRIAR PERFIL AUTOMATICAMENTE
                    var perfil = new PerfilModel
                    {
                        UsuarioId = user.Id,
                        Bio = $"Novo autor no FCAMM - {model.NomeCompleto}",
                        TotalTextos = 0,
                        TotalVisualizacoes = 0,
                        DataAtualizacao = DateTime.UtcNow
                    };

                    _context.Perfis.Add(perfil);
                    await _context.SaveChangesAsync();

                    // COMMIT DA TRANSAÇÃO
                    await transaction.CommitAsync();

                    _logger.LogInformation("Usuário {Email} cadastrado com sucesso com perfil!", model.Email);

                    // Faz login automaticamente
                    await _signInManager.SignInAsync(user, false);

                    TempData["Success"] =
                        $"Bem-vindo, {model.NomeCompleto}! Sua conta e perfil foram criados com sucesso.";

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // ROLLBACK EM CASO DE ERRO
                    await transaction.RollbackAsync();
                    AddErrors(result);
                    TempData["Error"] = "Erro ao criar conta. Verifique as informações fornecidas.";
                }


            }
            catch (Exception ex)
            {
                // ROLLBACK EM CASO DE EXCEÇÃO
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar usuário e perfil para {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Erro interno ao criar conta. Tente novamente.");
            }

        }


        return View(model);
    }


    [HttpGet("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Limpa cookies de autenticação externos existentes
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário logado {Email}", model.Email);
                TempData["Success"] = "Login realizado com sucesso!";
                return RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta bloqueada: {Email}", model.Email);
                ModelState.AddModelError(string.Empty,
                    "Conta temporariamente bloqueada devido a muitas tentativas de login incorretas.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
            TempData["Error"] = "Credenciais inválidas. Verifique seu email e senha.";
        }

        return View(model);
    }
    
    
    // GET: /conta/perfil
    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        // Buscar usuário com perfil em uma única query
        var user = await _context.Users
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        // Se não tem perfil, criar um (failsafe)
        if (user.Perfil == null)
        {
            await CriarPerfilSeNaoExistir(user.Id);
            // Recarregar usuário com perfil
            user = await _context.Users
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == user.Id);
        }

        var model = new EditarPerfilViewModel
        {
            Nome = user.Nome,
            Bio = user.Perfil?.Bio,
            Website = user.Perfil?.Website,
            Twitter = user.Perfil?.Twitter,
            LinkedIn = user.Perfil?.LinkedIn,
            Sobre = user.Perfil?.Sobre,
            Especialidade = user.Perfil?.Especialidade,
            Email = user.Email,
            TotalTextos = user.Perfil?.TotalTextos ?? 0,
            TotalVisualizacoes = user.Perfil?.TotalVisualizacoes ?? 0
        };

        return View(model);
    }

    // POST: /conta/perfil
    [HttpPost("perfil")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(EditarPerfilViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
            return View(model);
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Atualizar dados do usuário
            user.Nome = model.Nome;
            user.DataAtualizacao = DateTime.UtcNow;

            // Atualizar ou criar perfil
            if (user.Perfil != null)
            {
                // Atualizar perfil existente
                user.Perfil.Bio = model.Bio;
                user.Perfil.Website = model.Website;
                user.Perfil.Twitter = model.Twitter;
                user.Perfil.LinkedIn = model.LinkedIn;
                user.Perfil.Sobre = model.Sobre;
                user.Perfil.Especialidade = model.Especialidade;
                user.Perfil.DataAtualizacao = DateTime.UtcNow;
            }
            else
            {
                // Criar novo perfil
                var novoPerfil = new PerfilModel
                {
                    UsuarioId = user.Id,
                    Bio = model.Bio,
                    Website = model.Website,
                    Twitter = model.Twitter,
                    LinkedIn = model.LinkedIn,
                    Sobre = model.Sobre,
                    Especialidade = model.Especialidade,
                    TotalTextos = 0,
                    TotalVisualizacoes = 0,
                    DataAtualizacao = DateTime.UtcNow
                };

                _context.Perfis.Add(novoPerfil);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Perfil atualizado com sucesso: {UserId}", user.Id);
            TempData["Success"] = "Perfil atualizado com sucesso!";

            return RedirectToAction(nameof(Perfil));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil: {UserId}", user?.Id);
            ModelState.AddModelError(string.Empty, "Erro ao atualizar perfil. Tente novamente.");
            TempData["Error"] = "Erro ao salvar as alterações.";
        }

        return View(model);
    }
    
    
    // GET: /conta/alterar-senha
    [HttpGet("alterar-senha")]
    [Authorize]
    public IActionResult AlterarSenha()
    {
        return View();
    }

    // POST: /conta/alterar-senha
    [HttpPost("alterar-senha")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.SenhaAtual, model.NovaSenha);

        if (result.Succeeded)
        {
            _logger.LogInformation("Senha alterada com sucesso: {UserId}", user.Id);
            TempData["Success"] = "Senha alterada com sucesso!";
        
            // Fazer login novamente para manter a sessão
            await _signInManager.RefreshSignInAsync(user);
        
            return RedirectToAction(nameof(Perfil));
        }

        AddErrors(result);
        TempData["Error"] = "Erro ao alterar senha. Verifique a senha atual.";

        return View(model);
    }


    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuário fez logout.");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("acesso-negado")]
    public IActionResult AcessoNegado()
    {
        return View();
    }

    // MÉTODOS AUXILIARES
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
    
    private async Task CriarPerfilSeNaoExistir(string usuarioId)
    {
        var perfilExiste = await _context.Perfis.AnyAsync(p => p.UsuarioId == usuarioId);

        if (!perfilExiste)
        {
            var perfil = new PerfilModel
            {
                UsuarioId = usuarioId,
                Bio = "Novo membro do FCAMM",
                TotalTextos = 0,
                TotalVisualizacoes = 0,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Perfis.Add(perfil);
            await _context.SaveChangesAsync();
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("login-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Não foi possível carregar usuário para autenticação de dois fatores.");
        }

        var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpPost("login-2fa")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe,
        string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Não foi possível carregar usuário para autenticação de dois fatores.");
        }

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result =
            await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe,
                model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuário logado com 2FA: {UserId}", user.Id);
            return RedirectToLocal(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("Conta bloqueada: {UserId}", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Código de autenticação inválido para usuário: {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "Código de autenticação inválido.");
            return View();
        }
    }

    [HttpGet("bloqueado")]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }
}
