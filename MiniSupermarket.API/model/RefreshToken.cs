using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiniSupermarket.API.Models
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefreshTokenId { get; set; }

        [Required]
        [StringLength(512)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool Revoked { get; set; }                  // true = đã thu hồi

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}