using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Enums;

namespace BusinessObjects.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int PlanId { get; set; }
        public Plan? Plan { get; set; }

        [Required, StringLength(80)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Range(1, 3)]
        public int MealsPerDay { get; set; }

        /// <summary>Khung giờ giao hàng: Morning / Afternoon / Evening</summary>
        [StringLength(20)]
        public string? DeliveryTimeSlot { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

        /// <summary>Ngày bắt đầu tạm ngưng (inclusive).</summary>
        public DateOnly? PauseFrom { get; set; }

        /// <summary>Ngày kết thúc tạm ngưng (inclusive). EndDate được kéo dài tương ứng.</summary>
        public DateOnly? PauseTo { get; set; }

        [Range(0, 100000000)]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
