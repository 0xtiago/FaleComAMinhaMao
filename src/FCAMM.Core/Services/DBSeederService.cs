using FCAMM.Core.Enums;
using FCAMM.Core.Models;
using FCAMM.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FCAMM.Core.Data;

public class DBSeederService
{
    private readonly AppDbContext _context;
    private readonly UserManager<UsuarioModel> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ISluggerService _sluggerService;
    private readonly ILogger<DBSeederService> _logger;
    
    public DBSeederService(
        AppDbContext context,
        UserManager<UsuarioModel> userManager,
        RoleManager<IdentityRole> roleManager,
        ISluggerService sluggerService,
        ILogger<DBSeederService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _sluggerService = sluggerService;
        _logger = logger;
    }
    
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando seeding do banco de dados...");

            // Garantir que o banco foi criado
            await _context.Database.EnsureCreatedAsync();

            // Executar seeders na ordem correta
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedCategoriasAsync();
            await SeedTagsAsync();

            _logger.LogInformation("Seeding do banco de dados concluído com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o seeding do banco de dados");
            throw;
        }
    }
    
    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Verificando roles do sistema...");

        var roles = new[] { "Admin", "Moderador", "Autor" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Role criada: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Erro ao criar role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogDebug("Role já existe: {RoleName}", roleName);
            }
        }
    }
    
    private async Task SeedAdminUserAsync()
    {
        _logger.LogInformation("Verificando usuário administrador...");

        const string adminEmail = "admin@fcamm.com";
        const string adminPassword = "Admin@123";
        const string adminNome = "Administrador do Sistema";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Criar usuário admin
                adminUser = new UsuarioModel
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Nome = adminNome,
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true,
                    Role = RolesEnum.Admin
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Atribuir role de Admin
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    // Criar perfil para o admin
                    var perfilAdmin = new PerfilModel
                    {
                        UsuarioId = adminUser.Id,
                        Bio = "Administrador do sistema FCAMM",
                        Sobre = "Conta administrativa para gerenciamento do blog FCAMM - Fale com a minha mão.",
                        TotalTextos = 0,
                        TotalVisualizacoes = 0,
                        DataAtualizacao = DateTime.UtcNow
                    };

                    _context.Perfis.Add(perfilAdmin);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Usuário administrador criado: {Email}", adminEmail);
                    _logger.LogWarning("IMPORTANTE: Usuário admin criado com senha padrão. Email: {Email}, Senha: {Password}", 
                        adminEmail, adminPassword);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Erro ao criar usuário admin: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro durante criação do usuário admin");
                throw;
            }
        }
        else
        {
            _logger.LogDebug("Usuário administrador já existe: {Email}", adminEmail);

            // Verificar se tem perfil
            var temPerfil = await _context.Perfis.AnyAsync(p => p.UsuarioId == adminUser.Id);
            if (!temPerfil)
            {
                var perfilAdmin = new PerfilModel
                {
                    UsuarioId = adminUser.Id,
                    Bio = "Administrador do sistema FCAMM",
                    Sobre = "Conta administrativa para gerenciamento do blog FCAMM - Fale com a minha mão.",
                    TotalTextos = 0,
                    TotalVisualizacoes = 0,
                    DataAtualizacao = DateTime.UtcNow
                };

                _context.Perfis.Add(perfilAdmin);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Perfil criado para usuário admin existente");
            }

            // Verificar se tem a role de Admin
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Role Admin atribuída ao usuário existente");
            }
        }
    }
    
    private async Task SeedCategoriasAsync()
    {
        _logger.LogInformation("Verificando categorias padrão...");

        var categoriasPadrao = new[]
        {
            new { Nome = "Sem Categoria", Descricao = "Categoria padrão para posts sem categoria específica" },
            new { Nome = "Geral", Descricao = "Assuntos gerais e diversos" },
            new { Nome = "Tecnologia", Descricao = "Artigos sobre tecnologia, programação e inovação" },
            new { Nome = "Literatura", Descricao = "Textos literários, resenhas e análises" },
            new { Nome = "Opinião", Descricao = "Artigos de opinião e reflexões pessoais" }
        };

        foreach (var catData in categoriasPadrao)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Nome == catData.Nome);

            if (!categoriaExiste)
            {
                var slug = _sluggerService.GenerateSlug(catData.Nome);

                // Garantir que o slug é único
                var slugUnico = slug;
                var contador = 1;
                while (await _context.Categorias.AnyAsync(c => c.Slug == slugUnico))
                {
                    slugUnico = $"{slug}-{contador}";
                    contador++;
                }

                var categoria = new CategoriaModel
                {
                    Nome = catData.Nome,
                    Descricao = catData.Descricao,
                    Slug = slugUnico,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };

                _context.Categorias.Add(categoria);
                _logger.LogInformation("Categoria criada: {Nome}", catData.Nome);
            }
            else
            {
                _logger.LogDebug("Categoria já existe: {Nome}", catData.Nome);
            }
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Categorias padrão criadas com sucesso");
        }
    }
    
    private async Task SeedTagsAsync()
    {
        _logger.LogInformation("Verificando tags padrão...");

        var tagsPadrao = new[]
        {
            "blog", "fcamm", "texto", "artigo", "opinião", 
            "reflexão", "tecnologia", "literatura", "escrita", "criatividade"
        };

        foreach (var tagNome in tagsPadrao)
        {
            var tagExiste = await _context.Tags
                .AnyAsync(t => t.Nome.ToLower() == tagNome.ToLower());

            if (!tagExiste)
            {
                var slug = _sluggerService.GenerateSlug(tagNome);

                // Garantir que o slug é único
                var slugUnico = slug;
                var contador = 1;
                while (await _context.Tags.AnyAsync(t => t.Slug == slugUnico))
                {
                    slugUnico = $"{slug}-{contador}";
                    contador++;
                }

                var tag = new TagModel
                {
                    Nome = tagNome,
                    Slug = slugUnico,
                    DataCriacao = DateTime.UtcNow
                };

                _context.Tags.Add(tag);
                _logger.LogInformation("Tag criada: {Nome}", tagNome);
            }
            else
            {
                _logger.LogDebug("Tag já existe: {Nome}", tagNome);
            }
        }

        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tags padrão criadas com sucesso");
        }
    }
    
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<UsuarioModel>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var sluggerService = services.GetRequiredService<ISluggerService>();
            var logger = services.GetRequiredService<ILogger<DBSeederService>>();

            var seeder = new DBSeederService(context, userManager, roleManager, sluggerService, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DBSeederService>>();
            logger.LogError(ex, "Erro durante o seeding do banco de dados");
            throw;
        }
    }
    
    
}