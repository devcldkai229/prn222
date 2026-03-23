using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Enums;

namespace BusinessObjects.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        public Guid? ShipperId { get; set; }
        public User? Shipper { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Planned;

        /// <summary>True when meals were auto-selected (no user selection found).</summary>
        public bool IsAutoFilled { get; set; } = false;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
