﻿namespace Utilities_aspnet.FormBuilder;

[Table("FormBuilder")]
public partial class FormBuilderEntity : BaseEntity
{
    public string UserId { get; set; }
    public int FormBuilderFieldId { get; set; }
    [Required]
    [StringLength(1000)]
    public string Value { get; set; }

    public virtual UserEntity User { get; set; }
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


    [ForeignKey(nameof(FormBuilderFieldId))]
    [InverseProperty(nameof(FormBuilderFieldListEntity.FormBuilders))]
    public virtual FormBuilderFieldListEntity FormBuilderField { get; set; }
}

[Table("FormBuilderFieldTypes")]
public partial class FormBuilderFieldTypeEntity
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    public string FieldTypeName { get; set; }

    [InverseProperty(nameof(FormBuilderFieldListEntity.CategoryId))]
    public virtual ICollection<FormBuilderFieldListEntity> FormBuilderFieldLists { get; set; }
}

[Table("FormBuilderFields")]
public partial class FormBuilderFieldListEntity
{

    [Key]
    public int FormBuilderFieldId { get; set; }
    public Guid CategoryId { get; set; }
    public int FormBuilderFieldTypeId { get; set; }
    [Required]
    [StringLength(100)]
    public string Lable { get; set; }
    public bool Required { get; set; }
    [StringLength(200)]
    [Display(Name = "انتخاب ها", Prompt = "نمونه : زن, مرد  | مجرد, متاهل (فاصله اهمیت ندارد)")]
    public string? OptionList { get; set; }

    [ForeignKey(nameof(FormBuilderFieldTypeId))]
    public virtual FormBuilderFieldTypeEntity FormBuilderFieldType { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public virtual CategoryEntity Gategory { get; set; }
    [InverseProperty(nameof(FormBuilderEntity.FormBuilderField))]
    public virtual ICollection<FormBuilderEntity> FormBuilders { get; set; }
}

//[Table("FormBuilderTypes")]
//public partial class FormBuilderTypeEntity
//{

//    [Key]
//    public int Id { get; set; }
//    [Required]
//    [StringLength(50)]
//    public string UserTypeName { get; set; }

//    public bool Enable { get; set; }

//    [InverseProperty(nameof(FormBuilderFieldListEntity.FormBuilderType))]
//    public virtual ICollection<FormBuilderFieldListEntity> FormBuilderFieldLists { get; set; }
//}