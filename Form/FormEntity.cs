﻿namespace Utilities_aspnet.Form;

[Table("Forms")]
public class FormEntity : BaseEntity {
    public string Value { get; set; }

    public UserEntity User { get; set; }
    public string UserId { get; set; }

    public ProductEntity? Product { get; set; }
    public Guid? ProductId { get; set; }

    public ProjectEntity? Project { get; set; }
    public Guid? ProjectId { get; set; }

    public TutorialEntity? Tutorial { get; set; }
    public Guid? TutorialId { get; set; }

    public EventEntity? Event { get; set; }
    public Guid? EventId { get; set; }

    public AdEntity? Ad { get; set; }
    public Guid? AdId { get; set; }

    public CompanyEntity? Company { get; set; }
    public Guid? CompanyId { get; set; }

    public TenderEntity? Tender { get; set; }
    public Guid? TenderId { get; set; }

    public ServiceEntity? Service { get; set; }
    public Guid? ServiceId { get; set; }

    public MagazineEntity? Magazine { get; set; }
    public Guid? MagazineId { get; set; }

    [ForeignKey(nameof(FormFieldId))]
    [InverseProperty(nameof(FormFieldEntity.Forms))]
    public FormFieldEntity FormField { get; set; }

    public Guid FormFieldId { get; set; }
}

[Table("FormFields")]
public class FormFieldEntity : BaseEntity {
    public string Label { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? OptionList { get; set; }
    public FormFieldType Type { get; set; }

    public Guid? CategoryId { get; set; }
    public CategoryEntity? Category { get; set; }

    [InverseProperty(nameof(FormEntity.FormField))]
    public IEnumerable<FormEntity> Forms { get; set; }
}

public enum FormFieldType {
    SingleLineText,
    MultiLineText,
    MultiSelect,
    SingleSelect,
    Bool,
    Number,
    File,
    Image
}


public class FormReadDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public Guid FormFieldId { get; set; }
}

public class CreateFormFieldDto
{
    public string Label { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? OptionList { get; set; }
    public FormFieldType Type { get; set; }

    public Guid CategoryId { get; set; }
}