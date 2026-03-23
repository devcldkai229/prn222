using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class WeeklyMenu
    {
        public int Id { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateOnly WeekStart { get; set; }

        [Required]
        public DateOnly WeekEnd { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WeeklyMenuItem> Items { get; set; } = new List<WeeklyMenuItem>();
    }
}
