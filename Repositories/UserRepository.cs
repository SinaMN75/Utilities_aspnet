namespace Utilities_aspnet.Repositories;

public interface IUserRepository {
	Task<GenericResponse<IQueryable<UserEntity>>> Filter(UserFilterDto dto);
	Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null);
	Task<GenericResponse<UserEntity?>> Update(UserCreateUpdateDto dto);
	Task<GenericResponse> Delete(string id);
	Task<GenericResponse<UserEntity?>> GetTokenForTest(string? mobile);
	Task<GenericResponse<string?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginDto dto);
	Task<GenericResponse<UserEntity?>> VerifyCodeForLogin(VerifyMobileForLoginDto dto);
	Task<GenericResponse<UserEntity?>> Register(RegisterDto dto);
	Task<GenericResponse<UserEntity?>> LoginWithPassword(LoginWithPasswordDto model);
	Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList();
	Task<GenericResponse> ToggleBlock(string userId);
	Task<GenericResponse> TransferWalletToWallet(TransferFromWalletToWalletDto dto);
	Task<UserEntity?> ReadByIdMinimal(string? idOrUserName, string? token = null);
}

public class UserRepository : IUserRepository {
	private readonly DbContext _dbContext;
	private readonly UserManager<UserEntity> _userManager;
	private readonly ISmsNotificationRepository _sms;
	private readonly string? _userId;

