using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;

namespace FinPlan.Web.Pages.Budgets
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
        public Budget? Budget { get; set; }

        public IActionResult OnGet()
        {
            Budget = new Budget
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || Budget == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                Budget.UserId = user.Id;

                // Игнорируем валидацию и сохраняем
                _context.Budgets.Add(Budget);
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