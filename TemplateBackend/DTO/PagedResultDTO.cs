namespace TemplateBackend.DTO
{
    public class PagedResultDTO<T>
    {
        // The actual list of tasks for current page
        public List<T> Data { get; set; } = new();

        // Total number of tasks matching the filter
        public int TotalCount { get; set; }

        // Math.Ceiling(TotalCount / PageSize)
        public int TotalPages { get; set; }

        // Current page number (starts at 1)
        public int CurrentPage { get; set; }

        // How many tasks per page (default 10)
        public int PageSize { get; set; }

        // page < totalPages
        public bool HasNext { get; set; }

        // page > 1
        public bool HasPrevious { get; set; }
    }
}