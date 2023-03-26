﻿namespace Utilities_aspnet.Entities;

[Table("Forms")]
public class FormEntity : BaseEntity {
	[StringLength(2000)]
	public string? Title { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	[ForeignKey(nameof(FormFieldId))]
	[InverseProperty(nameof(FormFieldEntity.Forms))]
	public FormFieldEntity? FormField { get; set; }

	public Guid? FormFieldId { get; set; }

	[JsonIgnore]
	public UserEntity? User { get; set; }

	[JsonIgnore]
	public string? UserId { get; set; }

	[JsonIgnore]
	public ProductEntity? Product { get; set; }

	[JsonIgnore]
	public Guid? ProductId { get; set; }

	[JsonIgnore]
	public OrderDetailEntity? OrderDetail { get; set; }

	[JsonIgnore]
	public Guid? OrderDetailId { get; set; }
}

[Table("FormFields")]
public class FormFieldEntity : BaseEntity {
	[StringLength(500)]
	public string? Label { get; set; }

	[StringLength(2000)]
	public string? OptionList { get; set; }

	[StringLength(500)]
	public string? UseCase { get; set; }

	public bool? IsRequired { get; set; } = false;

	public FormFieldType? Type { get; set; }

	public Guid? ParentId { get; set; }

	public FormFieldEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<FormFieldEntity>? Children { get; set; }

	public Guid? CategoryId { get; set; }

	[JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[InverseProperty(nameof(FormEntity.FormField))]
	public IEnumerable<FormEntity>? Forms { get; set; }
}

public class FormCreateDto {
	public string? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? OrderDetailId { get; set; }
	public IEnumerable<FormTitleDto>? Form { get; set; }
}

public class FormTitleDto {
	public Guid? Id { get; set; }
	public string? Title { get; set; }
}