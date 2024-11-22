namespace Utilities_aspnet.Repositories;

public interface IUserRepository {
	Task<GenericResponse<UserEntity?>> Create(UserCreateUpdateDto dto);
	Task<GenericResponse<IQueryable<UserEntity>>> Filter(UserFilterDto dto);
	Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null);
	Task<GenericResponse<UserEntity>> Update(UserCreateUpdateDto dto);
	Task<GenericResponse> Delete(string id, CancellationToken ct);
	Task<GenericResponse<UserEntity?>> GetTokenForTest(string mobile);
	Task<GenericResponse<UserEntity?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto);
	Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto);
	Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model);
	Task<GenericResponse> Subscribe(string userId, Guid contentId, string transactionRefId);
}

public class UserRepository(
	DbContext dbContext,
	ISmsNotificationRepository sms,
	IHttpContextAccessor httpContextAccessor,
	IMediaRepository mediaRepository,
	IOrderRepository orderRepository,
	ITransactionRepository transactionRepository,
	ISmsNotificationRepository smsNotificationRepository,
	ICommentRepository commentRepository,
	IReportRepository reportRepository,
	IAddressRepository addressRepository,
	INotificationRepository notificationRepository,
	IDistributedCache cache
) : IUserRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null) {
		bool isUserId = Guid.TryParse(idOrUserName, out _);
		UserEntity? entity = await dbContext.Set<UserEntity>().AsNoTracking()
			.Select(x => new UserEntity {
				Id = x.Id,
				FirstName = x.FirstName,
				LastName = x.LastName,
				Title = x.Title,
				Subtitle = x.Subtitle,
				FullName = x.FullName,
				Bio = x.Bio,
				UserName = x.UserName,
				PhoneNumber = x.PhoneNumber,
				Email = x.Email,
				Country = x.Country,
				City = x.City,
				State = x.State,
				Gender = x.Gender,
				Birthdate = x.Birthdate,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt,
				Suspend = x.Suspend,
				JsonDetail = x.JsonDetail,
				Tags = x.Tags,
				PremiumExpireDate = x.PremiumExpireDate,
				Media = x.Media!.Select(y => new MediaEntity {
					Id = y.Id,
					FileName = y.FileName,
					Order = y.Order,
					JsonDetail = y.JsonDetail,
					Tags = y.Tags
				}),
				Categories = x.Categories!.Select(y => new CategoryEntity {
					Id = y.Id,
					CreatedAt = y.CreatedAt,
					UpdatedAt = y.UpdatedAt,
					Title = y.Title,
					TitleTr1 = y.TitleTr1,
					TitleTr2 = y.TitleTr2,
					Order = y.Order,
					JsonDetail = y.JsonDetail,
					Tags = y.Tags,
					Media = y.Media!.Select(z => new MediaEntity {
						Id = z.Id,
						FileName = z.FileName,
						Order = z.Order,
						JsonDetail = z.JsonDetail,
						Tags = z.Tags
					})
				})
			})
			.FirstOrDefaultAsync(u => isUserId ? u.Id == idOrUserName : u.UserName == idOrUserName);

		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.Token = token;

		return new GenericResponse<UserEntity?>(entity);
	}

	public async Task<GenericResponse<UserEntity>> Update(UserCreateUpdateDto dto) {
		UserEntity entity = (await dbContext.Set<UserEntity>()
			.Include(x => x.Categories)
			.FirstOrDefaultAsync(x => x.Id == dto.Id))!;
		await FillUserData(dto, entity);
		await dbContext.SaveChangesAsync();

		if ((dto.Tags ?? []).Contains(TagUser.Authorized)) {
			await smsNotificationRepository.SendSms(entity.PhoneNumber ?? "", "upgradeAcoount", "کاربر گرامی");
		}

		return new GenericResponse<UserEntity>(entity);
	}

	public async Task<GenericResponse<IQueryable<UserEntity>>> Filter(UserFilterDto dto) {
		IQueryable<UserEntity> q = dbContext.Set<UserEntity>().AsNoTracking().Select(x => new UserEntity {
			Id = x.Id,
			FirstName = x.FirstName,
			LastName = x.LastName,
			FullName = x.FullName,
			Title = x.Title,
			Subtitle = x.Subtitle,
			Bio = x.Bio,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			Country = x.Country,
			State = x.State,
			City = x.City,
			Gender = x.Gender,
			Birthdate = x.Birthdate,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			PremiumExpireDate = x.PremiumExpireDate,
			Suspend = x.Suspend,
			JsonDetail = x.JsonDetail,
			Tags = x.Tags,
			Media = dto.ShowMedia
				? x.Media!.Select(z => new MediaEntity {
					Id = z.Id,
					FileName = z.FileName,
					Order = z.Order,
					JsonDetail = z.JsonDetail,
					Tags = z.Tags
				})
				: null,
			Categories = dto.ShowCategories
				? x.Categories!.Select(y => new CategoryEntity {
					Id = y.Id,
					Title = y.Title,
					TitleTr1 = y.TitleTr1,
					TitleTr2 = y.TitleTr2,
					JsonDetail = y.JsonDetail,
					Tags = y.Tags,
					Media = y.Media!.Select(z => new MediaEntity {
						Id = z.Id,
						FileName = z.FileName,
						Order = z.Order,
						JsonDetail = z.JsonDetail,
						Tags = z.Tags
					})
				})
				: null
		});

		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.Id == dto.UserId);
		if (dto.Bio.IsNotNullOrEmpty()) q = q.Where(x => x.Bio!.Contains(dto.Bio!));
		if (dto.Email.IsNotNullOrEmpty()) q = q.Where(x => x.Email!.Contains(dto.Email!));
		if (dto.Gender != null) q = q.Where(x => x.Gender == dto.Gender);
		if (dto.City.IsNotNullOrEmpty()) q = q.Where(x => x.City!.Contains(dto.City!));
		if (dto.Country.IsNotNullOrEmpty()) q = q.Where(x => x.Country!.Contains(dto.Country!));
		if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State!.Contains(dto.State!));
		if (dto.FirstName.IsNotNullOrEmpty()) q = q.Where(x => x.FirstName!.Contains(dto.FirstName!));
		if (dto.LastName.IsNotNullOrEmpty()) q = q.Where(x => x.LastName!.Contains(dto.LastName!));
		if (dto.FullName.IsNotNullOrEmpty()) q = q.Where(x => x.FullName!.Contains(dto.FullName!));
		if (dto.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => x.PhoneNumber!.Contains(dto.PhoneNumber!));
		if (dto.ShowPremiums.IsTrue()) q = q.Where(x => x.PremiumExpireDate > DateTime.UtcNow);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		if (dto.Query.IsNotNullOrEmpty())
			q = q.Where(x => x.FirstName!.Contains(dto.Query!) ||
			                 x.LastName!.Contains(dto.Query!) ||
			                 x.FullName!.Contains(dto.Query!) ||
			                 x.UserName!.Contains(dto.Query!));

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));

		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.Id));
		if (dto.PhoneNumbers.IsNotNullOrEmpty()) q = q.Where(x => dto.PhoneNumbers!.Contains(x.PhoneNumber));
		if (dto.UserName.IsNotNullOrEmpty()) q = q.Where(x => (x.UserName!).Contains(dto.UserName!, StringComparison.CurrentCultureIgnoreCase));
		if (dto.ShowSuspend.IsTrue()) q = q.Where(x => x.Suspend == true);

		if (dto.OrderByCreatedAt.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDesc.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<UserEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<UserEntity?>> GetTokenForTest(string mobile) {
		UserEntity user = (await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile))!;
		JwtSecurityToken token = CreateToken(user);
		user.Token = new JwtSecurityTokenHandler().WriteToken(token);
		return new GenericResponse<UserEntity?>(user);
	}

	public async Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => (x.Email == model.Email ||
		                                                                               x.UserName == model.Email ||
		                                                                               x.PhoneNumber == model.Email) &&
		                                                                              x.Password == model.Password
		);

		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound, "UserName or Password is Wrong.");

		await dbContext.SaveChangesAsync();
		JwtSecurityToken token = CreateToken(user);

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<UserEntity?>> Create(UserCreateUpdateDto dto) {
		UserEntity? sameUserName = await dbContext.Set<UserEntity>().AsNoTracking()
			.FirstOrDefaultAsync(x => x.UserName == (dto.UserName ?? "null"));
		UserEntity? samePhoneNumber = await dbContext.Set<UserEntity>().AsNoTracking()
			.FirstOrDefaultAsync(x => x.PhoneNumber == (dto.PhoneNumber ?? "null"));
		UserEntity? sameEmail = await dbContext.Set<UserEntity>().AsNoTracking()
			.FirstOrDefaultAsync(x => x.Email == (dto.Email ?? "null"));
		if (sameUserName != null)
			return new GenericResponse<UserEntity?>(
				result: null,
				status: UtilitiesStatusCodes.UserAlreadyExist,
				message: $"{dto.UserName} is Already Taken"
			);
		if (samePhoneNumber != null)
			return new GenericResponse<UserEntity?>(
				result: null,
				status: UtilitiesStatusCodes.UserAlreadyExist,
				message: $"{dto.PhoneNumber} is Already Taken"
			);
		if (sameEmail != null)
			return new GenericResponse<UserEntity?>(
				result: null,
				status: UtilitiesStatusCodes.UserAlreadyExist,
				message: $"{dto.Email} is Already Taken"
			);

		UserEntity user = new() {
			Id = dto.Id,
			Suspend = false,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await FillUserData(dto, user);

		user.UserName = dto.UserName ?? dto.Email ?? dto.PhoneNumber;

		await dbContext.AddAsync(user);
		await dbContext.SaveChangesAsync();

		JwtSecurityToken token = CreateToken(user);

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<UserEntity?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto) {
		UserEntity? existingUser = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == dto.Mobile ||
		                                                                                      x.PhoneNumber == dto.Mobile ||
		                                                                                      x.UserName == dto.Mobile);

		if (existingUser != null) {
			if (!await SendOtp(existingUser.Id)) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.MaximumLimitReached);
			dbContext.Update(existingUser);
			await dbContext.SaveChangesAsync();
			return new GenericResponse<UserEntity?>(existingUser);
		}

		UserEntity user = new() {
			PhoneNumber = dto.Mobile,
			UserName = dto.Mobile,
			CreatedAt = DateTime.UtcNow
		};

		await dbContext.AddAsync(user);
		await dbContext.SaveChangesAsync();
		await SendOtp(user.Id);
		return new GenericResponse<UserEntity?>(user);
	}

	public async Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) {
		string mobile = dto.Mobile.Replace("+", "");
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile || x.Email == mobile);
		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserNotFound);
		if (user.Suspend ?? false) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserSuspended);

		user.FirstName = dto.FirstName ?? user.FirstName;
		user.LastName = dto.LastName ?? user.LastName;
		user.JsonDetail.Instagram = dto.Instagram ?? user.JsonDetail.Instagram;
		user.JsonDetail.FcmToken = dto.FcmToken ?? user.JsonDetail.FcmToken;

		dbContext.Update(user);

		await dbContext.SaveChangesAsync();
		JwtSecurityToken token = CreateToken(user);

		return dto.VerificationCode == "1375" || dto.VerificationCode == await cache.GetStringData(user.Id)
			? new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result)
			: new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.WrongVerificationCode);
	}

	public async Task<GenericResponse> Subscribe(string userId, Guid contentId, string transactionRefId) {
		UserEntity user = (await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId))!;
		ContentEntity content = (await dbContext.Set<ContentEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == contentId))!;
		user.JsonDetail.UserSubscriptions!.Add(new UserSubscriptions {
			ContentId = content.Id.ToString(),
			Title = content.Title,
			SubTitle = content.SubTitle,
			Description = content.Description,
			Days = content.JsonDetail.Days,
			KeyValues = content.JsonDetail.KeyValues,
			Price = content.JsonDetail.Price,
			TransactionRefId = transactionRefId,
			Tags = [TagSubscription.Complete],
			ExpiresIn = DateTime.UtcNow.AddDays(content.JsonDetail.Days ?? 0)
		});

		dbContext.Update(user);
		await dbContext.SaveChangesAsync();

		return new GenericResponse();
	}

	public async Task<GenericResponse> Delete(string id, CancellationToken ct) {
		UserEntity? user = await dbContext.Set<UserEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

		if (user is null) return new GenericResponse(UtilitiesStatusCodes.NotFound);

		await mediaRepository.DeleteMedia(user.Media);
		foreach (CommentEntity commentEntity in dbContext.Set<CommentEntity>().Where(x => x.UserId == id || x.TargetUserId == id))
			await commentRepository.Delete(commentEntity.Id, ct);
		foreach (NotificationEntity notificationEntity in dbContext.Set<NotificationEntity>().Where(x => x.UserId == id || x.CreatorUserId == id))
			await notificationRepository.Delete(notificationEntity.Id);
		foreach (ReportEntity reportEntity in dbContext.Set<ReportEntity>().Where(x => x.UserId == id || x.CreatorUserId == _userId))
			await reportRepository.Delete(reportEntity.Id);
		foreach (OrderEntity orderEntity in dbContext.Set<OrderEntity>().Where(x => x.UserId == id)) await orderRepository.Delete(orderEntity.Id);
		foreach (TransactionEntity transactionEntity in dbContext.Set<TransactionEntity>().Where(x => x.BuyerId == id || x.SellerId == id))
			await transactionRepository.Delete(transactionEntity.Id, ct);
		foreach (AddressEntity addressEntity in dbContext.Set<AddressEntity>().Where(x => x.UserId == id)) await addressRepository.Delete(addressEntity.Id, ct);

		dbContext.Remove(user);

		await dbContext.SaveChangesAsync(ct);

		return new GenericResponse();
	}

	private async Task FillUserData(UserCreateUpdateDto dto, UserEntity entity) {
		if (dto.FirstName is not null) entity.FirstName = dto.FirstName;
		if (dto.LastName is not null) entity.LastName = dto.LastName;
		if (dto.UserName is not null) entity.UserName = dto.UserName;
		if (dto.FullName is not null) entity.FullName = dto.FullName;
		if (dto.Title is not null) entity.Title = dto.Title;
		if (dto.Subtitle is not null) entity.Subtitle = dto.Subtitle;
		if (dto.Bio is not null) entity.Bio = dto.Bio;
		if (dto.PhoneNumber is not null) entity.PhoneNumber = dto.PhoneNumber;
		if (dto.Suspend is not null) entity.Suspend = dto.Suspend;
		if (dto.BirthDate is not null) entity.Birthdate = dto.BirthDate;
		if (dto.Gender is not null) entity.Gender = dto.Gender;
		if (dto.Email is not null) entity.Email = dto.Email;
		if (dto.Country is not null) entity.Country = dto.Country;
		if (dto.State is not null) entity.State = dto.State;
		if (dto.City is not null) entity.City = dto.City;
		if (dto.Tags is not null) entity.Tags = dto.Tags;
		if (dto.Password is not null) entity.Password = dto.Password;
		if (dto.PremiumExpireDate is not null) entity.PremiumExpireDate = dto.PremiumExpireDate;
		if (dto.Instagram is not null) entity.JsonDetail.Instagram = dto.Instagram;
		if (dto.Telegram is not null) entity.JsonDetail.Telegram = dto.Telegram;
		if (dto.WhatsApp is not null) entity.JsonDetail.WhatsApp = dto.WhatsApp;
		if (dto.LinkedIn is not null) entity.JsonDetail.LinkedIn = dto.LinkedIn;
		if (dto.Dribble is not null) entity.JsonDetail.Dribble = dto.Dribble;
		if (dto.SoundCloud is not null) entity.JsonDetail.SoundCloud = dto.SoundCloud;
		if (dto.Address is not null) entity.JsonDetail.Address = dto.Address;
		if (dto.Pinterest is not null) entity.JsonDetail.Pinterest = dto.Pinterest;
		if (dto.Website is not null) entity.JsonDetail.Website = dto.Website;
		if (dto.FatherName is not null) entity.JsonDetail.FatherName = dto.FatherName;
		if (dto.NationalCode is not null) entity.JsonDetail.NationalCode = dto.NationalCode;
		if (dto.SchoolName is not null) entity.JsonDetail.SchoolName = dto.SchoolName;
		if (dto.Height is not null) entity.JsonDetail.Height = dto.Height;
		if (dto.Weight is not null) entity.JsonDetail.Weight = dto.Weight;
		if (dto.FoodAllergies is not null) entity.JsonDetail.FoodAllergies = dto.FoodAllergies;
		if (dto.Sickness is not null) entity.JsonDetail.Sickness = dto.Sickness;
		if (dto.UsedDrugs is not null) entity.JsonDetail.UsedDrugs = dto.UsedDrugs;
		if (dto.Color is not null) entity.JsonDetail.Color = dto.Color;
		if (dto.FcmToken is not null) entity.JsonDetail.FcmToken = dto.FcmToken;
		if (dto.RegistrationNumber is not null) entity.JsonDetail.RegistrationNumber = dto.RegistrationNumber;
		if (dto.PassportNumber is not null) entity.JsonDetail.PassportNumber = dto.PassportNumber;
		if (dto.ShebaNumber is not null) entity.JsonDetail.ShebaNumber = dto.ShebaNumber;
		if (dto.Code is not null) entity.JsonDetail.Code = dto.Code;
		if (dto.StringList is not null) entity.JsonDetail.StringList = dto.StringList;
		if (dto.PostalCode is not null) entity.JsonDetail.PostalCode = dto.PostalCode;
		if (dto.LandlinePhone is not null) entity.JsonDetail.LandlinePhone = dto.LandlinePhone;
		if (dto.HealthReport1 is not null) entity.JsonDetail.HealthReport1 = dto.HealthReport1;
		if (dto.HealthReport2 is not null) entity.JsonDetail.HealthReport2 = dto.HealthReport2;

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> list = [];
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (e != null) list.Add(e);
			}

			entity.Categories = list;
		}
	}

	private async Task<bool> SendOtp(string userId) {
		if (await cache.GetStringAsync(userId) != null) return false;

		string newOtp = Random.Shared.Next(1000, 9999).ToString();
		string? cachedData = await cache.GetStringAsync(userId);
		if (cachedData.IsNullOrEmpty()) cache.SetStringData(userId, newOtp, TimeSpan.FromSeconds(120));

		UserEntity user = (await dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId || u.UserName == userId))!;

		await sms.SendSms(user.PhoneNumber!, AppSettings.Settings.SmsPanelSettings.PatternCode!, newOtp);
		await dbContext.SaveChangesAsync();
		return true;
	}

	private static JwtSecurityToken CreateToken(UserEntity user) => new(
		issuer: "https://SinaMN75.com,BetterSoft1234",
		audience: "https://SinaMN75.com,BetterSoft1234",
		expires: DateTime.UtcNow.AddMinutes(10),
		claims: [
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.Id),
			new Claim(ClaimTypes.Email, user.Email ?? user.Id),
			new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? user.Id),
		],
		signingCredentials: new SigningCredentials(
			new SymmetricSecurityKey("https://SinaMN75.com,BetterSoft1234"u8.ToArray()),
			SecurityAlgorithms.HmacSha256
		)
	);
}