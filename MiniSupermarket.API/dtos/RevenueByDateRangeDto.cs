namespace MiniSupermarket.API.dtos
{
    public class RevenueByDateRangeDto
    {
        public string CashierUsername { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
