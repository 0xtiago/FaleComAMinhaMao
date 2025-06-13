using FCAMM.Blazor.Components.Account.Pages;
using FCAMM.Core.Models;
using FCAMM.Web.ViewModels.Conta;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FCAMM.Web.Controllers;

public class ContaController : Controller
{
    private readonly UserManager<UsuarioModel> _userManager;
    private readonly SignInManager<UsuarioModel> _signInManager;
    private readonly ILogger<ContaController> _logger;


    public ContaController(
        UserManager<UsuarioModel> userManager,
        SignInManager<UsuarioModel> signInManager,
        ILogger<ContaController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Registrar(string? returnUrl = null)
    {
        //Aqui ele retornará para a pagina anterior apos o registro
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new UsuarioModel
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                Nome = model.NomeCompleto,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
            };


            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário {Email} cadastrado com sucesso!", model.Email);

                //Faz login automagicamente
                await _signInManager.SignInAsync(user, false);

                return RedirectToLocal(returnUrl);
            }

            // Se houve erros, adiciona-os ao ModelState para exibir na view
            AddErrors(result);

        }

        return View(model);
    }


[HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Limpa cookies de autenticação externos existentes
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
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
                return RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                // Se tiver 2FA habilitado (implementação futura)
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta bloqueada: {Email}", model.Email);
                ModelState.AddModelError(string.Empty,
                    "Conta temporariamente bloqueada devido a muitas tentativas de login incorretas.");
                return View(model);
            }

            


        }
        // Login falhou
        ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
        return View(model); 
    }


[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuário fez logout.");
            
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }
        
        //MÉTODOS AUXILIARES
        // Método para adicionar erros do Identity ao ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            // Verifica se a URL é local (não externa) para evitar ataques de redirecionamento
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        
        // GET: /Conta/LoginWith2fa
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            // Verifica se o usuário passou pelo primeiro fator de autenticação
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException("Não foi possível carregar usuário para autenticação de dois fatores.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        // POST: /Conta/LoginWith2fa
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string? returnUrl = null)
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

            // Remove espaços e hífens do código
            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

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

        // GET: /Conta/Lockout
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }
        
    }
