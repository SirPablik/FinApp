using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;

namespace FinPlan.Web.Pages.Transactions
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
        public Transaction Transaction { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (Transaction == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == Transaction.Id && t.UserId == userId);

            if (existingTransaction == null) return NotFound();

            existingTransaction.Amount = Transaction.Amount;
            existingTransaction.Category = Transaction.Category;
            existingTransaction.Type = Transaction.Type;
            existingTransaction.Date = Transaction.Date;
            existingTransaction.Description = Transaction.Description;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}