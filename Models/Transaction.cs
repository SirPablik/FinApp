using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlan.Web.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Сумма обязательна")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Сумма")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Категория обязательна")]
        [Display(Name = "Категория")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "Тип операции")]
        public string Type { get; set; } // "Income" или "Expense"

        [Required]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Описание")]
        public string Description { get; set; }
    }
}