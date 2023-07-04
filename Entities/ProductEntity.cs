namespace Utilities_aspnet.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity {
	[MaxLength(100)]
	public string? Title { get; set; }

	[MaxLength(100)]
	public string? Subtitle { get; set; }

	[StringLength(2000)]
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
	public int? DiscountPrice { get; set; }
	public int? Price { get; set; }
	public Currency? Currency { get; set; }
	public ProductStatus? Status { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public ProductState? ProductState { get; set; }
	public DateTime? ExpireDate { get; set; }

	public string? SeenUsers { get; set; } = "";
	
	[MaxLength(500)]
	public string? Teams { get; set; } = "";

	public ProductEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<ProductEntity>? Children { get; set; }

	public string? UserId { get; set; }
	public UserEntity? User { get; set; }
	
	[MaxLength(1000)]
	public ProductJsonDetail JsonDetail { get; set; } = new();
	
	[MaxLength(100)]
	public List<TagProduct>? Tags { get; set; } = new();

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
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public double? Latitude { get; set; }
	public int? ResponseTime { get; set; }
	public int? OnTimeDelivery { get; set; }
	public double? Longitude { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public int? MinOrder { get; set; }
	public int? MaxOrder { get; set; }
	public int? MaxPrice { get; set; }
	public int? MinPrice { get; set; }
	public int? ShippingCost { get; set; }
	public int? ShippingTime { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
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
	public string? Type1 { get; set; }
	public string? Type2 { get; set; }
	public string? Unit { get; set; }
	public string? UseCase { get; set; }
	public string? KeyValue { get; set; }
	public string? State { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Price { get; set; }
	public int? Length { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public int? Weight { get; set; }
	public int? MinOrder { get; set; }
	public int? MaxOrder { get; set; }
	public int? MaxPrice { get; set; }
	public int? MinPrice { get; set; }
	public int? ScorePlus { get; set; }
	public int? ScoreMinus { get; set; }
	public int? DiscountPrice { get; set; }
	public int? ResponseTime { get; set; }
	public int? OnTimeDelivery { get; set; }
	public int? DiscountPercent { get; set; }
	public int? CommentsCount { get; set; }
	public int? Stock { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime? ExpireDate { get; set; }
	public ProductStatus? Status { get; set; }
	public Currency? Currency { get; set; }
	public AgeCategory? AgeCategory { get; set; }
	public ProductState? ProductState { get; set; }
	public int? ShippingTime { get; set; }
	public int? ShippingCost { get; set; }
	public DateTime? Boosted { get; set; }
	public Guid? ParentId { get; set; }
	public List<KeyValue>? KeyValues { get; set; }
	public List<TagProduct>? Tags { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public ProductInsightDto? ProductInsight { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<string>? Teams { get; set; }
	public IEnumerable<UploadDto>? Upload { get; set; }
	public IEnumerable<FormTitleDto>? Form { get; set; }
}

public class ProductFilterDto {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Type { get; set; }
	public string? UseCase { get; set; }
	public string? State { get; set; }
	public int? StartPriceRange { get; set; }
	public int? EndPriceRange { get; set; }
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
	public bool? ShowChildren { get; set; } = false;
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
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public ProductStatus? Status { get; set; }
	public Currency? Currency { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public List<TagProduct>? Tags { get; set; }
	public IEnumerable<string>? UserIds { get; set; }
	public string? Query { get; set; }
	public bool ShowExpired { get; set; } = false;
	public bool Boosted { get; set; }
}

public class ProductInsightDto {
	public ReactionEntity? Reaction { get; set; }
	public string? UserId { get; set; }
}