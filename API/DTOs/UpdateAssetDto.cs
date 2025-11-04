namespace API.DTOs
{
    public class UpdateAssetDto
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? PhotoUrl { get; set; }
        public int? LengthCm { get; set; }
        public int? WidthCm { get; set; }
        public int? HeightCm { get; set; }
        public int? ClearanceFrontCm { get; set; }
        public int? ClearanceSidesCm { get; set; }
        public int? ClearanceBackCm { get; set; }
        public string? FunctionZone { get; set; }
        public bool? MustBeNearWall { get; set; }
        public bool? MustBeNearWindow { get; set; }
        public bool? MustBeNearOutlet { get; set; }
        public bool? CanRotate { get; set; }
        public int[]? CannotAdjacentTo { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? Condition { get; set; }
        public string? Notes { get; set; }
    }
}
