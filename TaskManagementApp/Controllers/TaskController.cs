using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementApp.Data;
using TaskManagementApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagementApp.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard(string searchTerm, string statusFilter, string sortOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Query the tasks belonging to the logged-in user
            var tasksQuery = _context.TaskItems.Where(t => t.UserId == userId);

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchTerm) || t.Description.Contains(searchTerm));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

            // Sorting logic
            tasksQuery = sortOrder switch
            {
                "Priority" => tasksQuery.OrderBy(t => t.Priority),
                "DueDate" => tasksQuery.OrderBy(t => t.DueDate),
                _ => tasksQuery.OrderBy(t => t.CreatedAt),
            };

            // Populate status options for filtering
            ViewData["StatusOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Statuses", Selected = string.IsNullOrEmpty(statusFilter) },
                new SelectListItem { Value = "To-Do", Text = "To-Do", Selected = statusFilter == "To-Do" },
                new SelectListItem { Value = "In Progress", Text = "In Progress", Selected = statusFilter == "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed", Selected = statusFilter == "Completed" }
            };

            // Execute the query to get the tasks
            var tasks = await tasksQuery.ToListAsync();

            return View(tasks);
        }

        public async Task<IActionResult> ViewAll()
        {
            var tasks = await _context.TaskItems.ToListAsync();
            return View(tasks);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            task.UserId = userId;
            task.UserName = userName;
            task.CreatedAt = DateTime.Now;

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return Unauthorized();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem updatedTask)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id != updatedTask.Id)
                return BadRequest();

            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                task.Title = updatedTask.Title;
                task.Description = updatedTask.Description;
                task.Priority = updatedTask.Priority;
                task.Status = updatedTask.Status;
                task.DueDate = updatedTask.DueDate;

                _context.Update(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Dashboard));
            }

            return View(updatedTask);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return Unauthorized();

            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task != null)
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}