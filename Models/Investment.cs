using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinPlan.Web.Models
{
    public class Investment
    {
        [Key]
        public int Id { get; set; }

       
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Название актива обязательно")]
        [Display(Name = "Название актива")]
        public string AssetName { get; set; } // "Apple", "Bitcoin", "ОФЗ"

        [Required]
        [Display(Name = "Тип актива")]
        public string AssetType { get; set; } // "Stock", "Bond", "Crypto"

        [Required(ErrorMessage = "Количество обязательно")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Цена покупки обязательна")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Цена покупки")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Display(Name = "Дата покупки")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
    }
}