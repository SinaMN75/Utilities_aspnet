namespace Utilities_aspnet.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? Subtitle { get; set; }

	[StringLength(2000)]
	public string? Description { get; set; }

	[StringLength(2000)]
	public string? Details { get; set; }

	[StringLength(500)]
	public string? Address { get; set; }

	[StringLength(500)]
	public string? Author { get; set; }

	[StringLength(500)]
	public string? PhoneNumber { get; set; }

	[StringLength(500)]
	public string? Link { get; set; }

	[StringLength(500)]
	public string? Website { get; set; }

	[StringLength(500)]
	public string? Email { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Unit { get; set; }

	[StringLength(500)]
	public string? State { get; set; }

	[StringLength(500)]
	public string? Packaging { get; set; }

	[StringLength(500)]
	public string? Shipping { get; set; }

	[StringLength(500)]
	public string? Port { get; set; }

	[StringLength(2000)]
	public string? KeyValues1 { get; set; }

	[StringLength(2000)]
	public string? KeyValues2 { get; set; }

	[StringLength(500)]
	public string? Value { get; set; }

	[StringLength(500)]
	public string? Value1 { get; set; }

	[StringLength(500)]
	public string? Value2 { get; set; }

	[StringLength(500)]
	public string? Value3 { get; set; }

	[StringLength(500)]
	public string? Value4 { get; set; }

	[StringLength(500)]
	public string? Value5 { get; set; }

	[StringLength(500)]
	public string? Value6 { get; set; }

	[StringLength(500)]
	public string? Value7 { get; set; }

	[StringLength(500)]
	public string? Value8 { get; set; }

	[StringLength(500)]
	public string? Value9 { get; set; }

	[StringLength(500)]
	public string? Value10 { get; set; }

	[StringLength(500)]
	public string? Value11 { get; set; }

	[StringLength(500)]
	public string? Value12 { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? Price { get; set; }
	public double? VoteCount { get; set; }
	public bool? IsForSale { get; set; }
	public bool? Enabled { get; set; }
	public int? VisitsCount { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public ProductStatus? Status { get; set; }
	public double? Stock { get; set; }

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<FormEntity>? Forms { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	public IEnumerable<VoteFieldEntity>? VoteFields { get; set; }
	public IEnumerable<VoteEntity>? Votes { get; set; }
	public IEnumerable<ReportEntity>? Reports { get; set; }
	public IEnumerable<BookmarkEntity>? Bookmarks { get; set; }
	public IEnumerable<CommentEntity>? Comments { get; set; }
	public IEnumerable<TeamEntity>? Teams { get; set; }
	public IEnumerable<OrderDetailEntity>? OrderDetails { get; set; }

	[NotMapped]
	public bool IsBookmarked { get; set; }

	[NotMapped]
	public bool IsTopProduct { get; set; } = false;

	[NotMapped]
	public int? CommentsCount { get; set; }

	[NotMapped]
	public int? DownloadCount { get; set; }
}

public class ProductReadDto {
	public Guid? Id { get; set; }
	public string? UserId { get; set; }
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Details { get; set; }
	public string? Address { get; set; }
	public string? Author { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Link { get; set; }
	public string? Website { get; set; }
	public string? Email { get; set; }
	public string? Type { get; set; }
	public string? Unit { get; set; }
	public string? State { get; set; }
	public string? UseCase { get; set; }
	public string? Packaging { get; set; }
	public string? Port { get; set; }
	public string? Shipping { get; set; }
	public string? KeyValues1 { get; set; }
	public string? KeyValues2 { get; set; }
	public string? Value { get; set; }
	public string? Value1 { get; set; }
	public string? Value2 { get; set; }
	public string? Value3 { get; set; }
	public string? Value4 { get; set; }
	public string? Value5 { get; set; }
	public string? Value6 { get; set; }
	public string? Value7 { get; set; }
	public string? Value8 { get; set; }
	public string? Value9 { get; set; }
	public string? Value10 { get; set; }
	public string? Value11 { get; set; }
	public string? Value12 { get; set; }
	public bool? IsForSale { get; set; }
	public bool? Enabled { get; set; }
	public bool IsBookmarked { get; set; }
	public bool IsTopProduct { get; set; } = false;
	public int? VisitsCount { get; set; }
	public int? CommentsCount { get; set; }
	public int? DownloadCount { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? Score { get; set; }
	public double? Price { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? DeletedAt { get; set; }
	public ProductStatus? Status { get; set; }
	public UserReadDto? User { get; set; }
	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryReadDto>? Categories { get; set; }
	public IEnumerable<VoteReadDto>? VoteFields { get; set; }
	public IEnumerable<FormDto>? Forms { get; set; }
	public IEnumerable<CommentReadDto>? Comments { get; set; }
	public IEnumerable<TeamReadDto>? Teams { get; set; }
}

public class ProductCreateUpdateDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Details { get; set; }
	public string? Address { get; set; }
	public string? Author { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Link { get; set; }
	public string? Website { get; set; }
	public string? Email { get; set; }
	public string? Type { get; set; }
	public string? Unit { get; set; }
	public string? UseCase { get; set; }
	public string? State { get; set; }
	public string? Packaging { get; set; }
	public string? Port { get; set; }
	public string? Shipping { get; set; }
	public string? KeyValues1 { get; set; }
	public string? KeyValues2 { get; set; }
	public string? Value { get; set; }
	public string? Value1 { get; set; }
	public string? Value2 { get; set; }
	public string? Value3 { get; set; }
	public string? Value4 { get; set; }
	public string? Value5 { get; set; }
	public string? Value6 { get; set; }
	public string? Value7 { get; set; }
	public string? Value8 { get; set; }
	public string? Value9 { get; set; }
	public string? Value10 { get; set; }
	public string? Value11 { get; set; }
	public string? Value12 { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? Price { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? ScorePlus { get; set; }
	public double? ScoreMinus { get; set; }
	public bool? IsForSale { get; set; }
	public bool? Enabled { get; set; }
	public int? Stock { get; set; }
	public int? VisitsCount { get; set; }
	public int? VisitsCountPlus { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? DeletedAt { get; set; }
	public ProductStatus? Status { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<string>? Teams { get; set; }
}

public class ProductFilterDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Details { get; set; }
	public string? Address { get; set; }
	public string? Author { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Type { get; set; }
	public string? Unit { get; set; }
	public string? UseCase { get; set; }
	public string? State { get; set; }
	public string? UserId { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? StartPriceRange { get; set; }
	public double? EndPriceRange { get; set; }
	public bool? Enabled { get; set; }
	public bool? IsForSale { get; set; }
	public bool? ShowMedia { get; set; } = false;
	public bool? ShowForms { get; set; } = false;
	public bool? ShowCategories { get; set; } = false;
	public bool? ShowVoteFields { get; set; } = false;
	public bool? ShowVotes { get; set; } = false;
	public bool? ShowReports { get; set; } = false;
	public bool? ShowComments { get; set; } = false;
	public bool? ShowTeams { get; set; } = false;
	public bool? ShowCreator { get; set; } = false;
	public bool? OrderByVotes { get; set; } = false;
	public bool? OrderByAtoZ { get; set; } = false;
	public bool? OrderByZtoA { get; set; } = false;
	public bool? OrderByPriceAccending { get; set; } = false;
	public bool? OrderByPriceDecending { get; set; } = false;
	public bool? OrderByCreatedDate { get; set; } = false;
	public bool? OrderByCreaedDateDecending { get; set; } = false;
	public int? VisitsCount { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public ProductStatus? Status { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public ProductFilterOrder? FilterOrder { get; set; } = ProductFilterOrder.AToZ;
	public string? Query { get; set; }
}

