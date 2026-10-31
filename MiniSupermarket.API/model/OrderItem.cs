using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiniSupermarket.API.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải >= 1")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }             // Giá tại thời điểm bán

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }             // Quantity * UnitPrice

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        [JsonIgnore]                                       // Tránh vòng lặp Order -> Items -> Order
        public virtual Order? Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}