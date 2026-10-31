namespace MiniSupermarket.API.dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        // Các khóa ngoại và tên để hiển thị
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Lấy từ bảng Category

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;     // Lấy từ bảng Brand

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }                 // (Tuỳ chọn) Lấy từ Supplier nếu cần
    }
}
