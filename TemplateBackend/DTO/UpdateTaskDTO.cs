using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.DTO
{
    public class UpdateTaskDTO
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

        // Maps to → TaskItem.Status
        // If "Finished" → sets CompletedAt = DateTime.UtcNow
        // If "In Progress" → clears CompletedAt = null
        [RegularExpression("^(In Progress|Finished)$",
            ErrorMessage = "Status must be 'In Progress' or 'Finished'!")]
        public string Status { get; set; } = "In Progress";

        // Maps to → TaskItem.Priority
        [RegularExpression("^(High|Medium|Low)$",
            ErrorMessage = "Priority must be High, Medium or Low!")]
        public string Priority { get; set; } = "Medium";

        // Maps to → TaskItem.Tags
        public List<string> Tags { get; set; } = new();

        // Maps to → TaskItem.DueDate
        public DateTime? DueDate { get; set; }
    }
}
