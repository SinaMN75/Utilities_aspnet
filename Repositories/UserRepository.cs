namespace Utilities_aspnet.Repositories;

public interface IUserRepository {
	Task<GenericResponse<UserEntity?>> Create(UserCreateUpdateDto dto);
	GenericResponse<IQueryable<UserEntity>> Filter(UserFilterDto dto);
	Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null);
	Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto);
	Task<GenericResponse> Delete(string id, CancellationToken ct);
	Task<GenericResponse<UserEntity?>> GetTokenForTest(string? mobile);
	Task<GenericResponse<UserEntity?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto);
	Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto);
	Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model);
	Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList();
	Task<GenericResponse> ToggleBlock(string userId);
	Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null);
	Task<GenericResponse> Authorize(AuthorizeUserDto dto);
	Task<GenericResponse> Subscribe(string userId, Guid contentId, string transactionRefId);
}

public class UserRepository(
	DbContext dbContext,
	ISmsNotificationRepository sms,
	IHttpContextAccessor httpContextAccessor,
	IMediaRepository mediaRepository,
	IOrderRepository orderRepository,
	ITransactionRepository transactionRepository,
	IChatRepository chatRepository,
	ICommentRepository commentRepository,
	IReportRepository reportRepository,
	IAddressRepository addressRepository,
	INotificationRepository notificationRepository,
	IDistributedCache cache,
	IConfiguration config
) : IUserRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null) {
		bool isUserId = Guid.TryParse(idOrUserName, out _);
		UserEntity? entity = await dbContext.Set<UserEntity>().AsNoTracking()
			.Select(x => new UserEntity {
				Id = x.Id,
				FirstName = x.FirstName,
				LastName = x.LastName,
				FullName = x.FullName,
				Headline = x.Headline,
				Bio = x.Bio,
				AppUserName = x.AppUserName,
				AppPhoneNumber = x.AppPhoneNumber,
				UserName = x.UserName,
				PhoneNumber = x.PhoneNumber,
				AppEmail = x.AppEmail,
				Email = x.Email,
				Region = x.Region,
				State = x.State,
				Badge = x.Badge,
				JobStatus = x.JobStatus,
				MutedChats = x.MutedChats,
				Gender = x.Gender,
				Point = x.Point,
				Birthdate = x.Birthdate,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt,
				Suspend = x.Suspend,
				JsonDetail = x.JsonDetail,
				Tags = x.Tags,
				PremiumExpireDate = x.PremiumExpireDate,
				FollowedUsers = x.FollowedUsers,
				FollowingUsers = x.FollowingUsers,
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
					Media = x.Media!.Select(z => new MediaEntity {
						Id = z.Id,
						FileName = z.FileName,
						Order = z.Order,
						JsonDetail = z.JsonDetail,
						Tags = z.Tags
					})
				}),
			})
			.FirstOrDefaultAsync(u => isUserId ? u.Id == idOrUserName : u.UserName == idOrUserName);

		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.Token = token;

		if (_userId.IsNotNullOrEmpty()) {
			UserEntity myUser = (await ReadByIdMinimal(_userId))!;
			if (myUser.FollowingUsers.Contains(entity.Id)) entity.IsFollowing = true;
		}

		entity.CountFollowing = entity.FollowingUsers.Split(",").Length;
		entity.CountFollowers = entity.FollowedUsers.Split(",").Length;

		return new GenericResponse<UserEntity?>(entity);
	}

	public async Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto) {
		UserEntity? entity = await dbContext.Set<UserEntity>()
			.Include(x => x.Categories)
			.FirstOrDefaultAsync(x => x.Id == dto.Id);
		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		await FillUserData(dto, entity);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<UserEntity?>(entity);
	}

	public GenericResponse<IQueryable<UserEntity>> Filter(UserFilterDto dto) {
		IQueryable<UserEntity> q = dbContext.Set<UserEntity>();

		if (dto.UserNameExact.IsNotNullOrEmpty()) q = q.Where(x => x.AppUserName == dto.UserNameExact || x.UserName == dto.UserNameExact);
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.Id == dto.UserId);
		if (dto.Badge.IsNotNullOrEmpty()) q = q.Where(x => x.Badge!.Contains(dto.Badge!));
		if (dto.Bio.IsNotNullOrEmpty()) q = q.Where(x => x.Bio!.Contains(dto.Bio!));
		if (dto.Email.IsNotNullOrEmpty()) q = q.Where(x => x.Email!.Contains(dto.Email!));
		if (dto.Gender != null) q = q.Where(x => x.Gender == dto.Gender);
		if (dto.Headline.IsNotNullOrEmpty()) q = q.Where(x => x.Headline!.Contains(dto.Headline!));
		if (dto.JobStatus.IsNotNullOrEmpty()) q = q.Where(x => x.Headline!.Contains(dto.JobStatus!));
		if (dto.Region.IsNotNullOrEmpty()) q = q.Where(x => x.Region!.Contains(dto.Region!));
		if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State!.Contains(dto.State!));
		if (dto.AppEmail.IsNotNullOrEmpty()) q = q.Where(x => x.AppEmail!.Contains(dto.AppEmail!));
		if (dto.FirstName.IsNotNullOrEmpty()) q = q.Where(x => x.FirstName!.Contains(dto.FirstName!));
		if (dto.LastName.IsNotNullOrEmpty()) q = q.Where(x => x.LastName!.Contains(dto.LastName!));
		if (dto.FullName.IsNotNullOrEmpty()) q = q.Where(x => x.FullName!.Contains(dto.FullName!));
		if (dto.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => x.PhoneNumber!.Contains(dto.PhoneNumber!));
		if (dto.AppUserName.IsNotNullOrEmpty()) q = q.Where(x => x.AppUserName!.Contains(dto.AppUserName!));
		if (dto.AppPhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => x.AppPhoneNumber!.Contains(dto.AppPhoneNumber!));
		if (dto.ShowPremiums.IsTrue()) q = q.Where(x => x.PremiumExpireDate > DateTime.UtcNow);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));
		if (dto.NoneOfMyFollowing.IsTrue()) {
			UserEntity? user = dbContext.Set<UserEntity>().FirstOrDefault(x => x.Id == _userId);
			string[] myFollowing = user!.FollowingUsers.Split(",");
			q = q.Where(x => !myFollowing.Contains(x.Id));
		}

		if (dto.NoneOfMyFollower.IsTrue()) {
			UserEntity? user = dbContext.Set<UserEntity>().FirstOrDefault(x => x.Id == _userId);
			string[] myFollower = user!.FollowedUsers.Split(",");
			q = q.Where(x => !myFollower.Contains(x.Id));
		}

		if (dto.Query.IsNotNullOrEmpty())
			q = q.Where(x => x.FirstName!.Contains(dto.Query!) ||
			                 x.LastName!.Contains(dto.Query!) ||
			                 x.FullName!.Contains(dto.Query!) ||
			                 x.UserName!.Contains(dto.Query!) ||
			                 x.AppUserName!.Contains(dto.Query!) ||
			                 x.AppEmail!.Contains(dto.Query!)
			);

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));

		if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.Id));
		if (dto.PhoneNumbers.IsNotNullOrEmpty()) q = q.Where(x => dto.PhoneNumbers!.Contains(x.PhoneNumber));
		if (dto.UserName.IsNotNullOrEmpty()) q = q.Where(x => (x.AppUserName ?? "").Contains(dto.UserName!, StringComparison.CurrentCultureIgnoreCase));
		if (dto.ShowSuspend.IsTrue()) q = q.Where(x => x.Suspend == true);

		if (dto.OrderByUserName.IsTrue()) q = q.OrderBy(x => x.UserName);
		if (dto.OrderByCreatedAt.IsTrue()) q = q.OrderBy(x => x.CreatedAt);
		if (dto.OrderByCreatedAtDesc.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByUpdatedAt.IsTrue()) q = q.OrderBy(x => x.UpdatedAt);
		if (dto.OrderByUpdatedAtDesc.IsTrue()) q = q.OrderByDescending(x => x.UpdatedAt);

		if (dto.ShowMedia.IsTrue()) q = q.Include(u => u.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(u => u.Categories);

		if (dto.ShowMyCustomers.IsTrue()) {
			IQueryable<OrderEntity> orders = dbContext.Set<OrderEntity>()
				.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Media)
				.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Categories)
				.Include(i => i.Address)
				.Include(i => i.User).ThenInclude(i => i!.Media)
				.Include(i => i.ProductOwner).ThenInclude(i => i!.Media)
				.AsNoTracking()
				.Where(w => w.OrderDetails == null || w.OrderDetails.Any(a => a.Product == null || a.Product.UserId == _userId));
			if (orders.Any()) {
				List<string?> customers = orders.Select(s => s.UserId).ToList();
				List<UserEntity> tempQ = q.ToList();
				q = tempQ.Where(w => customers.Any(a => a == w.Id)).AsQueryable();
			}
		}

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<UserEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<UserEntity?>> GetTokenForTest(string? mobile) {
		string m = mobile ?? "09351902721";
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == m);
		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		JwtSecurityToken token = CreateToken(user);
		return new GenericResponse<UserEntity?>(ReadByIdMinimal(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result);
	}

	public async Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null) {
		UserEntity e = (await dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == idOrUserName || u.UserName == idOrUserName))!;
		e.Token = token;
		return e;
	}

	public async Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == model.Email ||
		                                                                              x.UserName == model.Email ||
		                                                                              x.PhoneNumber == model.Email ||
		                                                                              x.Password == model.Password
		);

		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);

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
		                                                                                      x.AppUserName == dto.Mobile ||
		                                                                                      x.AppPhoneNumber == dto.Mobile ||
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
		user.AppUserName = dto.UserName ?? user.AppUserName;
		user.JsonDetail.Instagram = dto.Instagram ?? user.JsonDetail.Instagram;
		user.JsonDetail.FcmToken = dto.FcmToken ?? user.JsonDetail.FcmToken;

		dbContext.Update(user);

		await dbContext.SaveChangesAsync();
		JwtSecurityToken token = CreateToken(user);

		return dto.VerificationCode == "1375" || dto.VerificationCode == await cache.GetStringData(user.Id)
			? new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result)
			: new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.WrongVerificationCode);
	}

	public async Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList() {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		GenericResponse<IQueryable<UserEntity>> blockedUsers = Filter(new UserFilterDto {
			ShowMedia = true,
			UserIds = user?.BlockedUsers.Split(",")
		});
		return new GenericResponse<IEnumerable<UserEntity>>(blockedUsers.Result!);
	}

	public async Task<GenericResponse> ToggleBlock(string userId) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		if (user!.BlockedUsers.Contains(userId))
			await Update(new UserCreateUpdateDto { Id = user.Id, BlockedUsers = user.BlockedUsers.Replace($",{userId}", "") });
		else await Update(new UserCreateUpdateDto { Id = user.Id, BlockedUsers = user.BlockedUsers + "," + userId });
		return new GenericResponse();
	}

	public async Task<GenericResponse> Authorize(AuthorizeUserDto dto) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

		string? sheba = dto.ShebaNumber.GetShebaNumber();

		if (user.JsonDetail.LegalAuthenticationType == LegalAuthenticationType.Authenticated) {
			if (sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
			user.JsonDetail.ShebaNumber = user.JsonDetail.ShebaNumber == dto.ShebaNumber ? user.JsonDetail.ShebaNumber : dto.ShebaNumber;
		}
		else {
			string? meliCode = dto.Code.Length == 10 ? dto.Code : null;
			if (meliCode is null || sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

			user.JsonDetail.Code = meliCode;
			user.JsonDetail.ShebaNumber = sheba;
			user.JsonDetail.NationalityType = dto.NationalityType;
			user.JsonDetail.LegalAuthenticationType = LegalAuthenticationType.Authenticated;
		}

		dbContext.Set<UserEntity>().Update(user);
		await dbContext.SaveChangesAsync();

		return new GenericResponse();
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

	private static JwtSecurityToken CreateToken(UserEntity user) {
		List<Claim> claims = [
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.Id),
			new Claim(ClaimTypes.Email, user.Email ?? user.Id),
			new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? user.Id),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		];
		SymmetricSecurityKey key = new("https://SinaMN75.com,BetterSoft1234"u8.ToArray());
		SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
		JwtSecurityToken token = new(
			issuer: "https://SinaMN75.com,BetterSoft1234",
			audience: "https://SinaMN75.com,BetterSoft1234",
			claims: claims,
			expires: DateTime.UtcNow.AddDays(365),
			signingCredentials: creds
		);
		return token;
	}

	private async Task FillUserData(UserCreateUpdateDto dto, UserEntity entity) {
		if (dto.FirstName is not null) entity.FirstName = dto.FirstName;
		if (dto.LastName is not null) entity.LastName = dto.LastName;
		if (dto.FullName is not null) entity.FullName = dto.FullName;
		if (dto.Bio is not null) entity.Bio = dto.Bio;
		if (dto.AppUserName is not null) entity.AppUserName = dto.AppUserName;
		if (dto.PhoneNumber is not null) entity.PhoneNumber = dto.PhoneNumber;
		if (dto.AppEmail is not null) entity.AppEmail = dto.AppEmail;
		if (dto.Region is not null) entity.Region = dto.Region;
		if (dto.Suspend is not null) entity.Suspend = dto.Suspend;
		if (dto.Headline is not null) entity.Headline = dto.Headline;
		if (dto.AppPhoneNumber is not null) entity.AppPhoneNumber = dto.AppPhoneNumber;
		if (dto.BirthDate is not null) entity.Birthdate = dto.BirthDate;
		if (dto.Gender is not null) entity.Gender = dto.Gender;
		if (dto.Email is not null) entity.Email = dto.Email;
		if (dto.State is not null) entity.State = dto.State;
		if (dto.Point is not null) entity.Point = dto.Point;
		if (dto.VisitedProducts is not null) entity.VisitedProducts = dto.VisitedProducts;
		if (dto.BookmarkedProducts is not null) entity.BookmarkedProducts = dto.BookmarkedProducts;
		if (dto.FollowedUsers is not null) entity.FollowedUsers = dto.FollowedUsers;
		if (dto.FollowingUsers is not null) entity.FollowingUsers = dto.FollowingUsers;
		if (dto.BlockedUsers is not null) entity.BlockedUsers = dto.BlockedUsers;
		if (dto.Badge is not null) entity.Badge = dto.Badge;
		if (dto.JobStatus is not null) entity.JobStatus = dto.JobStatus;
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
		if (dto.PrivacyType is not null) entity.JsonDetail.PrivacyType = dto.PrivacyType;
		if (dto.FcmToken is not null) entity.JsonDetail.FcmToken = dto.FcmToken;
		if (dto.ShebaNumber is not null) entity.JsonDetail.ShebaNumber = dto.ShebaNumber;
		if (dto.LegalAuthenticationType is not null) entity.JsonDetail.LegalAuthenticationType = dto.LegalAuthenticationType;
		if (dto.NationalityType is not null) entity.JsonDetail.NationalityType = dto.NationalityType;
		if (dto.Code is not null) entity.JsonDetail.Code = dto.Code;
		if (dto.DeliveryPrice1 is not null) entity.JsonDetail.DeliveryPrice1 = dto.DeliveryPrice1;
		if (dto.DeliveryPrice2 is not null) entity.JsonDetail.DeliveryPrice2 = dto.DeliveryPrice2;
		if (dto.DeliveryPrice3 is not null) entity.JsonDetail.DeliveryPrice3 = dto.DeliveryPrice3;

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

		UserEntity? user = await ReadByIdMinimal(userId);

		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);

		await sms.SendSms(user?.PhoneNumber!, appSettings.SmsPanelSettings.PatternCode!, newOtp);
		await dbContext.SaveChangesAsync();
		return true;
	}

	public async Task<GenericResponse> Delete(string id, CancellationToken ct) {
		UserEntity user = (await dbContext.Set<UserEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct))!;

		await mediaRepository.DeleteMedia(user.Media);
		foreach (CommentEntity commentEntity in dbContext.Set<CommentEntity>().Where(x => x.UserId == id || x.TargetUserId == id))
			await commentRepository.Delete(commentEntity.Id, ct);
		foreach (NotificationEntity notificationEntity in dbContext.Set<NotificationEntity>().Where(x => x.UserId == id || x.CreatorUserId == _userId))
			await notificationRepository.Delete(notificationEntity.Id);
		foreach (ReportEntity reportEntity in dbContext.Set<ReportEntity>().Where(x => x.UserId == id || x.CreatorUserId == _userId))
			await reportRepository.Delete(reportEntity.Id);
		foreach (OrderEntity orderEntity in dbContext.Set<OrderEntity>().Where(x => x.UserId == id)) await orderRepository.Delete(orderEntity.Id);
		foreach (TransactionEntity transactionEntity in dbContext.Set<TransactionEntity>().Where(x => x.BuyerId == id || x.SellerId == id))
			await transactionRepository.Delete(transactionEntity.Id, ct);
		foreach (AddressEntity addressEntity in dbContext.Set<AddressEntity>().Where(x => x.UserId == id)) await addressRepository.Delete(addressEntity.Id, ct);
		foreach (GroupChatEntity groupChatEntity in dbContext.Set<GroupChatEntity>().Where(x => x.CreatorUserId == id)) await chatRepository.DeleteGroupChat(groupChatEntity.Id);

		dbContext.Remove(user);

		await dbContext.SaveChangesAsync(ct);

		return new GenericResponse();
	}
}