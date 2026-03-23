using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    public class Meal
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string[] Ingredients { get; set; } = new string[] { }; // JSON array of strings

        public string[] Images { get; set; } = new string[] { }; // JSON array of image URLs

        [StringLength(10000)]
        public string? Description { get; set; }

        [Range(0, 10000)]
        public int Calories { get; set; }

        [Range(0, 10000)]
        public decimal Protein { get; set; } // grams

        [Range(0, 10000)]
        public decimal Carbs { get; set; }

        [Range(0, 10000)]
        public decimal Fat { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public double[]? Embedding { get; set; }
    }
}
