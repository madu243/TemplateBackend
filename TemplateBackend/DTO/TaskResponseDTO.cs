using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.DTO
{
    public class TaskResponseDTO
    {
        // Primary key from Tasks table
        public int Id { get; set; }

        // Task title
        public string Title { get; set; } = string.Empty;

        // Task description
        public string Description { get; set; } = string.Empty;

        // "In Progress" or "Finished"
        public string Status { get; set; } = string.Empty;

        // "High", "Medium" or "Low"
        public string Priority { get; set; } = string.Empty;

        // Foreign key to Users table
        public int UserId { get; set; }

        // Converted from comma-separated string back to List
        // task.Tags.Split(',') in TaskService.MapToDTO()
        public List<string> Tags { get; set; } = new();

        // Optional due date
        public DateTime? DueDate { get; set; }

        // Set when Status changes to "Finished"
        public DateTime? CompletedAt { get; set; }

        // When task was created
        public DateTime CreatedAt { get; set; }

        // When task was last updated
        public DateTime UpdatedAt { get; set; }
    }
}
