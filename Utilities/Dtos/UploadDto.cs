﻿namespace Utilities_aspnet.Utilities.Dtos;

public class UploadDto {
    public string? UserId { get; set; }
    public List<IFormFile> Files { get; set; }
    public Guid? AdsId { get; set; }
    public Guid? JobId { get; set; }
    public Guid? LearnId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TenderId { get; set; }
    public Guid? TutorialId { get; set; }
    public Guid? EventId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? MagazineId { get; set; }
    public Guid? ServiceId { get; set; }
}