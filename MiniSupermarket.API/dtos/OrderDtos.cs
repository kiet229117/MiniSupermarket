using System.ComponentModel.DataAnnotations;

namespace MiniSupermarket.API.Dtos
{
    // Dữ liệu client gửi lên khi tạo hoá đơn.
    // Giá bán, tổng tiền, mã hoá đơn do SERVER tính, client không được gửi.
    public class CreateOrderDto
    {
        public int? CustomerId { get; set; }                 // null = khách vãng lai

        [Range(1, int.MaxValue, ErrorMessage = "Thu ngân không hợp lệ")]
        public int CashierId { get; set; }                   // sau này lấy từ JWT, bỏ field này

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "CASH";  // CASH, CARD, MOMO, BANK_TRANSFER

        [Range(0, double.MaxValue, ErrorMessage = "Giảm giá phải >= 0")]
        public decimal Discount { get; set; }

        [MinLength(1, ErrorMessage = "Hoá đơn phải có ít nhất 1 sản phẩm")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm không hợp lệ")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải >= 1")]
        public int Quantity { get; set; }
    }

    public class PosCheckoutDto
    {
        public string? CashierUsername { get; set; }
        public string? CustomerPhone { get; set; }
        public string PaymentMethod { get; set; } = "CASH";
        public decimal Discount { get; set; } = 0;
        public List<PosCheckoutItemDto> Items { get; set; } = new();
    }

    public class PosCheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}