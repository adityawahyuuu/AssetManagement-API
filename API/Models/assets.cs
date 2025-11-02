using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("assets")]
    public class Asset
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("room_id")]
        public int RoomId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Required]
        [Column("length_cm")]
        public int LengthCm { get; set; }

        [Required]
        [Column("width_cm")]
        public int WidthCm { get; set; }

        [Required]
        [Column("height_cm")]
        public int HeightCm { get; set; }

        [Column("clearance_front_cm")]
        public int ClearanceFrontCm { get; set; } = 0;

        [Column("clearance_sides_cm")]
        public int ClearanceSidesCm { get; set; } = 0;

        [Column("clearance_back_cm")]
        public int ClearanceBackCm { get; set; } = 0;

        [Column("function_zone")]
        [MaxLength(50)]
        public string? FunctionZone { get; set; }

        [Column("must_be_near_wall")]
        public bool MustBeNearWall { get; set; } = false;

        [Column("must_be_near_window")]
        public bool MustBeNearWindow { get; set; } = false;

        [Column("must_be_near_outlet")]
        public bool MustBeNearOutlet { get; set; } = false;

        [Column("can_rotate")]
        public bool CanRotate { get; set; } = true;

        [Column("cannot_adjacent_to")]
        public int[]? CannotAdjacentTo { get; set; }

        [Column("purchase_date")]
        public DateTime? PurchaseDate { get; set; }

        [Column("purchase_price")]
        public decimal? PurchasePrice { get; set; }

        [Column("condition")]
        [MaxLength(50)]
        public string? Condition { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [ForeignKey("UserId")]
        public virtual user_login? User { get; set; }
    }
}
