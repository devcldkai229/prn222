using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    public class Plan
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Weekly"

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 365)]
        public int DurationDays { get; set; }

        [Range(1, 3)]
        public int MealsPerDay { get; set; } // just 2 meals per day

        [Range(0, 100000000)]
        public decimal BasePrice { get; set; }

        [Range(0, 100000000)]
        public decimal ExtraPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
