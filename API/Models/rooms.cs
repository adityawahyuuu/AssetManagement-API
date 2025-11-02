using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("length_m")]
        public decimal LengthM { get; set; }

        [Required]
        [Column("width_m")]
        public decimal WidthM { get; set; }

        [Column("door_position")]
        [MaxLength(20)]
        public string? DoorPosition { get; set; }

        [Column("door_width_cm")]
        public int? DoorWidthCm { get; set; }

        [Column("window_position")]
        [MaxLength(20)]
        public string? WindowPosition { get; set; }

        [Column("window_width_cm")]
        public int? WindowWidthCm { get; set; }

        [Column("power_outlet_positions")]
        public string[]? PowerOutletPositions { get; set; }

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual user_login? User { get; set; }

        public virtual ICollection<Asset>? Assets { get; set; }
    }
}
