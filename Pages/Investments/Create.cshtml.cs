using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using FinPlan.Web.Data;
using FinPlan.Web.Models;

namespace FinPlan.Web.Pages.Investments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public Investment? Investment { get; set; }

        public IActionResult OnGet()
        {
            Investment = new Investment { PurchaseDate = DateTime.Now };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || Investment == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                Investment.UserId = user.Id;

                _context.Investments.Add(Investment);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка: {ex.Message}");
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
                return Page();
            }
        }
    }
}