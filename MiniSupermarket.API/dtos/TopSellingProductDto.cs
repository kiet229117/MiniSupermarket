namespace MiniSupermarket.API.dtos
{
    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalSoldQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
