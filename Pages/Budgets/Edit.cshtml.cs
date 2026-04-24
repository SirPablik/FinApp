using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;

namespace FinPlan.Web.Pages.Budgets
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Budget Budget { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (Budget == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == Budget.Id && b.UserId == userId);

            if (existingBudget == null) return NotFound();

            existingBudget.Category = Budget.Category;
            existingBudget.Limit = Budget.Limit;
            existingBudget.StartDate = Budget.StartDate;
            existingBudget.EndDate = Budget.EndDate;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}