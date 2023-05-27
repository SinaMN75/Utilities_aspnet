namespace Utilities_aspnet.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity {
	[StringLength(500)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? Subtitle { get; set; }

	[StringLength(2000)]
	public string? Description { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[StringLength(500)]
	public string? Type { get; set; }

	[StringLength(500)]
	public string? State { get; set; }

	public DateTime Boosted { get; set; }
	public double? Stock { get; set; }
	public double? VoteCount { get; set; }
	public int? DiscountPercent { get; set; }
	public int? VisitsCount { get; set; }
	public bool? Enabled { get; set; }
	public double? DiscountPrice { get; set; }
	public double? Price { get; set; }

	public Currency? Currency { get; set; }
	public ProductStatus? Status { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public ProductState? ProductState { get; set; }
	public DateTime? ExpireDate { get; set; }

	public string? SeenUsers { get; set; } = "";
	public string? Teams { get; set; } = "";

	public ProductEntity? Product { get; set; }
	public Guid? ParentId { get; set; }

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }

	public ProductJsonDetail ProductJsonDetail { get; set; } = new();

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<FormEntity>? Forms { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	public IEnumerable<ProductInsight>? ProductInsights { get; set; }
	public IEnumerable<VisitProducts>? VisitProducts { get; set; }

	[NotMapped]
	public string? SuccessfulPurchase { get; set; }
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
	public double? Latitude { get; set; }
	public double? ResponseTime { get; set; }
	public double? OnTimeDelivery { get; set; }
	public double? Longitude { get; set; }
	public double? Length { get; set; }
	public double? Width { get; set; }
	public double? Height { get; set; }
	public double? Weight { get; set; }
	public double? MinOrder { get; set; }
	public double? MaxOrder { get; set; }
	public double? MaxPrice { get; set; }
	public double? MinPrice { get; set; }
	public double? ShippingCost { get; set; }
	public int? ShippingTime { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}

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
	public string? KeyValue { get; set; }
	public string? State { get; set; }
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
	public double? DiscountPrice { get; set; }
	public double? ResponseTime { get; set; }
	public double? OnTimeDelivery { get; set; }
	public int? DiscountPercent { get; set; }
	public int? Stock { get; set; }
	public int? VisitsCount { get; set; }
	public int? VisitsCountPlus { get; set; }
	public bool? Enabled { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? ExpireDate { get; set; }
	public ProductStatus? Status { get; set; }
	public Currency? Currency { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public ProductState? ProductState { get; set; }
	public int? ShippingTime { get; set; }
	public double? ShippingCost { get; set; }
	public DateTime? Boosted { get; set; }
	public Guid? ParentId { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public ProductInsightDto? ProductInsight { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<string>? Teams { get; set; }
	public IEnumerable<UploadDto>? Upload { get; set; }
}

public class ProductFilterDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public string? State { get; set; }
	public double? StartPriceRange { get; set; }
	public double? EndPriceRange { get; set; }
	public bool? Enabled { get; set; }
	public bool? IsFollowing { get; set; }
	public bool? IsBookmarked { get; set; }
	public bool? HasDiscount { get; set; }
	public bool? ShowMedia { get; set; } = false;
	public bool? ShowForms { get; set; } = false;
	public bool? ShowFormFields { get; set; } = false;
	public bool? ShowCategories { get; set; } = false;
	public bool? ShowCategoriesFormFields { get; set; } = false;
	public bool? ShowVisitProducts { get; set; } = false;
	public bool? ShowCreator { get; set; } = false;
	public bool? ShowCategoryMedia { get; set; } = false;
	public bool? OrderByVotes { get; set; } = false;
	public bool? OrderByVotesDecending { get; set; } = false;
	public bool? OrderByAtoZ { get; set; } = false;
	public bool? OrderByZtoA { get; set; } = false;
	public bool? OrderByPriceAccending { get; set; } = false;
	public bool? OrderByPriceDecending { get; set; } = false;
	public bool? OrderByCreatedDate { get; set; } = true;
	public bool? OrderByCreaedDateDecending { get; set; } = false;
	public bool? OrderByMostUsedHashtag { get; set; }
	public bool? OrderByAgeCategory { get; set; }
	public bool? OrderByCategory { get; set; }
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public ProductStatus? Status { get; set; }
	public Currency? Currency { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public string? Query { get; set; }
	public bool ShowExpired { get; set; } = false;
	public bool Boosted { get; set; }
}

public class ProductInsightDto {
	public ReactionEntity? Reaction { get; set; }
	public string? UserId { get; set; }
}

public class SimpleSellDto {
	public string? BuyerUserId { get; set; }
	public Guid ProductId { get; set; }
}