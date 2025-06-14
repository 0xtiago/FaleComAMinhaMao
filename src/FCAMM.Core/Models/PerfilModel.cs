using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCAMM.Core.Models;

public class PerfilModel
    {
        [Key]
        [ForeignKey("Usuario")] // Chave primária E estrangeira
        public string UsuarioId { get; set; } = string.Empty;
        
        // Dados comuns para todos os tipos
        [StringLength(500)]
        [Display(Name = "Biografia")]
        public string? Bio { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Website")]
        [Url(ErrorMessage = "Digite uma URL válida")]
        public string? Website { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Twitter")]
        public string? Twitter { get; set; }
        
        [StringLength(100)]
        [Display(Name = "LinkedIn")]
        public string? LinkedIn { get; set; }
        
        [StringLength(1000)]
        [Display(Name = "Sobre Mim")]
        public string? Sobre { get; set; }
        
        // Campos específicos por tipo (nullable para quem não usa)
        
        // Para AUTOR
        [StringLength(100)]
        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }
        
        public int TotalTextos { get; set; } = 0;
        public int TotalVisualizacoes { get; set; } = 0;
        
        // Para MODERADOR/ADMIN
        [Display(Name = "Data de Nomeação")]
        public DateTime? DataNomeacao { get; set; }
        
        
        
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        
        // Relacionamento 1:1 OBRIGATÓRIO
        public virtual UsuarioModel Usuario { get; set; } = null!;
    }