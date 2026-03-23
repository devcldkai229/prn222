using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// Log giao dịch với payment gateway (MoMo, VNPay, etc.)
    /// </summary>
    public class PaymentTransaction
    {
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }
        public Payment? Payment { get; set; }

        /// <summary>
        /// Tên gateway: MoMo, VNPay, BankTransfer, COD
        /// </summary>
        [Required, StringLength(50)]
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// Request ID từ gateway
        /// </summary>
        [StringLength(200)]
        public string? RequestId { get; set; }

        /// <summary>
        /// Order ID từ gateway
        /// </summary>
        [StringLength(200)]
        public string? OrderId { get; set; }

        /// <summary>
        /// Response code từ gateway
        /// </summary>
        [StringLength(50)]
        public string? ResponseCode { get; set; }

        /// <summary>
        /// Response message từ gateway
        /// </summary>
        [StringLength(500)]
        public string? ResponseMessage { get; set; }

        /// <summary>
        /// Raw JSON response từ gateway (để debug)
        /// </summary>
        public string? RawResponseJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
