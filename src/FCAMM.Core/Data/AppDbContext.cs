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
            
        builder.Entity<UsuarioModel>()
            .HasOne(u => u.Perfil)
            .WithOne(p => p.Usuario)
            .HasForeignKey<PerfilModel>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade); // Se deletar usuário, deleta perfil

        builder.Entity<PerfilModel>()
            .HasKey(p => p.UsuarioId);

        builder.Entity<PerfilModel>(entity =>
        {
            entity.Property(p => p.DataAtualizacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            entity.Property(p => p.TotalTextos)
                .HasDefaultValue(0);
                
            entity.Property(p => p.TotalVisualizacoes)
                .HasDefaultValue(0);
        });

        builder.Entity<PostModel>()
            .HasOne(p => p.Autor)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AutorId)
            .OnDelete(DeleteBehavior.Restrict); // Não deletar posts se deletar autor

        builder.Entity<PostModel>()
            .HasOne(p => p.AprovadoPor)
            .WithMany(u => u.PostsAprovados)
            .HasForeignKey(p => p.AprovadoPorId)
            .OnDelete(DeleteBehavior.SetNull); // Setar null se deletar aprovador

        builder.Entity<PostModel>()
            .HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict); // Não deletar posts se deletar categoria
            
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

        // Índices para melhorar performance nas consultas
        builder.Entity<UsuarioModel>()
            .HasIndex(u => u.Role);

        builder.Entity<PerfilModel>()
            .HasIndex(p => p.Especialidade);

        builder.Entity<PostModel>()
            .HasIndex(p => p.Status);

        builder.Entity<PostModel>()
            .HasIndex(p => p.DataPublicacao);

        // Índices para aprovação
        builder.Entity<PostModel>()
            .HasIndex(p => p.AprovadoPorId);

        builder.Entity<PostModel>()
            .HasIndex(p => p.DataAprovacao);
    }
}