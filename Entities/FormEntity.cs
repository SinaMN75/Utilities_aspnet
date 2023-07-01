namespace Utilities_aspnet.Entities;

[Table("Forms")]
public class FormEntity : BaseEntity {
	[MaxLength(50)]
	public string? Title { get; set; }

	[MaxLength(20)]
	public string? UseCase { get; set; }

	[ForeignKey(nameof(FormFieldId))]
	[InverseProperty(nameof(FormFieldEntity.Forms))]
	public FormFieldEntity? FormField { get; set; }

	public Guid? FormFieldId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public UserEntity? User { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public string? UserId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public ProductEntity? Product { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public Guid? ProductId { get; set; }
}

[Table("FormFields")]
public class FormFieldEntity : BaseEntity {
	public string? Label { get; set; }
	public string? UseCase { get; set; }
	public bool? IsRequired { get; set; } = false;

	[StringLength(2000)]
	public string? OptionList { get; set; }

	public FormFieldType? Type { get; set; }

	public Guid? CategoryId { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	[JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[InverseProperty(nameof(FormEntity.FormField))]
	public IEnumerable<FormEntity>? Forms { get; set; }
}

public class FormCreateDto {
	public string? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public IEnumerable<FormTitleDto>? Form { get; set; }
}

public class FormTitleDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
}