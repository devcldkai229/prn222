using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

/// <summary>
/// Quản lý kho nguyên liệu theo ngày cho từng món ăn
/// Mỗi món có giới hạn số lượng có thể phục vụ trong một ngày cụ thể
/// </summary>
public class KitchenInventory
{
    public int Id { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public int MealId { get; set; }
    public Meal? Meal { get; set; }

    [Range(0, 10000)]
    public int QuantityLimit { get; set; }

    [Range(0, 10000)]
    public int QuantityUsed { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
