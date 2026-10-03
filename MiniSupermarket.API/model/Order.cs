
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSupermarket.API.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }              // Tổng tiền hàng

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }              // Giảm giá nhập tay

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }                 // Khách phải trả

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "CASH";   // CASH, CARD, MOMO, BANK_TRANSFER

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PAID";          // PAID, CANCELLED, REFUNDED

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }               // null = khách vãng lai
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public int CashierId { get; set; }                 // thu ngân lập hoá đơn
        [ForeignKey("CashierId")]
        public virtual User? Cashier { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}