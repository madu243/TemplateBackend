namespace TemplateBackend.DTO
{
    public class TaskStatsDTO
    {
        public int TotalTasks { get; set; }
        public int InProgress { get; set; }
        public int Finished { get; set; }
        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }
        public int OverdueTasks { get; set; }
    }
}