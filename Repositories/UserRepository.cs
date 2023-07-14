namespace Utilities_aspnet.Repositories;

public interface IUserRepository {
	GenericResponse<IQueryable<UserEntity>> Filter(UserFilterDto dto);
	Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null);
	Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto);
	Task<GenericResponse<UserEntity?>> GetTokenForTest(string? mobile);
	Task<GenericResponse<UserEntity?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto);
	Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto);
	Task<GenericResponse<UserEntity?>> Register(RegisterDto dto);
	Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model);
	Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList();
	Task<GenericResponse> ToggleBlock(string userId);
	Task<GenericResponse> TransferWalletToWallet(TransferFromWalletToWalletDto dto, CancellationToken ct);
	Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null);
	Task<GenericResponse> Authorize(AuthorizeUserDto dto);
}

public class UserRepository : IUserRepository {
	private readonly DbContext _dbContext;
	private readonly IMemoryCache _memoryCache;
	private readonly ISmsNotificationRepository _sms;
	private readonly ITransactionRepository _transactionRepository;
	private readonly string? _userId;

	public UserRepository(
		DbContext dbContext,
		ISmsNotificationRepository sms,
		IHttpContextAccessor httpContextAccessor,
		ITransactionRepository transactionRepository,
		IMemoryCache memoryCache) {
		_dbContext = dbContext;
		_sms = sms;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
		_transactionRepository = transactionRepository;
		_memoryCache = memoryCache;
	}

