using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;

namespace FinPlan.Web.Pages.Investments
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
        public Investment Investment { get; set; } = new Investment();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var investment = await _context.Investments
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (investment == null) return NotFound();

            Investment = investment;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Investments
                .FirstOrDefaultAsync(i => i.Id == Investment.Id && i.UserId == userId);

            if (existing == null) return NotFound();

            existing.AssetName = Investment.AssetName;
            existing.AssetType = Investment.AssetType;
            existing.Quantity = Investment.Quantity;
            existing.PurchasePrice = Investment.PurchasePrice;
            existing.PurchaseDate = Investment.PurchaseDate;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}