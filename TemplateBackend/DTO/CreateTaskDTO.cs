using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.DTO
{
    public class CreateTaskDTO
    {
        // Maps to → TaskItem.Title
        [Required(ErrorMessage = "Title is required!")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Title must be 3-200 characters!")]
        public string Title { get; set; } = string.Empty;

        // Maps to → TaskItem.Description
        [StringLength(1000,
            ErrorMessage = "Description max 1000 characters!")]
        public string Description { get; set; } = string.Empty;

        // Maps to → TaskItem.Priority
        // Status is NOT here — always defaults to "In Progress"
        [RegularExpression("^(High|Medium|Low)$",
            ErrorMessage = "Priority must be High, Medium or Low!")]
        public string Priority { get; set; } = "Medium";

        // Maps to → TaskItem.Tags (stored as comma-separated string)
        // string.Join(",", dto.Tags) in TaskService
        public List<string> Tags { get; set; } = new();

        // Maps to → TaskItem.DueDate (nullable)
        public DateTime? DueDate { get; set; }
    }
}