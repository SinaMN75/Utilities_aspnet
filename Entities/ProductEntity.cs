namespace Utilities_aspnet.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(10)]
	public string? Code { get; set; } = new Random().Next(10000, 99999).ToString();

	[MaxLength(100)]
	public string? Subtitle { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	[MaxLength(100)]
	public string? Country { get; set; }

	[MaxLength(100)]
	public string? State { get; set; }

	[MaxLength(100)]
	public string? City { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }

	public int? Stock { get; set; }
	public long? Price { get; set; }
	public long? Price1 { get; set; }
	public long? Price2 { get; set; }
	public Currency? Currency { get; set; }

	public ProductEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }

	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? ExpireDate { get; set; }

	public ProductJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(100)]
	public List<TagProduct> Tags { get; set; } = [];

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<ProductEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	public IEnumerable<OrderDetailEntity>? OrderDetail { get; set; }
	public IEnumerable<CommentEntity>? Comments { get; set; }
	public IEnumerable<ReportEntity>? Reports { get; set; }
	public IEnumerable<NotificationEntity>? Notifications { get; set; }
	public IEnumerable<QuestionEntity>? Questions { get; set; }
}

public class ProductJsonDetail {
	public string? Details { get; set; }
	public string? Address { get; set; }
	public string? Author { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Link { get; set; }
	public string? Website { get; set; }
	public string? Email { get; set; }
	public string? Unit { get; set; }
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public string? ClubName { get; set; }
	public string? Color { get; set; }
	public string? AdminMessage { get; set; }
	public string? PaymentRefId { get; set; }
	public int? MaximumMembers { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public long? MaxPrice { get; set; }
	public long? MinPrice { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public List<KeyValue>? KeyValues { get; set; } = [];
	public List<ReservationTime>? ReservationTimes { get; set; } = [];
	public List<VisitCount>? VisitCounts { get; set; } = [];
	public List<Seat>? Seats { get; set; } = [];
	public List<Guid>? RelatedProducts { get; set; } = [];
	public List<UserReaction>? UsersReactions { get; set; } = [];
	public List<string>? Policies { get; set; } = [];
}

public class UserReaction {
	public required string UserId { get; set; }
	public required Reaction Reaction { get; set; }
}

public class Seat {
	public string? ChairId { get; set; } = Guid.NewGuid().ToString();
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Date { get; set; }
	public string? Sans { get; set; }
	public string? Salon { get; set; }
	public int? Row { get; set; }
	public int? Column { get; set; }
	public long? Price { get; set; }
	public int? Gender { get; set; }
	public string? ReservedByUserId { get; set; }
	public string? ReservedByUserName { get; set; }
	public TagReservationChair? Tag { get; set; }
}

public class ReservationTime {
	public string ReserveId { get; set; } = Guid.NewGuid().ToString();
	public DateTime? DateFrom { get; set; }
	public DateTime? DateTo { get; set; }
	public long? Price { get; set; }
	public long? PriceForAnyExtra { get; set; }
	public int? MaxMemberAllowed { get; set; }
	public int? MaxExtraMemberAllowed { get; set; }
	public string? ReservedByUserId { get; set; }
	public string? ReservedByUserName { get; set; }
}

public class VisitCount {
	public string VisitId { get; set; } = Guid.NewGuid().ToString();
	public string? UserId { get; set; }
	public int Count { get; set; } = 1;
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
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public string? Unit { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public string? State { get; set; }
	public string? Color { get; set; }
	public string? AdminMessage { get; set; }
	public string? PaymentRefId { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? ClubName { get; set; }
	public int? MaximumMembers { get; set; }
	public List<string>? Policies { get; set; } = [];
	public long? Price { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public long? MaxPrice { get; set; }
	public long? MinPrice { get; set; }
	public long? Price1 { get; set; }
	public long? Price2 { get; set; }
	public int? Stock { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? ExpireDate { get; set; }
	public Currency? Currency { get; set; }
	public Guid? ParentId { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<TagProduct>? Tags { get; set; }
	public List<Guid>? RelatedProducts { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<ProductCreateUpdateDto>? Children { get; set; }
	public List<ReservationTime>? ReservationTimes { get; set; }
	public List<Seat>? Seats { get; set; }
}

public class ProductFilterDto : BaseFilterDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? State { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public string? StateRegion { get; set; }
	public string? Query { get; set; }
	public long? StartPriceRange { get; set; }
	public long? EndPriceRange { get; set; }
	public bool? ShowMedia { get; set; } = false;
	public bool? ShowCategories { get; set; } = false;
	public bool? ShowCreator { get; set; } = false;
	public bool? ShowCategoryMedia { get; set; } = false;
	public bool? ShowChildren { get; set; } = false;
	public bool? ShowComments { get; set; } = false;
	public bool? OrderByAtoZ { get; set; } = false;
	public bool? OrderByZtoA { get; set; } = false;
	public bool? OrderByPriceAscending { get; set; } = false;
	public bool? OrderByPriceDescending { get; set; } = false;
	public bool? OrderByCreatedDate { get; set; } = true;
	public bool? OrderByCreatedDateDescending { get; set; } = false;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public bool ShowExpired { get; set; } = false;
	public Currency? Currency { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagProduct>? Tags { get; set; }
	public List<TagProduct>? TagsInclude { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public IEnumerable<Guid>? Ids { get; set; }
}

public class ReactionCreateUpdateDto {
	public required Reaction Reaction { get; set; }
	public required Guid ProductId { get; set; }
}