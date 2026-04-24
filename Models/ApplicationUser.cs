using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FinPlan.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Поле ФИО обязательно")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Предпочитаемая валюта")]
        public string PreferredCurrency { get; set; } = "RUB";

        [Display(Name = "Инвестиционный профиль")]
        public string InvestmentProfile { get; set; } = "Conservative"; 

        // Навигационные свойства
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<Investment> Investments { get; set; }
    }
}