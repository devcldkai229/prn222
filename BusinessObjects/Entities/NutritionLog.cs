using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class NutritionLog
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required, EmailAddress, StringLength(120)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public int MealId { get; set; }
        public Meal? Meal { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}
