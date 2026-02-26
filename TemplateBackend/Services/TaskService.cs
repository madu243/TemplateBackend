using Microsoft.EntityFrameworkCore;
using System;
using TemplateBackend.Data;
using TemplateBackend.Models;
using TemplateBackend.DTO;

namespace TemplateBackend.Services
{
    public class TaskService
    {
        private readonly TaskDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            TaskDbContext context,
            ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET ALL TASKS ──────────────────────────────────
        public async Task<List<TaskResponseDTO>> GetAllAsync(
            int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks.Select(MapToDTO).ToList();
        }

        // ── GET ALL PAGED ──────────────────────────────────
        public async Task<PagedResultDTO<TaskResponseDTO>> GetAllPagedAsync(
            int userId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? priority = null)
        {
            var query = _context.Tasks
                .Where(t => t.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultDTO<TaskResponseDTO>
            {
                Data = tasks.Select(MapToDTO).ToList(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };
        }

        // ── GET TASK BY ID ─────────────────────────────────
        public async Task<TaskResponseDTO?> GetByIdAsync(
            int id,
            int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.UserId == userId);

            return task == null ? null : MapToDTO(task);
        }

        // ── CREATE TASK ────────────────────────────────────
        public async Task<TaskResponseDTO> CreateAsync(
            CreateTaskDTO dto,
            int userId)
        {
            var task = new TaskItem
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Priority = dto.Priority,
                Status = "In Progress",
                UserId = userId,
                Tags = string.Join(",", dto.Tags),
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return MapToDTO(task);
        }

        // ── UPDATE TASK ────────────────────────────────────
        public async Task<bool> UpdateAsync(
            int id,
            UpdateTaskDTO dto,
            int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.UserId == userId);

            if (task == null) return false;

            task.Title = dto.Title.Trim();
            task.Description = dto.Description.Trim();
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.Tags = string.Join(",", dto.Tags);
            task.DueDate = dto.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            // Set completedAt if finished
            if (dto.Status == "Finished" && task.CompletedAt == null)
                task.CompletedAt = DateTime.UtcNow;

            // Clear completedAt if back to In Progress
            if (dto.Status == "In Progress")
                task.CompletedAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── MARK AS FINISHED ───────────────────────────────
        public async Task<bool> MarkAsFinishedAsync(
            int id,
            int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.UserId == userId);

            if (task == null) return false;

            task.Status = "Finished";
            task.CompletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── DELETE TASK ────────────────────────────────────
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.UserId == userId);

            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── DELETE ALL TASKS ───────────────────────────────
        public async Task<int> DeleteAllAsync(int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _context.Tasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();
            return tasks.Count;
        }

        // ── GET BY STATUS ──────────────────────────────────
        public async Task<List<TaskResponseDTO>> GetByStatusAsync(
            int userId,
            string status)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId
                         && t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks.Select(MapToDTO).ToList();
        }

        // ── GET BY PRIORITY ────────────────────────────────
        public async Task<List<TaskResponseDTO>> GetByPriorityAsync(
            int userId,
            string priority)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId
                         && t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks.Select(MapToDTO).ToList();
        }

        // ── SEARCH TASKS ───────────────────────────────────
        public async Task<List<TaskResponseDTO>> SearchAsync(
            int userId,
            string keyword)
        {
            var lowerKeyword = keyword.ToLower();

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId
                    && (t.Title.ToLower().Contains(lowerKeyword)
                    || t.Description.ToLower()
                           .Contains(lowerKeyword)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks.Select(MapToDTO).ToList();
        }

        // ── GET STATS ──────────────────────────────────────
        public async Task<TaskStatsDTO> GetStatsAsync(int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return new TaskStatsDTO
            {
                TotalTasks = tasks.Count,
                InProgress = tasks.Count(t =>
                    t.Status == "In Progress"),
                Finished = tasks.Count(t =>
                    t.Status == "Finished"),
                HighPriority = tasks.Count(t =>
                    t.Priority == "High"),
                MediumPriority = tasks.Count(t =>
                    t.Priority == "Medium"),
                LowPriority = tasks.Count(t =>
                    t.Priority == "Low"),
                OverdueTasks = tasks.Count(t =>
                    t.DueDate < DateTime.UtcNow
                    && t.Status != "Finished")
            };
        }

        // ── MAP TO DTO ─────────────────────────────────────
        private static TaskResponseDTO MapToDTO(TaskItem task) =>
            new TaskResponseDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                UserId = task.UserId,
                Tags = string.IsNullOrEmpty(task.Tags)
                    ? new List<string>()
                    : task.Tags.Split(',',
                        StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
    }
}
