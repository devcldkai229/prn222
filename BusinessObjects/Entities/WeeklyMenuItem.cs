using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class WeeklyMenuItem
    {
        public int Id { get; set; }

        [Required]
        public int WeeklyMenuId { get; set; }
        public WeeklyMenu? WeeklyMenu { get; set; }

        [Required]
        public int MealId { get; set; }
        public Meal? Meal { get; set; }

        [Range(1, 7)]
        public int DayOfWeek { get; set; } // 1..7 (Mon..Sun)
    }
}
