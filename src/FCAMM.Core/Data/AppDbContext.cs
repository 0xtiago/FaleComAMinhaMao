using FCAMM.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCAMM.Core.Data;

public class AppDbContext : IdentityDbContext<UsuarioModel>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
        
    public DbSet<PerfilModel> Perfis { get; set; }
    public DbSet<PostModel> Posts { get; set; }
    public DbSet<CategoriaModel> Categorias { get; set; }
    public DbSet<TagModel> Tags { get; set; }
    public DbSet<PostTagModel> PostTags { get; set; }
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
            
        // Configuração do relacionamento 1:1 Usuario-Perfil
        builder.Entity<UsuarioModel>()
            .HasOne(u => u.Perfil)
            .WithOne(p => p.UsuarioModel)
            .HasForeignKey<PerfilModel>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configuração do relacionamento N:N Post-Tag
        builder.Entity<PostTagModel>()
            .HasKey(pt => new { pt.PostId, pt.TagId });
            
        builder.Entity<PostTagModel>()
            .HasOne(pt => pt.PostModel)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId);
            
        builder.Entity<PostTagModel>()
            .HasOne(pt => pt.TagModel)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId);
            
        // Índices únicos para Slugs
        builder.Entity<PostModel>()
            .HasIndex(p => p.Slug)
            .IsUnique();
            
        builder.Entity<CategoriaModel>()
            .HasIndex(c => c.Slug)
            .IsUnique();
            
        builder.Entity<TagModel>()
            .HasIndex(t => t.Slug)
            .IsUnique();
    }
}