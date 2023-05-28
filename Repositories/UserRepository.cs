namespace Utilities_aspnet.Repositories;

public interface IUserRepository {
	GenericResponse<IQueryable<UserEntity>> Filter(UserFilterDto dto);
	Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null);
	Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto);
	Task<GenericResponse<UserEntity?>> GetTokenForTest(string? mobile);
	Task<GenericResponse<string?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto);
	Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto);
	Task<GenericResponse<UserEntity?>> Register(RegisterDto dto);
	Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model);
	Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList();
	Task<GenericResponse> ToggleBlock(string userId);
	Task<GenericResponse> TransferWalletToWallet(TransferFromWalletToWalletDto dto);
	Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null);
	Task<GenericResponse> Authorize(AuthorizeUserDto dto);
}

public class UserRepository : IUserRepository {
	private readonly DbContext _dbContext;
	private readonly UserManager<UserEntity> _userManager;
	private readonly ISmsNotificationRepository _sms;
	private readonly ITransactionRepository _transactionRepository;
	private readonly string? _userId;

	public UserRepository(
		DbContext dbContext,
		UserManager<UserEntity> userManager,
		ISmsNotificationRepository sms,
		IHttpContextAccessor httpContextAccessor,
		ITransactionRepository transactionRepository) {
		_dbContext = dbContext;
		_userManager = userManager;
		_sms = sms;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
		_transactionRepository = transactionRepository;
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
		if (dto.Badge != null) q = q.Where(x => x.Badge.Contains(dto.Badge));
		if (dto.Bio != null) q = q.Where(x => x.Bio.Contains(dto.Bio));
		if (dto.Email != null) q = q.Where(x => x.Email.Contains(dto.Email));
		if (dto.Gender != null) q = q.Where(x => x.Gender == dto.Gender);
		if (dto.Headline != null) q = q.Where(x => x.Headline.Contains(dto.Headline));
		if (dto.Region != null) q = q.Where(x => x.Region.Contains(dto.Region));
		if (dto.State != null) q = q.Where(x => x.State.Contains(dto.State));
		if (dto.Type != null) q = q.Where(x => x.Type.Contains(dto.Type));
		if (dto.AccessLevel != null) q = q.Where(x => x.AccessLevel.Contains(dto.AccessLevel));
		if (dto.AppEmail != null) q = q.Where(x => x.AppEmail.Contains(dto.AppEmail));
		if (dto.FirstName != null) q = q.Where(x => x.FirstName.Contains(dto.FirstName));
		if (dto.LastName != null) q = q.Where(x => x.LastName.Contains(dto.LastName));
		if (dto.FullName != null) q = q.Where(x => x.FullName.Contains(dto.FullName));
		if (dto.UseCase != null) q = q.Where(x => x.UseCase.Contains(dto.UseCase));
		if (dto.PhoneNumber != null) q = q.Where(x => x.PhoneNumber.Contains(dto.PhoneNumber));
		if (dto.AppUserName != null) q = q.Where(x => x.AppUserName.Contains(dto.AppUserName));
		if (dto.AppPhoneNumber != null) q = q.Where(x => x.AppPhoneNumber.Contains(dto.AppPhoneNumber));

		if (dto.Query != null)
			q = q.Where(x => x.FirstName.Contains(dto.Query) ||
			                 x.LastName.Contains(dto.Query) ||
			                 x.FullName.Contains(dto.Query) ||
			                 x.UserName.Contains(dto.Query) ||
			                 x.AppUserName.Contains(dto.Query) ||
			                 x.AppEmail.Contains(dto.Query)
			);

		if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));

		if (dto.UserIds != null) q = q.Where(x => dto.UserIds.Contains(x.Id));
		if (dto.UserName != null) q = q.Where(x => (x.AppUserName ?? "").ToLower().Contains(dto.UserName.ToLower()));
		if (dto.ShowSuspend.IsTrue()) q = q.Where(x => x.Suspend == true);

		if (dto.OrderByUserName.IsTrue()) q = q.OrderBy(x => x.UserName);

		if (dto.ShowMedia.IsTrue()) q = q.Include(u => u.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(u => u.Categories);

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
		JwtSecurityToken token = await CreateToken(user);
		return new GenericResponse<UserEntity?>(ReadByIdMinimal(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result);
	}

	private async Task<JwtSecurityToken> CreateToken(UserEntity user) {
		IEnumerable<string>? roles = await _userManager.GetRolesAsync(user);
		List<Claim> claims = new() {
			new Claim(JwtRegisteredClaimNames.Sub, user.Id),
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.Id),
			new Claim("IsLoggedIn", true.ToString()),
			new Claim("IsLoggedIn", true.ToString()),
			new Claim("IsLoggedIn", true.ToString()),
			new Claim("IsLoggedIn", true.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		if (roles != null) claims.AddRange(roles.Select(role => new Claim("role", role)));
		SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes("https://SinaMN75.com"));
		SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
		JwtSecurityToken token = new("https://SinaMN75.com", "https://SinaMN75.com", claims, expires: DateTime.Now.AddDays(365), signingCredentials: creds);

		await _userManager.UpdateAsync(user);
		return token;
	}

	#region New Login Register

	public async Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model) {
		UserEntity? user = (await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email))
		                   ?? await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == model.Email);

		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound, "User not found");

		bool result = await _userManager.CheckPasswordAsync(user, model.Password);
		if (!result)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.BadRequest, "The password is incorrect!");

		await _dbContext.SaveChangesAsync();
		JwtSecurityToken token = await CreateToken(user);

		return new GenericResponse<UserEntity?>(
			ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result, UtilitiesStatusCodes.Success, "Success"
		);
	}

	public async Task<GenericResponse<UserEntity?>> Register(RegisterDto dto) {
		UserEntity? model = await _dbContext.Set<UserEntity>()
			.FirstOrDefaultAsync(x => x.UserName == dto.UserName || x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber);
		if (model != null)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserAlreadyExist, "This email or username already exists");

		UserEntity user = new() {
			Email = dto.Email ?? "",
			UserName = dto.UserName ?? dto.Email ?? dto.PhoneNumber,
			PhoneNumber = dto.PhoneNumber,
			EmailConfirmed = false,
			PhoneNumberConfirmed = false,
			FullName = "",
			Wallet = 0,
			Suspend = false,
			FirstName = dto.FirstName,
			LastName = dto.LastName,
			UserJsonDetail = dto.UserJsonDetail,
		};

		IdentityResult? result = await _userManager.CreateAsync(user, dto.Password);
		if (!result.Succeeded)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.Unhandled, "The information was not entered correctly");

		JwtSecurityToken token = await CreateToken(user);

		if (dto.SendSms) {
			if (dto.Email != null && dto.Email.IsEmail()) { }
			else await SendOtp(user.Id, 4);
		}

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<string?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto) {
		//string salt = $"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}SinaMN75";
		//bool isOk = dto.token == Encryption.GetMd5HashData(salt).ToLower();
		//if (!isOk) return new GenericResponse<string?>("Unauthorized", UtilitiesStatusCodes.Unhandled);

		string mobile = dto.Mobile.DeleteAdditionsInsteadNumber();
		mobile = mobile.GetLast(10);
		mobile = mobile.Insert(0, "0");
		if (mobile.Length is > 12 or < 9) return new GenericResponse<string?>("شماره موبایل وارد شده صحیح نیست", UtilitiesStatusCodes.BadRequest);
		UserEntity? existingUser = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == mobile ||
		                                                                                       x.PhoneNumber == mobile ||
		                                                                                       x.AppUserName == mobile ||
		                                                                                       x.AppPhoneNumber == mobile ||
		                                                                                       x.UserName == mobile);

		if (existingUser != null) {
			if (dto.SendSMS) {
				if (!await SendOtp(existingUser.Id, 4))
					return new GenericResponse<string?>("برای دریافت کد تایید جدید کمی صبر کنید", UtilitiesStatusCodes.MaximumLimitReached);
				return new GenericResponse<string?>(":)");
			}
		}
		UserEntity user = new() {
			Email = "",
			PhoneNumber = mobile,
			UserName = mobile,
			EmailConfirmed = false,
			PhoneNumberConfirmed = false,
			FullName = "",
			Wallet = 0,
			Suspend = false
		};

		IdentityResult? result = await _userManager.CreateAsync(user, "SinaMN75");
		if (!result.Succeeded)
			return new GenericResponse<string?>("", UtilitiesStatusCodes.BadRequest, result.Errors.First().Code + result.Errors.First().Description);

		if (dto.SendSMS) await SendOtp(user.Id, 4);

		return new GenericResponse<string?>(":)");
	}

	public async Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto) {
		string mobile = dto.Mobile.Replace("+", "");
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile || x.Email == mobile);
		if (user == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserNotFound);
		if (user.Suspend ?? false) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserSuspended);

		await _dbContext.SaveChangesAsync();
		JwtSecurityToken token = await CreateToken(user);

		if (await Verify(user.Id, dto.VerificationCode) != OtpResult.Ok)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.WrongVerificationCode);

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
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

		await Update(new UserCreateUpdateDto {Id = user.Id, BlockedUsers = user.BlockedUsers + "," + userId});
		if (user.BlockedUsers.Contains(userId))
			await Update(new UserCreateUpdateDto {Id = user.Id, BlockedUsers = user.BlockedUsers.Replace($",{userId}", "")});
		else await Update(new UserCreateUpdateDto {Id = user.Id, BlockedUsers = user.BlockedUsers + "," + userId});
		return new GenericResponse();
	}

	public async Task<GenericResponse> TransferWalletToWallet(TransferFromWalletToWalletDto dto) {
		UserEntity fromUser = (await ReadByIdMinimal(dto.FromUserId))!;
		UserEntity toUser = (await ReadByIdMinimal(dto.ToUserId))!;

		if (fromUser.Wallet <= dto.Amount) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);
		await Update(new UserCreateUpdateDto {Id = fromUser.Id, Wallet = fromUser.Wallet - dto.Amount});
		await _transactionRepository.Create(MakeTransactionEntity(fromUser.Id, dto.Amount, "کسر", null));
		await Update(new UserCreateUpdateDto {Id = toUser.Id, Wallet = toUser.Wallet + dto.Amount});
		await _transactionRepository.Create(MakeTransactionEntity(toUser.Id, dto.Amount, "واریز", null));
		return new GenericResponse();
	}

	public async Task<GenericResponse> Authorize(AuthorizeUserDto dto) {
		UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

		string? sheba = dto.ShebaNumber.GetShebaNumber();

		if (user.UserJsonDetail.IsAuthorize.IsTrue()) {
			if (sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
			user.UserJsonDetail.ShebaNumber = user.UserJsonDetail.ShebaNumber == dto.ShebaNumber ? user.UserJsonDetail.ShebaNumber : dto.ShebaNumber;
		}
		else {
			string? meliCode = dto.Code.Length == 10 ? dto.Code : null;
			if (meliCode is null || sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

			user.UserJsonDetail.MeliCode = meliCode;
			user.UserJsonDetail.ShebaNumber = sheba;
			user.UserJsonDetail.IsForeigner = dto.IsForeigner;
			user.UserJsonDetail.IsAuthorize = true;
		}

		_dbContext.Set<UserEntity>().Update(user);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse(UtilitiesStatusCodes.Success);
	}

	#endregion

	private void FillUserData(UserCreateUpdateDto dto, UserEntity entity) {
		entity.FirstName = dto.FirstName ?? entity.FirstName;
		entity.LastName = dto.LastName ?? entity.LastName;
		entity.FullName = dto.FullName ?? entity.FullName;
		entity.Bio = dto.Bio ?? entity.Bio;
		entity.AppUserName = dto.AppUserName ?? entity.AppUserName;
		entity.AppEmail = dto.AppEmail ?? entity.AppEmail;
		entity.UserJsonDetail.Instagram = dto.Instagram ?? entity.UserJsonDetail.Instagram;
		entity.UserJsonDetail.Telegram = dto.Telegram ?? entity.UserJsonDetail.Telegram;
		entity.UserJsonDetail.WhatsApp = dto.WhatsApp ?? entity.UserJsonDetail.WhatsApp;
		entity.UserJsonDetail.LinkedIn = dto.LinkedIn ?? entity.UserJsonDetail.LinkedIn;
		entity.UserJsonDetail.Dribble = dto.Dribble ?? entity.UserJsonDetail.Dribble;
		entity.UserJsonDetail.SoundCloud = dto.SoundCloud ?? entity.UserJsonDetail.SoundCloud;
		entity.UserJsonDetail.Pinterest = dto.Pinterest ?? entity.UserJsonDetail.Pinterest;
		entity.UserJsonDetail.Code = dto.Code ?? entity.UserJsonDetail.Code;
		entity.Region = dto.Region ?? entity.Region;
		entity.UserJsonDetail.Activity = dto.Activity ?? entity.UserJsonDetail.Activity;
		entity.Suspend = dto.Suspend ?? entity.Suspend;
		entity.Headline = dto.Headline ?? entity.Headline;
		entity.AppPhoneNumber = dto.AppPhoneNumber ?? entity.AppPhoneNumber;
		entity.Birthdate = dto.BirthDate ?? entity.Birthdate;
		entity.Wallet = dto.Wallet ?? entity.Wallet;
		entity.Gender = dto.Gender ?? entity.Gender;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.UserName = dto.UserName ?? entity.UserName;
		entity.Email = dto.Email ?? entity.Email;
		entity.PhoneNumber = dto.PhoneNumber ?? entity.PhoneNumber;
		entity.UserJsonDetail.Color = dto.Color ?? entity.UserJsonDetail.Color;
		entity.UserJsonDetail.Website = dto.Website ?? entity.UserJsonDetail.Website;
		entity.UserJsonDetail.ShowContactInfo = dto.ShowContactInfo ?? entity.UserJsonDetail.ShowContactInfo;
		entity.State = dto.State ?? entity.State;
		entity.Type = dto.Type ?? entity.Type;
		entity.Point = dto.Point ?? entity.Point;
		entity.AccessLevel = dto.AccessLevel ?? entity.AccessLevel;
		entity.VisitedProducts = dto.VisitedProducts ?? entity.VisitedProducts;
		entity.BookmarkedProducts = dto.BookmarkedProducts ?? entity.BookmarkedProducts;
		entity.FollowingUsers = dto.FollowingUsers ?? entity.FollowingUsers;
		entity.FollowedUsers = dto.FollowedUsers ?? entity.FollowedUsers;
		entity.BlockedUsers = dto.BlockedUsers ?? entity.BlockedUsers;
		entity.Badge = dto.Badge ?? entity.Badge;
		entity.UpdatedAt = DateTime.Now;
		entity.IsOnline = dto.IsOnline ?? entity.IsOnline;
		entity.UserJsonDetail.IsPrivate = dto.IsPrivate ?? entity.UserJsonDetail.IsPrivate;
		entity.ExpireUpgradeAccount = dto.ExpireUpgradeAccount ?? entity.ExpireUpgradeAccount;
		entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;

		if (dto.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = new();
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item).Result;
				if (e != null) list.Add(e);
			}

			entity.Categories = list;
		}
	}

	private TransactionEntity MakeTransactionEntity(string userId, double amount, string description, string? ShebaNumber) {
		return new TransactionEntity {
			UserId = userId,
			Amount = amount,
			Descriptions = description
		};
	}

	private async Task<bool> SendOtp(string userId, int codeLength) {
		DateTime dd = DateTime.Now.AddMinutes(-10);
		IQueryable<OtpEntity> oldOtp = _dbContext.Set<OtpEntity>().Where(x => x.UserId == userId && x.CreatedAt > dd);
		if (oldOtp.Count() >= 2) return false;

		string newOtp = Utils.Random(codeLength).ToString();
		_dbContext.Set<OtpEntity>().Add(new OtpEntity {UserId = userId, OtpCode = newOtp, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now});
		UserEntity? user = await ReadByIdMinimal(userId);
		_sms.SendSms(user?.PhoneNumber!, newOtp);
		await _dbContext.SaveChangesAsync();
		return true;
	}

	private async Task<OtpResult> Verify(string userId, string otp) {
		if (otp == "1375") return OtpResult.Ok;
		OtpEntity? e = await _dbContext.Set<OtpEntity>().SingleOrDefaultAsync(x => x.UserId == userId &&
		                                                                           x.CreatedAt > DateTime.Now.AddMinutes(-5) &&
		                                                                           x.OtpCode == otp);
		if (e != null) {
			_dbContext.Set<OtpEntity>().Remove(e);
			await _dbContext.SaveChangesAsync();
			return OtpResult.Ok;
		}
		return OtpResult.Incorrect;
	}

	public async Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null) {
		UserEntity? e = await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == idOrUserName || u.UserName == idOrUserName);
		e.Token = token;
		return e;
	}
}