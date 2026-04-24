using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinPlan.Web.Pages.Transactions;

public class IndexModel : PageModel
{
    public List<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();
    public List<FinancialNews> News { get; set; } = new List<FinancialNews>();

    public async Task OnGet()
    {
        await LoadCurrencyRates();
        LoadFinancialNews();
    }

    private async Task LoadCurrencyRates()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.GetStringAsync("https://www.cbr-xml-daily.ru/daily_json.js");
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<CBRData>(response, jsonOptions);

                if (data != null && data.Valute != null)
                {
                    var rates = new List<CurrencyRate>();

                    if (data.Valute.USD != null)
                        rates.Add(new CurrencyRate { Code = "USD", Name = "Доллар США", Rate = data.Valute.USD.Value, Change = data.Valute.USD.Change });
                    if (data.Valute.EUR != null)
                        rates.Add(new CurrencyRate { Code = "EUR", Name = "Евро", Rate = data.Valute.EUR.Value, Change = data.Valute.EUR.Change });
                    if (data.Valute.CNY != null)
                        rates.Add(new CurrencyRate { Code = "CNY", Name = "Китайский юань", Rate = data.Valute.CNY.Value, Change = data.Valute.CNY.Change });

                    CurrencyRates = rates;
                }
            }
        }
        catch (Exception)
        {
            CurrencyRates = GetDefaultRates();
        }
    }

    private List<CurrencyRate> GetDefaultRates()
    {
        return new List<CurrencyRate>
        {
            new CurrencyRate { Code = "USD", Name = "Доллар США", Rate = 90.50m, Change = 0.25m },
            new CurrencyRate { Code = "EUR", Name = "Евро", Rate = 98.00m, Change = -0.15m },
            new CurrencyRate { Code = "CNY", Name = "Китайский юань", Rate = 12.50m, Change = 0.05m },
        };
    }

    private void LoadFinancialNews()
    {
        News = new List<FinancialNews>
        {
            new FinancialNews
            {
                Title = "ЦБ РФ сохранил ключевую ставку",
                Summary = "Банк России принял решение сохранить ключевую ставку...",
                Date = DateTime.Now.AddDays(-1),
                Source = "Банк России",
                Category = "Экономика"
            },
            new FinancialNews
            {
                Title = "Рынок акций показывает рост",
                Summary = "Основные индексы МосБиржи продемонстрировали положительную динамику...",
                Date = DateTime.Now.AddDays(-2),
                Source = "МосБиржа",
                Category = "Инвестиции"
            }
        };
    }
}

public class CurrencyRate
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Change { get; set; }
}

public class FinancialNews
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class CBRData
{
    public CBRValutes Valute { get; set; } = new CBRValutes();
}

public class CBRValutes
{
    public CBRValute USD { get; set; } = new CBRValute();
    public CBRValute EUR { get; set; } = new CBRValute();
    public CBRValute CNY { get; set; } = new CBRValute();
}

public class CBRValute
{
    public decimal Value { get; set; }
    public decimal Change { get; set; }
}