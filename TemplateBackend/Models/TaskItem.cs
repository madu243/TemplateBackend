using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TemplateBackend.Models
{
    [Table("Tasks")]
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "In Progress";

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        // Tags stored as comma separated string
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Foreign Key ───────────────────────────────────
        [Required]
        public int UserId { get; set; }

        // ── Navigation Property ───────────────────────────
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
