using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        /// <summary>
        /// S? ti?n thanh to�n
        /// </summary>
        [Range(0, 100000000)]
        public decimal Amount { get; set; }

        /// <summary>
        /// �on v? ti?n t?: VND, USD, etc.
        /// </summary>
        [Required, StringLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Phuong th?c: MoMo, VNPay, BankTransfer, COD
        /// </summary>
        [Required, StringLength(50)]
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Tr?ng th�i: Pending, Paid, Failed, Cancelled, Expired
        /// </summary>
        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// M� thanh to�n n?i b? (unique)
        /// </summary>
        [Required, StringLength(100)]
        public string PaymentCode { get; set; } = string.Empty;

        /// <summary>
        /// M� t? thanh to�n
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        // Navigation
        public ICollection<PaymentTransaction> Transactions { get; set; } =
            new List<PaymentTransaction>();
    }
}
