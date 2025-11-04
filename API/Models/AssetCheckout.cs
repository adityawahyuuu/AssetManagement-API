using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("asset_checkouts")]
    public class AssetCheckout
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("asset_id")]
        public int AssetId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("checkout_date")]
        public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "checked-out";

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }

        [ForeignKey("UserId")]
        public virtual user_login? User { get; set; }
    }
}
