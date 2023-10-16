namespace Utilities_aspnet.Utilities;

public static class Seeder {
	public const string SampleDescription =
		"لورم ایپسوم یا طرح‌نما (به انگلیسی: Lorem ipsum) به متنی آزمایشی و بی‌معنی در صنعت چاپ، صفحه‌آرایی و طراحی گرافیک گفته می‌شود. طراح گرافیک از این متن به عنوان عنصری از ترکیب بندی برای پر کردن صفحه و ارایه اولیه شکل ظاهری و کلی طرح سفارش گرفته شده استفاده می نماید، تا از نظر گرافیکی نشانگر چگونگی نوع و اندازه فونت و ظاهر متن باشد. معمولا طراحان گرافیک برای صفحه‌آرایی، نخست از متن‌های آزمایشی و بی‌معنی استفاده می‌کنند تا صرفا به مشتری یا صاحب کار خود نشان دهند که صفحه طراحی یا صفحه بندی شده بعد از اینکه متن در آن قرار گیرد چگونه به نظر می‌رسد و قلم‌ها و اندازه‌بندی‌ها چگونه در نظر گرفته شده‌است. از آنجایی که طراحان عموما نویسنده متن نیستند و وظیفه رعایت حق تکثیر متون را ندارند و در همان حال کار آنها به نوعی وابسته به متن می‌باشد آنها با استفاده از محتویات ساختگی، صفحه گرافیکی خود را صفحه‌آرایی می‌کنند تا مرحله طراحی و صفحه‌بندی را به پایان برند.";

	public const string SampleTitle = "لورم ایپسوم یا طرح‌نما (به انگلیسی: Lorem ipsum)";

	public static void SetupModelBuilder(this ModelBuilder builder) {
		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<GroupChatEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonDetail, b => b.ToJson());
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(_ => _.KeyValues);
			b.OwnsMany(_ => _.DaysAvailable).OwnsMany(_ => _.Times);
			b.OwnsMany(_ => _.DaysReserved).OwnsMany(_ => _.Times);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonDetail, b => {
			b.ToJson();
			b.OwnsMany(_ => _.Reacts);
		});
	}

	public static void SeedContent(this ModelBuilder builder) {
		#region Content

		builder.Entity<ContentEntity>().HasData(
			new ContentEntity {
				Id = Guid.Parse("61b5a1b3-e6d3-49a7-8bf0-e9d5ba585c18"),
				Title = SampleTitle,
				SubTitle = SampleTitle,
				Description = SampleDescription,
				Tags = new List<TagContent> { TagContent.Terms }
			}
		);
		builder.Entity<ContentEntity>().HasData(
			new ContentEntity {
				Id = Guid.Parse("61f54f5d-5076-4449-9e06-1749ae675dea"),
				Title = SampleTitle,
				SubTitle = SampleTitle,
				Description = SampleDescription,
				Tags = new List<TagContent> { TagContent.AboutUs }
			}
		);
		builder.Entity<ContentEntity>().HasData(
			new ContentEntity {
				Id = Guid.Parse("af233cad-d72c-4823-a7eb-b9c942aa9609"),
				Title = SampleTitle,
				SubTitle = SampleTitle,
				Description = SampleDescription,
				Tags = new List<TagContent> { TagContent.HomeBanner1 }
			}
		);
		builder.Entity<ContentEntity>().HasData(
			new ContentEntity {
				Id = Guid.Parse("d1827b50-ec7c-40bc-9f39-a87e96a45264"),
				Title = SampleTitle,
				SubTitle = SampleTitle,
				Description = SampleDescription,
				Tags = new List<TagContent> { TagContent.HomeBanner2 }
			}
		);

		#endregion

		#region User

		builder.Entity<UserEntity>().HasData(
			new UserEntity {
				Id = "80757645-2f73-47bf-9693-b28290e98ce7",
				FullName = "Sina MohammadZadeh",
				FirstName = "Sina",
				LastName = "MohammadZadeh",
				UserName = "09351902721",
				PhoneNumber = "09351902721",
				Email = "sinamnouri@yahoo.com",
				AppUserName = "SinaMN75",
				AppPhoneNumber = "09351902721",
				Birthdate = new DateTime(1996, 07, 21),
				Bio = "Software Developer",
				Gender = GenderType.Male,
				State = "Iran",
				Region = "Tehran",
				Password = "123456789",
				CreatedAt = DateTime.Now,
				Headline = "Software Developer",
				Wallet = 1000000,
				AppEmail = "sinamnouri@yahoo.com",
				JsonDetail = new UserJsonDetail()
			}
		);

		#endregion

		#region Category

		builder.Entity<CategoryEntity>().HasData(
			new CategoryEntity {
				Id = Guid.Parse("860cfdad-1455-4ddd-bd6b-3e75798e3340"),
				Title = "موبایل",
				TitleTr1 = "Mobile",
				Tags = new List<TagCategory> { TagCategory.Category },
				JsonDetail = new CategoryJsonDetail()
			}
		);

		builder.Entity<CategoryEntity>().HasData(
			new CategoryEntity {
				Id = Guid.Parse("860cfdad-1455-4ddd-bd6b-3e75798e3340"),
				Title = "کتاب",
				TitleTr1 = "Book",
				Tags = new List<TagCategory> { TagCategory.Category },
				JsonDetail = new CategoryJsonDetail()
			}
		);

		builder.Entity<CategoryEntity>().HasData(
			new CategoryEntity {
				Id = Guid.Parse("860cfdad-1455-4ddd-bd6b-3e75798e3340"),
				Title = "کیف",
				TitleTr1 = "Bag",
				Tags = new List<TagCategory> { TagCategory.Category },
				JsonDetail = new CategoryJsonDetail()
			}
		);

		#endregion
	}
}