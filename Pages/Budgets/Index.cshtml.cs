using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;
using System.Text;

namespace FinPlan.Web.Pages.Budgets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<BudgetViewModel> Budgets { get; set; } = new List<BudgetViewModel>();

        // Параметры фильтра
        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        // Список категорий для фильтра
        public List<string> Categories { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            var budgets = _context.Budgets
                .Where(b => b.UserId == userId)
                .AsQueryable();

            // Фильтр по категории
            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                budgets = budgets.Where(b => b.Category == CategoryFilter);
            }

            // Фильтр по статусу (активные/завершённые)
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                var now = DateTime.Now;
                if (StatusFilter == "Active")
                {
                    budgets = budgets.Where(b => b.EndDate >= now);
                }
                else if (StatusFilter == "Completed")
                {
                    budgets = budgets.Where(b => b.EndDate < now);
                }
            }

            budgets = budgets.OrderByDescending(b => b.EndDate);

            // Рассчитываем потрачено и остаток
            Budgets = new List<BudgetViewModel>();
            foreach (var budget in budgets)
            {
                var spent = await _context.Transactions
                    .Where(t => t.UserId == userId
                             && t.Type == "Expense"
                             && t.Category == budget.Category
                             && t.Date >= budget.StartDate
                             && t.Date <= budget.EndDate)
                    .SumAsync(t => t.Amount);

                Budgets.Add(new BudgetViewModel
                {
                    Budget = budget,
                    Spent = spent,
                    Remaining = budget.Limit - spent
                });
            }

            // Получаем список категорий
            Categories = await _context.Budgets
                .Where(b => b.UserId == userId)
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();
        }

        // Экспорт с учётом фильтров
        public async Task<IActionResult> OnGetExportAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.EndDate)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("\uFEFFКатегория,Лимит,Потрачено,Остаток,Начало,Конец,Статус");

            foreach (var b in budgets)
            {
                var spent = await _context.Transactions
                    .Where(t => t.UserId == userId
                             && t.Type == "Expense"
                             && t.Category == b.Category
                             && t.Date >= b.StartDate
                             && t.Date <= b.EndDate)
                    .SumAsync(t => t.Amount);

                var remaining = b.Limit - spent;
                var status = b.EndDate >= DateTime.Now ? "Активен" : "Завершён";
                csv.AppendLine($"{b.Category},{b.Limit},{spent},{remaining},{b.StartDate:dd.MM.yyyy},{b.EndDate:dd.MM.yyyy},{status}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"budgets_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }

    public class BudgetViewModel
    {
        public Budget Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
    }
}