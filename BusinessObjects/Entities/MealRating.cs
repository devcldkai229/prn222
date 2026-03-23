using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// 📊 Đánh giá món ăn của User sau khi nhận hàng
    /// Flow 8: Meal Feedback & Preference Learning
    /// </summary>
    public class MealRating
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        [Required]
        public int MealId { get; set; }
        public Meal Meal { get; set; } = null!;

        /// <summary>
        /// Ngày giao hàng (để dễ query món của ngày hôm qua)
        /// </summary>
        [Required]
        public DateOnly DeliveryDate { get; set; }

        /// <summary>
        /// Số sao: 1-5 sao
        /// 1-2 sao: Bad (hỏi chặn món)
        /// 3 sao: OK
        /// 4-5 sao: Excellent (tăng priority)
        /// </summary>
        [Range(1, 5)]
        public int Stars { get; set; }

        /// <summary>
        /// Tags (Optional): "Hơi mặn", "Khô", "Ít đạm", "Ngon tuyệt"
        /// Lưu dạng JSON array
        /// </summary>
        [StringLength(500)]
        public string[]? Tags { get; set; }

        /// <summary>
        /// Ghi chú thêm (optional)
        /// </summary>
        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
