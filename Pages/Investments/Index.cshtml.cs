using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;
using System.Text;

namespace FinPlan.Web.Pages.Investments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Investment> Investments { get; set; } = new List<Investment>();

        // Параметры фильтра
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AssetTypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        // Список типов активов
        public List<string> AssetTypes { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            var query = _context.Investments
                .Where(i => i.UserId == userId)
                .AsQueryable();

            // Поиск по названию актива
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(i => i.AssetName.Contains(SearchTerm));
            }

            // Фильтр по типу актива
            if (!string.IsNullOrEmpty(AssetTypeFilter))
            {
                query = query.Where(i => i.AssetType == AssetTypeFilter);
            }

            // Фильтр по дате
            if (DateFrom.HasValue)
            {
                query = query.Where(i => i.PurchaseDate >= DateFrom.Value);
            }
            if (DateTo.HasValue)
            {
                query = query.Where(i => i.PurchaseDate <= DateTo.Value);
            }

            Investments = await query
                .OrderByDescending(i => i.PurchaseDate)
                .ToListAsync();

            // Получаем типы активов
            AssetTypes = await _context.Investments
                .Where(i => i.UserId == userId)
                .Select(i => i.AssetType)
                .Distinct()
                .ToListAsync();
        }

        // Экспорт с фильтрами
        public async Task<IActionResult> OnGetExportAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var query = _context.Investments
                .Where(i => i.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(i => i.AssetName.Contains(SearchTerm));
            }
            if (!string.IsNullOrEmpty(AssetTypeFilter))
            {
                query = query.Where(i => i.AssetType == AssetTypeFilter);
            }
            if (DateFrom.HasValue)
            {
                query = query.Where(i => i.PurchaseDate >= DateFrom.Value);
            }
            if (DateTo.HasValue)
            {
                query = query.Where(i => i.PurchaseDate <= DateTo.Value);
            }

            var investments = await query.OrderByDescending(i => i.PurchaseDate).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("\uFEFFАктив,Тип,Количество,Цена,Стоимость,Дата покупки");

            foreach (var i in investments)
            {
                var totalValue = i.Quantity * i.PurchasePrice;
                csv.AppendLine($"{i.AssetName},{i.AssetType},{i.Quantity},{i.PurchasePrice},{totalValue},{i.PurchaseDate:dd.MM.yyyy}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"investments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}