	public async Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null) {
		bool isUserId = Guid.TryParse(idOrUserName, out _);
		UserEntity? entity = await _dbContext.Set<UserEntity>()
			.Include(u => u.Media)
			.Include(u => u.Categories)!.ThenInclude(u => u.Media)
			.FirstOrDefaultAsync(u => isUserId ? u.Id == idOrUserName : u.UserName == idOrUserName);

		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		entity.Token = token;

		if (_userId.IsNotNullOrEmpty()) {
			UserEntity myUser = (await ReadByIdMinimal(_userId))!;
			if (myUser.FollowingUsers.Contains(entity.Id)) entity.IsFollowing = true;
		}

		foreach (string i in entity.FollowingUsers.Split(","))
			if (i.Length >= 10)
				entity.CountFollowing += 1;
		foreach (string i in entity.FollowedUsers.Split(","))
			if (i.Length >= 10)
				entity.CountFollowers += 1;

		return new GenericResponse<UserEntity?>(entity);
	}

	public async Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto) {
		UserEntity? entity = await _dbContext.Set<UserEntity>().Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == dto.Id);
		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		FillUserData(dto, entity);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<UserEntity?>(entity);
	}

	public GenericResponse<IQueryable<UserEntity>> Filter(UserFilterDto dto) {
		IQueryable<UserEntity> q = _dbContext.Set<UserEntity>();

		if (dto.UserNameExact != null) q = q.Where(x => x.AppUserName == dto.UserNameExact || x.UserName == dto.UserNameExact);
		if (dto.UserId != null) q = q.Where(x => x.Id == dto.UserId);
		if (dto.Badge != null) q = q.Where(x => x.Badge!.Contains(dto.Badge));
		if (dto.Bio != null) q = q.Where(x => x.Bio!.Contains(dto.Bio));
		if (dto.Email != null) q = q.Where(x => x.Email!.Contains(dto.Email));
		if (dto.Gender != null) q = q.Where(x => x.Gender == dto.Gender);
		if (dto.Headline != null) q = q.Where(x => x.Headline!.Contains(dto.Headline));
		if (dto.JobStatus != null) q = q.Where(x => x.Headline!.Contains(dto.JobStatus));
		if (dto.Region != null) q = q.Where(x => x.Region!.Contains(dto.Region));
		if (dto.State != null) q = q.Where(x => x.State!.Contains(dto.State));
		if (dto.AppEmail != null) q = q.Where(x => x.AppEmail!.Contains(dto.AppEmail));
		if (dto.FirstName != null) q = q.Where(x => x.FirstName!.Contains(dto.FirstName));
		if (dto.LastName != null) q = q.Where(x => x.LastName!.Contains(dto.LastName));
		if (dto.FullName != null) q = q.Where(x => x.FullName!.Contains(dto.FullName));
		if (dto.PhoneNumber != null) q = q.Where(x => x.PhoneNumber!.Contains(dto.PhoneNumber));
		if (dto.AppUserName != null) q = q.Where(x => x.AppUserName!.Contains(dto.AppUserName));
		if (dto.AppPhoneNumber != null) q = q.Where(x => x.AppPhoneNumber!.Contains(dto.AppPhoneNumber));

		if (dto.Query != null)
			q = q.Where(x => x.FirstName!.Contains(dto.Query) ||
			                 x.LastName!.Contains(dto.Query) ||
			                 x.FullName!.Contains(dto.Query) ||
			                 x.UserName!.Contains(dto.Query) ||
			                 x.AppUserName!.Contains(dto.Query) ||
			                 x.AppEmail!.Contains(dto.Query)
			);

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));

		if (dto.UserIds != null) q = q.Where(x => dto.UserIds.Contains(x.Id));
		if (dto.PhoneNumbers != null) q = q.Where(x => dto.PhoneNumbers.Contains(x.PhoneNumber));
		if (dto.UserName != null) q = q.Where(x => (x.AppUserName ?? "").ToLower().Contains(dto.UserName.ToLower()));
		if (dto.ShowSuspend.IsTrue()) q = q.Where(x => x.Suspend == true);

		if (dto.OrderByUserName.IsTrue()) q = q.OrderBy(x => x.UserName);

		if (dto.ShowMedia.IsTrue()) q = q.Include(u => u.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(u => u.Categories);

		if (dto.ShowMyCustomers.IsTrue()) {
			IQueryable<OrderEntity> orders = _dbContext.Set<OrderEntity>()
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
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == m);
		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);
		JwtSecurityToken token = CreateToken(user);
		return new GenericResponse<UserEntity?>(ReadByIdMinimal(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result);
	}

	public async Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null) {
		UserEntity e = (await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == idOrUserName || u.UserName == idOrUserName))!;
		e.Token = token;
		return e;
	}

	public async Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model) {
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == model.Email ||
		                                                                               x.UserName == model.Email ||
		                                                                               x.PhoneNumber == model.Email ||
		                                                                               x.Password == model.Password);

		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);

		await _dbContext.SaveChangesAsync();
		JwtSecurityToken token = CreateToken(user);

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<UserEntity?>> Register(RegisterDto dto) {
		UserEntity? model = await _dbContext.Set<UserEntity>()
			.FirstOrDefaultAsync(x => x.UserName == dto.UserName || x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber);
		if (model != null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserAlreadyExist);

		UserEntity user = new() {
			Email = dto.Email ?? "",
			UserName = dto.UserName ?? dto.Email ?? dto.PhoneNumber,
			PhoneNumber = dto.PhoneNumber,
			FullName = "",
			Wallet = 0,
			Suspend = false,
			Password = dto.Password,
			FirstName = dto.FirstName,
			LastName = dto.LastName,
			JsonDetail = dto.JsonDetail = new UserJsonDetail()
		};

		await _dbContext.AddAsync(user);
		await _dbContext.SaveChangesAsync();

		JwtSecurityToken token = CreateToken(user);

		if (dto.SendSms) {
			if (dto.Email != null && dto.Email.IsEmail()) { }
			else { await SendOtp(user.Id); }
		}

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<UserEntity?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto) {
		//string salt = $"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}SinaMN75";
		//bool isOk = dto.token == Encryption.GetMd5HashData(salt).ToLower();
		//if (!isOk) return new GenericResponse<string?>("Unauthorized", UtilitiesStatusCodes.Unhandled);

		string mobile = dto.Mobile.DeleteAdditionsInsteadNumber();
		mobile = mobile.GetLast(10);
		mobile = mobile.Insert(0, "0");
		if (mobile.Length is > 12 or < 9) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.BadRequest);
		UserEntity? existingUser = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == mobile ||
		                                                                                       x.PhoneNumber == mobile ||
		                                                                                       x.AppUserName == mobile ||
		                                                                                       x.AppPhoneNumber == mobile ||
		                                                                                       x.UserName == mobile);

		if (existingUser != null) {
			if (!await SendOtp(existingUser.Id)) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.MaximumLimitReached);
			return new GenericResponse<UserEntity?>(existingUser);
		}
		UserEntity user = new() {
			PhoneNumber = mobile,
			UserName = mobile
		};

		await _dbContext.AddAsync(user);
		await _dbContext.SaveChangesAsync();
		await SendOtp(user.Id);
		return new GenericResponse<UserEntity?>(user);
	}

	public async Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) {
		string mobile = dto.Mobile.Replace("+", "");
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile || x.Email == mobile);
		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserNotFound);
		if (user.Suspend ?? false) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserSuspended);

		await _dbContext.SaveChangesAsync();
		JwtSecurityToken token = CreateToken(user);

		return dto.VerificationCode == "1375" || dto.VerificationCode == _memoryCache.Get<string>(user.Id)
			? new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result)
			: new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.WrongVerificationCode);
	}

	public async Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList() {
		UserEntity? user = await ReadByIdMinimal(_userId);
		GenericResponse<IQueryable<UserEntity>> blockedUsers = Filter(new UserFilterDto {
			ShowMedia = true,
			UserIds = user?.BlockedUsers.Split(",")
		});
		return new GenericResponse<IEnumerable<UserEntity>>(blockedUsers.Result!);
	}

	public async Task<GenericResponse> ToggleBlock(string userId) {
		UserEntity? user = await ReadByIdMinimal(_userId);

		await Update(new UserCreateUpdateDto { Id = user!.Id, BlockedUsers = user.BlockedUsers + "," + userId });
		if (user.BlockedUsers.Contains(userId))
			await Update(new UserCreateUpdateDto { Id = user.Id, BlockedUsers = user.BlockedUsers.Replace($",{userId}", "") });
		else await Update(new UserCreateUpdateDto { Id = user.Id, BlockedUsers = user.BlockedUsers + "," + userId });
		return new GenericResponse();
	}

	public async Task<GenericResponse> TransferWalletToWallet(TransferFromWalletToWalletDto dto, CancellationToken ct) {
		UserEntity fromUser = (await ReadByIdMinimal(dto.FromUserId))!;
		UserEntity toUser = (await ReadByIdMinimal(dto.ToUserId))!;

		if (fromUser.Wallet <= dto.Amount) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);
		await Update(new UserCreateUpdateDto { Id = fromUser.Id, Wallet = fromUser.Wallet - dto.Amount });
		await _transactionRepository.Create(MakeTransaction(fromUser.Id, dto.Amount, "کسر", null, TransactionType.WithdrawFromTheWallet), ct);
		await Update(new UserCreateUpdateDto { Id = toUser.Id, Wallet = toUser.Wallet + dto.Amount });
		await _transactionRepository.Create(MakeTransaction(toUser.Id, dto.Amount, "واریز", null, TransactionType.DepositToWallet), ct);
		return new GenericResponse();
	}

	public async Task<GenericResponse> Authorize(AuthorizeUserDto dto) {
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
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

		_dbContext.Set<UserEntity>().Update(user);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse();
	}

	private static JwtSecurityToken CreateToken(UserEntity user) {
		List<Claim> claims = new() {
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.Id),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		SymmetricSecurityKey key = new("https://SinaMN75.com"u8.ToArray());
		SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
		JwtSecurityToken token = new("https://SinaMN75.com", "https://SinaMN75.com", claims, expires: DateTime.Now.AddDays(365), signingCredentials: creds);
		return token;
	}

	private void FillUserData(UserCreateUpdateDto dto, UserEntity entity) {
		entity.FirstName = dto.FirstName ?? entity.FirstName;
		entity.LastName = dto.LastName ?? entity.LastName;
		entity.FullName = dto.FullName ?? entity.FullName;
		entity.Bio = dto.Bio ?? entity.Bio;
		entity.AppUserName = dto.AppUserName ?? entity.AppUserName;
		entity.AppEmail = dto.AppEmail ?? entity.AppEmail;
		entity.Region = dto.Region ?? entity.Region;
		entity.Suspend = dto.Suspend ?? entity.Suspend;
		entity.Headline = dto.Headline ?? entity.Headline;
		entity.AppPhoneNumber = dto.AppPhoneNumber ?? entity.AppPhoneNumber;
		entity.Birthdate = dto.BirthDate ?? entity.Birthdate;
		entity.Wallet = dto.Wallet ?? entity.Wallet;
		entity.Gender = dto.Gender ?? entity.Gender;
		entity.Email = dto.Email ?? entity.Email;
		entity.State = dto.State ?? entity.State;
		entity.Point = dto.Point ?? entity.Point;
		entity.VisitedProducts = dto.VisitedProducts ?? entity.VisitedProducts;
		entity.BookmarkedProducts = dto.BookmarkedProducts ?? entity.BookmarkedProducts;
		entity.FollowingUsers = dto.FollowingUsers ?? entity.FollowingUsers;
		entity.FollowedUsers = dto.FollowedUsers ?? entity.FollowedUsers;
		entity.BlockedUsers = dto.BlockedUsers ?? entity.BlockedUsers;
		entity.Badge = dto.Badge ?? entity.Badge;
		entity.JobStatus = dto.JobStatus ?? entity.JobStatus;
		entity.UpdatedAt = DateTime.Now;
		entity.IsOnline = dto.IsOnline ?? entity.IsOnline;
		entity.ExpireUpgradeAccount = dto.ExpireUpgradeAccount ?? entity.ExpireUpgradeAccount;
		entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;
		entity.JsonDetail = new UserJsonDetail {
			Instagram = dto.Instagram ?? entity.JsonDetail.Instagram,
			Telegram = dto.Telegram ?? entity.JsonDetail.Telegram,
			WhatsApp = dto.WhatsApp ?? entity.JsonDetail.WhatsApp,
			LinkedIn = dto.LinkedIn ?? entity.JsonDetail.LinkedIn,
			Dribble = dto.Dribble ?? entity.JsonDetail.Dribble,
			SoundCloud = dto.SoundCloud ?? entity.JsonDetail.SoundCloud,
			Pinterest = dto.Pinterest ?? entity.JsonDetail.Pinterest,
			Website = dto.Website ?? entity.JsonDetail.Website,
			Activity = dto.Activity ?? entity.JsonDetail.Activity,
			Color = dto.Color ?? entity.JsonDetail.Color,
			PrivacyType = dto.PrivacyType ?? entity.JsonDetail.PrivacyType,
			ShebaNumber = dto.ShebaNumber,
			LegalAuthenticationType = dto.LegalAuthenticationType ?? entity.JsonDetail.LegalAuthenticationType,
			NationalityType = dto.NationalityType ?? entity.JsonDetail.NationalityType,
			Code = dto.Code ?? entity.JsonDetail.Code
		};

		if (dto.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = new();
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item).Result;
				if (e != null) list.Add(e);
			}

			entity.Categories = list;
		}
	}

	private static TransactionEntity MakeTransaction(string userId, int amount, string description, string? shebaNumber, TransactionType type) => new() {
		UserId = userId,
		Amount = amount,
		Descriptions = description,
		TransactionType = type,
		ShebaNumber = shebaNumber
	};

	private async Task<bool> SendOtp(string userId) {
		if (_memoryCache.Get<string>(userId) != null) return false;

		string newOtp = Random.Shared.Next(1000, 9999).ToString();
		_memoryCache.GetOrCreate<string>(userId, entry => {
			entry.Value = newOtp;
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120);
			return newOtp;
		});
		UserEntity? user = await ReadByIdMinimal(userId);
		_sms.SendSms(user?.PhoneNumber!, newOtp);
		await _dbContext.SaveChangesAsync();
		return true;
	}
}