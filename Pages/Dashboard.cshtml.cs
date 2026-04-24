using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Web.Models;
using System.Security.Claims;

namespace FinPlan.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Статистика
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public decimal MonthIncome { get; set; }
        public decimal MonthExpense { get; set; }
        public decimal InvestmentValue { get; set; }

        // Последние транзакции
        public IList<Transaction> RecentTransactions { get; set; } = new List<Transaction>();

        // Бюджеты
        public IList<Budget> Budgets { get; set; } = new List<Budget>();

        // Для графиков
        public List<ChartCategoryData> ExpenseByCategory { get; set; } = new List<ChartCategoryData>();
        public List<ChartMonthData> IncomeExpenseByMonth { get; set; } = new List<ChartMonthData>();
        public List<BudgetNotification> BudgetNotifications { get; set; } = new List<BudgetNotification>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return;

            // 1. Считаем доходы и расходы
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            TotalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            Balance = TotalIncome - TotalExpense;

            // 2. Считаем за текущий месяц
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthTransactions = transactions
                .Where(t => t.Date >= monthStart && t.Date <= monthEnd)
                .ToList();

            MonthIncome = monthTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            MonthExpense = monthTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            // 3. Считаем инвестиции
            var investments = await _context.Investments
                .Where(i => i.UserId == userId)
                .ToListAsync();

            InvestmentValue = investments.Sum(i => i.Quantity * i.PurchasePrice);

            // 4. Последние 5 транзакций
            RecentTransactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            // 5. Бюджеты
            Budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.EndDate >= now)
                .ToListAsync();

            BudgetNotifications = new List<BudgetNotification>();
            foreach (var budget in Budgets)
            {
                var spent = await _context.Transactions
                    .Where(t => t.UserId == userId
                             && t.Type == "Expense"
                             && t.Category == budget.Category
                             && t.Date >= budget.StartDate
                             && t.Date <= budget.EndDate)
                    .SumAsync(t => t.Amount);

                var percent = budget.Limit > 0 ? (spent / budget.Limit) * 100 : 0;

                if (percent >= 100)
                {
                    BudgetNotifications.Add(new BudgetNotification
                    {
                        Category = budget.Category,
                        Percent = percent,
                        Level = "danger",
                        Message = $"Превышен бюджет \"{budget.Category}\" на {(percent - 100):N0}%"
                    });
                }
                else if (percent >= 80)
                {
                    BudgetNotifications.Add(new BudgetNotification
                    {
                        Category = budget.Category,
                        Percent = percent,
                        Level = "warning",
                        Message = $"Бюджет \"{budget.Category}\" использован на {percent:N0}%"
                    });
                }

                // 6. Данные для графика "Расходы по категориям"
                ExpenseByCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new ChartCategoryData
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .Take(10)
                .ToList();

                // 7. Данные для графика "Доходы/Расходы по месяцам"
                IncomeExpenseByMonth = transactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => new ChartMonthData
                    {
                        Month = $"{g.Key.Month:00}.{g.Key.Year}",
                        Income = g.Where(t => t.Type == "Income").Sum(t => t.Amount),
                        Expense = g.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                    })
                    .OrderBy(x => x.Month)
                    .TakeLast(12)
                    .ToList();
            }
        }

        // ViewModel для графика по категориям
        public class ChartCategoryData
        {
            public string Category { get; set; }
            public decimal Amount { get; set; }
        }

        // ViewModel для графика по месяцам
        public class ChartMonthData
        {
            public string Month { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
        }
    }
    public class BudgetNotification
    {
        public string Category { get; set; }
        public decimal Percent { get; set; }
        public string Level { get; set; }  // "warning" или "danger"
        public string Message { get; set; }
    }
}