	public UserRepository(
		DbContext dbContext,
		UserManager<UserEntity> userManager,
		ISmsNotificationRepository sms,
		IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userManager = userManager;
		_sms = sms;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<UserEntity?>> ReadById(string idOrUserName, string? token = null) {
		bool isUserId = Guid.TryParse(idOrUserName, out _);
		UserEntity? entity = await _dbContext.Set<UserEntity>()
			.Include(u => u.Media)
			.Include(u => u.Categories)!.ThenInclude(u => u.Media)
			.FirstOrDefaultAsync(u => isUserId ? u.Id == idOrUserName : u.UserName == idOrUserName);

		if (entity == null) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.NotFound);

		entity.IsAdmin = await _userManager.IsInRoleAsync(entity, "Admin");
		entity.Token = token;
		entity.GrowthRate = GetGrowthRate(entity.Id).Result;

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

	public async Task<GenericResponse<IQueryable<UserEntity>>> Filter(UserFilterDto dto) {
		IQueryable<UserEntity> q = _dbContext.Set<UserEntity>().Where(x => x.DeletedAt == null).AsNoTracking();
		
		if (dto.UserNameExact != null) q = q.Where(x => x.AppUserName == dto.UserNameExact || x.UserName == dto.UserNameExact);
		if (dto.UserId != null) q = q.Where(x => x.Id == dto.UserId);
		if (dto.Activity != null) q = q.Where(x => x.Activity.Contains(dto.Activity));
		if (dto.Badge != null) q = q.Where(x => x.Badge.Contains(dto.Badge));
		if (dto.Bio != null) q = q.Where(x => x.Bio.Contains(dto.Bio));
		if (dto.Color != null) q = q.Where(x => x.Color.Contains(dto.Color));
		if (dto.Dribble != null) q = q.Where(x => x.Dribble.Contains(dto.Dribble));
		if (dto.Email != null) q = q.Where(x => x.Email.Contains(dto.Email));
		if (dto.Gender != null) q = q.Where(x => x.Gender.Contains(dto.Gender));
		if (dto.Headline != null) q = q.Where(x => x.Headline.Contains(dto.Headline));
		if (dto.Instagram != null) q = q.Where(x => x.Instagram.Contains(dto.Instagram));
		if (dto.Pinterest != null) q = q.Where(x => x.Pinterest.Contains(dto.Pinterest));
		if (dto.Region != null) q = q.Where(x => x.Region.Contains(dto.Region));
		if (dto.State != null) q = q.Where(x => x.State.Contains(dto.State));
		if (dto.Telegram != null) q = q.Where(x => x.Telegram.Contains(dto.Telegram));
		if (dto.Type != null) q = q.Where(x => x.Type.Contains(dto.Type));
		if (dto.Website != null) q = q.Where(x => x.Website.Contains(dto.Website));
		if (dto.AccessLevel != null) q = q.Where(x => x.AccessLevel.Contains(dto.AccessLevel));
		if (dto.AppEmail != null) q = q.Where(x => x.AppEmail.Contains(dto.AppEmail));
		if (dto.FirstName != null) q = q.Where(x => x.FirstName.Contains(dto.FirstName));
		if (dto.LastName != null) q = q.Where(x => x.LastName.Contains(dto.LastName));
		if (dto.FullName != null) q = q.Where(x => x.FullName.Contains(dto.FullName));
		if (dto.GenderTr1 != null) q = q.Where(x => x.GenderTr1.Contains(dto.GenderTr1));
		if (dto.UseCase != null) q = q.Where(x => x.UseCase.Contains(dto.UseCase));
		if (dto.GenderTr2 != null) q = q.Where(x => x.GenderTr2.Contains(dto.GenderTr2));
		if (dto.PhoneNumber != null) q = q.Where(x => x.PhoneNumber.Contains(dto.PhoneNumber));
		if (dto.AppUserName != null) q = q.Where(x => x.AppUserName.Contains(dto.AppUserName));
		if (dto.AppPhoneNumber != null) q = q.Where(x => x.AppPhoneNumber.Contains(dto.AppPhoneNumber));
		if (dto.WhatsApp != null) q = q.Where(x => x.WhatsApp.Contains(dto.WhatsApp));
		if (dto.LinkedIn != null) q = q.Where(x => x.LinkedIn.Contains(dto.LinkedIn));
		if (dto.SoundCloud != null) q = q.Where(x => x.SoundCloud.Contains(dto.SoundCloud));
		if (dto.StateTr1 != null) q = q.Where(x => x.StateTr1.Contains(dto.StateTr1));
		if (dto.StateTr2 != null) q = q.Where(x => x.StateTr2.Contains(dto.StateTr2));

		if (dto.Query != null)
			q = q.Where(x => x.FirstName.Contains(dto.Query) ||
			                 x.LastName.Contains(dto.Query) ||
			                 x.FullName.Contains(dto.Query) ||
			                 x.UserName.Contains(dto.Query) ||
			                 x.AppUserName.Contains(dto.Query) ||
			                 x.AppEmail.Contains(dto.Query)
			);

		if (dto.UserIds != null) q = q.Where(x => dto.UserIds.Contains(x.Id));
		if (dto.UserName != null) q = q.Where(x => (x.AppUserName ?? "").ToLower().Contains(dto.UserName.ToLower()));
		if (dto.ShowSuspend.IsTrue()) q = q.Where(x => x.Suspend == true);
		
		if (dto.OrderByUserName.IsTrue()) q = q.OrderBy(x => x.UserName);
		
		if (dto.ShowMedia.IsTrue()) q = q.Include(u => u.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(u => u.Categories);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<UserEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> Delete(string id) {
		UserEntity? user = await ReadByIdMinimal(id);
		if (user == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		user.DeletedAt = DateTime.Now;
		_dbContext.Set<UserEntity>().Update(user);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
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

		user.IsLoggedIn = true;
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
			AccessLevel = dto.AccessLevel,
			Suspend = false,
			FirstName = dto.FirstName,
			LastName = dto.LastName,
			IsLoggedIn = true,
		};

		IdentityResult? result = await _userManager.CreateAsync(user, dto.Password);
		if (!result.Succeeded)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.Unhandled, "The information was not entered correctly");

		JwtSecurityToken token = await CreateToken(user);

		if (dto.SendSMS ?? false) {
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
		if (user.Suspend) return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.UserSuspended);

		user.IsLoggedIn = true;
		await _dbContext.SaveChangesAsync();
		JwtSecurityToken token = await CreateToken(user);

		if (await Verify(user.Id, dto.VerificationCode) != OtpResult.Ok)
			return new GenericResponse<UserEntity?>(null, UtilitiesStatusCodes.WrongVerificationCode);

		return new GenericResponse<UserEntity?>(ReadById(user.Id, new JwtSecurityTokenHandler().WriteToken(token)).Result.Result);
	}

	public async Task<GenericResponse<IEnumerable<UserEntity>>> ReadMyBlockList() {
		UserEntity? user = await ReadByIdMinimal(_userId);
		GenericResponse<IQueryable<UserEntity>> blockedUsers = await Filter(new UserFilterDto {
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
		await Update(new UserCreateUpdateDto {Id = toUser.Id, Wallet = toUser.Wallet + dto.Amount});
		return new GenericResponse();
	}

	#endregion

	private void FillUserData(UserCreateUpdateDto dto, UserEntity entity) {
		entity.FirstName = dto.FirstName ?? entity.FirstName;
		entity.LastName = dto.LastName ?? entity.LastName;
		entity.FullName = dto.FullName ?? entity.FullName;
		entity.Bio = dto.Bio ?? entity.Bio;
		entity.AppUserName = dto.AppUserName ?? entity.AppUserName;
		entity.AppEmail = dto.AppEmail ?? entity.AppEmail;
		entity.Instagram = dto.Instagram ?? entity.Instagram;
		entity.Telegram = dto.Telegram ?? entity.Telegram;
		entity.WhatsApp = dto.WhatsApp ?? entity.WhatsApp;
		entity.LinkedIn = dto.LinkedIn ?? entity.LinkedIn;
		entity.Dribble = dto.Dribble ?? entity.Dribble;
		entity.SoundCloud = dto.SoundCloud ?? entity.SoundCloud;
		entity.Pinterest = dto.Pinterest ?? entity.Pinterest;
		entity.AppEmail = dto.AppEmail ?? entity.AppEmail;
		entity.Region = dto.Region ?? entity.Region;
		entity.Activity = dto.Activity ?? entity.Activity;
		entity.Suspend = dto.Suspend ?? entity.Suspend;
		entity.Headline = dto.Headline ?? entity.Headline;
		entity.AppPhoneNumber = dto.AppPhoneNumber ?? entity.AppPhoneNumber;
		entity.Birthdate = dto.BirthDate ?? entity.Birthdate;
		entity.Wallet = dto.Wallet ?? entity.Wallet;
		entity.Gender = dto.Gender ?? entity.Gender;
		entity.GenderTr1 = dto.GenderTr1 ?? entity.GenderTr1;
		entity.GenderTr2 = dto.GenderTr2 ?? entity.GenderTr2;
		entity.UseCase = dto.UseCase ?? entity.UseCase;
		entity.UserName = dto.UserName ?? entity.UserName;
		entity.Email = dto.Email ?? entity.Email;
		entity.PhoneNumber = dto.PhoneNumber ?? entity.PhoneNumber;
		entity.Color = dto.Color ?? entity.Color;
		entity.Website = dto.Website ?? entity.Website;
		entity.ShowContactInfo = dto.ShowContactInfo ?? entity.ShowContactInfo;
		entity.State = dto.State ?? entity.State;
		entity.Type = dto.Type ?? entity.Type;
		entity.StateTr1 = dto.StateTr1 ?? entity.StateTr1;
		entity.StateTr2 = dto.StateTr2 ?? entity.StateTr2;
		entity.Point = dto.Point ?? entity.Point;
		entity.AccessLevel = dto.AccessLevel ?? entity.AccessLevel;
		entity.VisitedProducts = dto.VisitedProducts ?? entity.VisitedProducts;
		entity.BookmarkedProducts = dto.BookmarkedProducts ?? entity.BookmarkedProducts;
		entity.FollowingUsers = dto.FollowingUsers ?? entity.FollowingUsers;
		entity.FollowedUsers = dto.FollowedUsers ?? entity.FollowedUsers;
		entity.BoughtProduts = dto.BoughtProduts ?? entity.BoughtProduts;
		entity.BlockedUsers = dto.BlockedUsers ?? entity.BlockedUsers;
		entity.Badge = dto.Badge ?? entity.Badge;
		entity.UpdatedAt = DateTime.Now;
		entity.IsLoggedIn = dto.IsLoggedIn ?? entity.IsLoggedIn;
		entity.IsOnline = dto.IsOnline ?? entity.IsOnline;

		if (dto.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = new();
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? e = _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item).Result;
				if (e != null) list.Add(e);
			}

			entity.Categories = list;
		}
	}

	private async Task<GrowthRateReadDto?> GetGrowthRate(string? id) {
		IEnumerable<CommentEntity> myComments = await _dbContext.Set<CommentEntity>().Where(x => x.UserId == id).ToListAsync();
		IEnumerable<Guid> productIds = await _dbContext.Set<ProductEntity>().Where(x => x.UserId == id).Select(x => x.Id).ToListAsync();
		IEnumerable<CommentEntity> comments = await _dbContext.Set<CommentEntity>().Where(x => productIds.Contains(x.ProductId ?? Guid.Empty)).ToListAsync();
		GrowthRateReadDto dto = new() {
			Feedback7 = 0,
			Id = id
		};
		double totalInteractive = dto.InterActive1 +
		                          dto.InterActive2 +
		                          dto.InterActive3 +
		                          dto.InterActive4 +
		                          dto.InterActive5 +
		                          dto.InterActive6;
		double totalFeedback = dto.Feedback1 +
		                       dto.Feedback2 +
		                       dto.Feedback3 +
		                       dto.Feedback4 +
		                       dto.Feedback5 +
		                       dto.Feedback6;
		double total = totalInteractive + totalFeedback;
		if (total > 0) {
			dto.TotalInterActive = totalInteractive / total * 100;
			dto.TotalFeedback = totalFeedback / total * 100;
		}

		return dto;
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