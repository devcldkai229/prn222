using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public int MealId { get; set; }
        public Meal? Meal { get; set; }

        public OrderItemStatus Status { get; set; } = OrderItemStatus.Planned;

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// DeliverySlot cho món này (Morning/Evening)
        /// Mỗi item có thể thuộc buổi khác nhau
        /// </summary>
        public int? DeliverySlotId { get; set; }
        public DeliverySlot? DeliverySlot { get; set; }

        /// <summary>
        /// Địa chỉ giao hàng tại thời điểm tạo đơn (snapshot)
        /// Để shipper biết giao đến địa chỉ nào
        /// </summary>
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        /// <summary>
        /// S3 Key của ảnh xác nhận giao hàng
        /// Shipper chụp ảnh khi giao hàng thành công và upload lên S3
        /// </summary>
        [StringLength(500)]
        public string[]? ImageS3Keys { get; set; }

        /// <summary>
        /// Thời điểm giao hàng thành công (khi shipper bấm "Hoàn thành")
        /// </summary>
        public DateTime? DeliveredAt { get; set; }
    }
}
