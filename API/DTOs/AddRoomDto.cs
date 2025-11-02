namespace API.DTOs
{
    public class AddRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal LengthM { get; set; }
        public decimal WidthM { get; set; }
        public string? DoorPosition { get; set; }
        public int? DoorWidthCm { get; set; }
        public string? WindowPosition { get; set; }
        public int? WindowWidthCm { get; set; }
        public string[]? PowerOutletPositions { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Notes { get; set; }
    }
}
