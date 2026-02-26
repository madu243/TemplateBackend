using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TemplateBackend.DTO;
using TemplateBackend.Services;

namespace TemplateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            TaskService taskService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        // ── Helper: Get logged in user ID ─────────────────
        private int GetUserId()
        {
            var claim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks
        // Get all tasks (with optional pagination & filter)
        // ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null)
        {
            var userId = GetUserId();

            var result = await _taskService.GetAllPagedAsync(
                userId, page, pageSize, status, priority);

            return Ok(new
            {
                message = "Tasks retrieved successfully!",
                data = result
            });
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks/{id}
        // Get single task by ID
        // ─────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var task = await _taskService
                .GetByIdAsync(id, userId);

            if (task == null)
                return NotFound(new
                {
                    message = "Task not found!"
                });

            return Ok(new
            {
                message = "Task retrieved successfully!",
                data = task
            });
        }

        // ─────────────────────────────────────────────────
        // POST api/tasks
        // Create new task
        // ─────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTaskDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var newTask = await _taskService
                .CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = newTask.Id },
                new
                {
                    message = "Task created successfully!",
                    data = newTask
                }
            );
        }

        // ─────────────────────────────────────────────────
        // PUT api/tasks/{id}
        // Update task
        // ─────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTaskDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var updated = await _taskService
                .UpdateAsync(id, dto, userId);

            if (!updated)
                return NotFound(new
                {
                    message = "Task not found!"
                });

            return Ok(new
            {
                message = "Task updated successfully!"
            });
        }

        // ─────────────────────────────────────────────────
        // PATCH api/tasks/{id}/finish
        // Mark task as finished
        // ─────────────────────────────────────────────────
        [HttpPatch("{id:int}/finish")]
        public async Task<IActionResult> MarkAsFinished(int id)
        {
            var userId = GetUserId();
            var finished = await _taskService
                .MarkAsFinishedAsync(id, userId);

            if (!finished)
                return NotFound(new
                {
                    message = "Task not found!"
                });

            return Ok(new
            {
                message = "Task marked as finished!"
            });
        }

        // ─────────────────────────────────────────────────
        // DELETE api/tasks/{id}
        // Delete single task
        // ─────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var deleted = await _taskService
                .DeleteAsync(id, userId);

            if (!deleted)
                return NotFound(new
                {
                    message = "Task not found!"
                });

            return Ok(new
            {
                message = "Task deleted successfully!"
            });
        }

        // ─────────────────────────────────────────────────
        // DELETE api/tasks
        // Delete all tasks for logged in user
        // ─────────────────────────────────────────────────
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetUserId();
            var deletedCount = await _taskService
                .DeleteAllAsync(userId);

            return Ok(new
            {
                message = $"{deletedCount} tasks deleted successfully!"
            });
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks/status/{status}
        // Filter tasks by status
        // ─────────────────────────────────────────────────
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var userId = GetUserId();
            var tasks = await _taskService
                .GetByStatusAsync(userId, status);

            return Ok(new
            {
                message = $"Tasks with status '{status}'!",
                count = tasks.Count,
                data = tasks
            });
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks/priority/{priority}
        // Filter tasks by priority
        // ─────────────────────────────────────────────────
        [HttpGet("priority/{priority}")]
        public async Task<IActionResult> GetByPriority(
            string priority)
        {
            var userId = GetUserId();
            var tasks = await _taskService
                .GetByPriorityAsync(userId, priority);

            return Ok(new
            {
                message = $"Tasks with priority '{priority}'!",
                count = tasks.Count,
                data = tasks
            });
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks/search?keyword=xxx
        // Search tasks by title or description
        // ─────────────────────────────────────────────────
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new
                {
                    message = "Search keyword is required!"
                });

            var userId = GetUserId();
            var tasks = await _taskService
                .SearchAsync(userId, keyword);

            return Ok(new
            {
                message = $"Results for '{keyword}'",
                count = tasks.Count,
                data = tasks
            });
        }

        // ─────────────────────────────────────────────────
        // GET api/tasks/stats
        // Get task statistics for dashboard
        // ─────────────────────────────────────────────────
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetUserId();
            var stats = await _taskService
                .GetStatsAsync(userId);

            return Ok(new
            {
                message = "Stats retrieved successfully!",
                data = stats
            });
        }
    }
}
