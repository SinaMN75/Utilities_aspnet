namespace Utilities_aspnet.Utilities.Dtos;

public class LocationReadDto {
    public int Id { get; set; }
    public string Title { get; set; }
    public int? ParentId { get; set; }
    public LocationReadDto? Parent { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public ICollection<MediaDto>? Media { get; set; }
    public LocationType Type { get; set; }
}