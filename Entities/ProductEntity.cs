namespace Utilities_aspnet.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(100)]
	public string? Subtitle { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	[MaxLength(20)]
	public string? Type { get; set; }

	[MaxLength(100)]
	public string? State { get; set; }

	public DateTime Boosted { get; set; }
	public int? Stock { get; set; }
	public int? VoteCount { get; set; }
	public int? DiscountPercent { get; set; }
	public int? CommentsCount { get; set; }
	public long? DiscountPrice { get; set; }
	public long? Price { get; set; }
	public Currency? Currency { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public DateTime? ExpireDate { get; set; }

	public string? SeenUsers { get; set; } = "";

	[MaxLength(500)]
	public string? Teams { get; set; } = "";

	public ProductEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }
	public GroupChatMessageEntity? GroupChatMessageEntity { get; set; }
	public Guid? GroupChatMessageId { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<ProductEntity>? Children { get; set; }

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }

	public ProductJsonDetail JsonDetail { get; set; } = new();

	[MaxLength(100)]
	public List<TagProduct>? Tags { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<FormEntity>? Forms { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	public IEnumerable<ProductInsight>? ProductInsights { get; set; }
	public IEnumerable<VisitProducts>? VisitProducts { get; set; }
	public IEnumerable<OrderDetailEntity>? OrderDetail { get; set; }
	public IEnumerable<CommentEntity>? Comments { get; set; }

	[NotMapped]
	public IEnumerable<OrderEntity>? Orders { get; set; }

	[NotMapped]
	public int? SuccessfulPurchase { get; set; }
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
	public string? KeyValue { get; set; }
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public string? Color { get; set; }
	public double? Latitude { get; set; }
	public string? AdminMessage { get; set; }
	public int? ResponseTime { get; set; }
	public int? OnTimeDelivery { get; set; }
	public double? Longitude { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public int? MinOrder { get; set; }
	public long? MaxOrder { get; set; }
	public long? MaxPrice { get; set; }
	public long? MinPrice { get; set; }
	public int? ShippingCost { get; set; }
	public int? ShippingTime { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public Reaction? UserReaction { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<ReservationTime>? ReservationTimes { get; set; }
	public List<VisitCount>? VisitCounts { get; set; }
	public List<Seat>? Seats { get; set; }
}

public class Seat {
	public string? ChairId { get; set; } = Guid.NewGuid().ToString();
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Date { get; set; }
	public string? Sans { get; set; }
	public string? Salon { get; set; }
	public int? Row { get; set; }
	public int? Column { get; set; }
	public long? Price { get; set; }
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

// public class ReservationChair {
// 	public string ChairId { get; set; } = Guid.NewGuid().ToString();
// 	public long? Price { get; set; }
// 	public string? ReservedByUserId { get; set; }
// 	public string? ReservedByUserName { get; set; }
// 	public TagReservationChair? Tag { get; set; }
// }
//
// public class ReservationChairSection {
// 	public string SectionId { get; set; } = Guid.NewGuid().ToString();
// 	public string? Title { get; set; }
// 	public List<ReservationChair>? ReservationRows { get; set; }
// }
//
// public class ReservationSaloon {
// 	public string SaloonId { get; set; } = Guid.NewGuid().ToString();
// 	public DateTime? StartDate { get; set; }
// 	public DateTime? EndDate { get; set; }
// 	public List<ReservationChairSection>? ReservationChairSections { get; set; }
// }

[Table("ProductsInsight")]
public class ProductInsight : BaseEntity {
	public ReactionEntity? Reaction { get; set; }
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	[NotMapped]
	public int? Count { get; set; } = 0;
}

[Table("VisitProducts")]
public class VisitProducts : BaseEntity {
	public UserEntity? User { get; set; }
	public string? UserId { get; set; }
	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }
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
	public string? Type { get; set; }
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public string? Unit { get; set; }
	public string? UseCase { get; set; }
	public string? KeyValue { get; set; }
	public string? State { get; set; }
	public string? Color { get; set; }
	public string? AdminMessage { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public long? Price { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public int? MinOrder { get; set; }
	public int? MaxOrder { get; set; }
	public long? MaxPrice { get; set; }
	public long? MinPrice { get; set; }
	public int? ScorePlus { get; set; }
	public int? ScoreMinus { get; set; }
	public long? DiscountPrice { get; set; }
	public int? ResponseTime { get; set; }
	public int? OnTimeDelivery { get; set; }
	public int? DiscountPercent { get; set; }
	public int? CommentsCount { get; set; }
	public int? Stock { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? ExpireDate { get; set; }
	public Currency? Currency { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public int? ShippingTime { get; set; }
	public int? ShippingCost { get; set; }
	public DateTime? Boosted { get; set; }
	public Guid? ParentId { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<TagProduct>? Tags { get; set; }
	public List<TagProduct>? RemoveTags { get; set; }
	public List<TagProduct>? AddTags { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public ProductInsightDto? ProductInsight { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<string>? Teams { get; set; }
	public IEnumerable<UploadDto>? Upload { get; set; }
	public IEnumerable<FormTitleDto>? Form { get; set; }
	public IEnumerable<ProductCreateUpdateDto>? Children { get; set; }
	public List<ReservationTime>? ReservationTimes { get; set; }
	// public ReservationSaloon? ReservationSaloon { get; set; }
	public List<Seat>? Seats { get; set; }
}

public class ProductFilterDto : BaseFilterDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public string? State { get; set; }
	public long? StartPriceRange { get; set; }
	public long? EndPriceRange { get; set; }
	public bool? IsFollowing { get; set; }
	public bool? IsBookmarked { get; set; }
	public bool? HasDiscount { get; set; }
	public bool? ShowMedia { get; set; } = false;
	public bool? ShowForms { get; set; } = false;
	public bool? ShowFormFields { get; set; } = false;
	public bool? ShowCategories { get; set; } = false;
	public bool? ShowVisitProducts { get; set; } = false;
	public bool? ShowCreator { get; set; } = false;
	public bool? ShowCategoryMedia { get; set; } = false;
	public bool? ShowChildren { get; set; } = false;
	public bool? ShowPostOfPrivateUser { get; set; }
	public bool? ShowComments { get; set; }
	public bool? OrderByVotes { get; set; } = false;
	public bool? OrderByVotesDescending { get; set; } = false;
	public bool? OrderByAtoZ { get; set; } = false;
	public bool? OrderByZtoA { get; set; } = false;
	public bool? OrderByPriceAscending { get; set; } = false;
	public bool? OrderByPriceDescending { get; set; } = false;
	public bool? OrderByCreatedDate { get; set; } = true;
	public bool? OrderByCreatedDateDescending { get; set; } = false;
	public bool? OrderByMostUsedHashtag { get; set; }
	public bool? OrderByAgeCategory { get; set; }
	public bool? OrderByCategory { get; set; }
	public bool? ShowCountOfComment { get; set; }
	public Currency? Currency { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagProduct>? Tags { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public string? Query { get; set; }
	public bool ShowExpired { get; set; } = false;
	public bool Boosted { get; set; }
	public bool ShowWithChildren { get; set; } = false;
	public bool? Shuffle1 { get; set; } = false;
	public bool? Shuffle2 { get; set; } = false;
	public bool? PostsThatMyFollowersSeen { get; set; } = false;
}

public class ProductInsightDto {
	public ReactionEntity? Reaction { get; set; }
	public string? UserId { get; set; }
}

public class CustomersPaymentPerProduct {
	public UserEntity? Customer { get; set; }
	public double? Payment { get; set; }
}