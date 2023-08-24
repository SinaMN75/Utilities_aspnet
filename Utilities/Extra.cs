namespace Utilities_aspnet.Utilities;

public class GenericResponse<T> : GenericResponse {
	public GenericResponse(T result, UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "") {
		Result = result;
		Status = status;
		Message = message;
	}

	public T? Result { get; }
}

public class GenericResponse {
	public GenericResponse(UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "") {
		Status = status;
		Message = message;
	}

	public UtilitiesStatusCodes Status { get; protected set; }
	public int? PageSize { get; set; }
	public int? PageCount { get; set; }
	public int? TotalCount { get; set; }
	protected string Message { get; set; }
}

public static class BoolExtension {
	public static bool IsTrue(this bool? value) => value != null && value != false;
}

public static class EnumerableExtension {
	public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? list) => list != null && list.Any();

	public static bool IsNotNull<T>(this IEnumerable<T>? list) => list != null;
}

public static class NumberExtension {
	public static int ToInt(this double value) => (int) value;
}

public static partial class StringExtension {
	public static bool IsEmail(this string email) => MyRegex().IsMatch(email);

	public static bool IsNotNullOrEmpty(this string? s) => s is { Length: > 0 };

	public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

	public static string? GetShebaNumber(this string s) {
		s = s.Replace(" ", "");
		s = s.Replace("IR", "");
		s = s.Replace("ir", "");
		return s.Length == 24 ? s : null;
	}

	public static string? AddCommaSeperatorUsers(this string? baseString, string? userId) {
		if (string.IsNullOrEmpty(baseString)) return userId;
		if (baseString.Contains(userId ?? "")) return baseString;
		return baseString + "," + userId;
	}

	[GeneratedRegex(@"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$", RegexOptions.IgnoreCase, "en-US")]
	private static partial Regex MyRegex();
}

public class Utils {
	public static Tuple<bool, UtilitiesStatusCodes> IsBlockedUser(UserEntity? reciever, UserEntity? sender) {
		bool isBlocked = false;
		UtilitiesStatusCodes utilCode = UtilitiesStatusCodes.Success;
		if (reciever is not null && sender is not null) {
			if (reciever.BlockedUsers.IsNotNullOrEmpty()) {
				isBlocked = reciever.BlockedUsers.Contains(sender.Id);
				if (isBlocked) utilCode = UtilitiesStatusCodes.UserSenderBlocked;
			}
			else if (sender.BlockedUsers.IsNotNullOrEmpty() && !isBlocked) {
				isBlocked = sender.BlockedUsers.Contains(reciever.Id);
				if (isBlocked) utilCode = UtilitiesStatusCodes.UserRecieverBlocked;
			}
		}
		return new Tuple<bool, UtilitiesStatusCodes>(isBlocked, utilCode);
	}

	public static Tuple<bool, UtilitiesStatusCodes> IsUserOverused(
		DbContext context,
		string? userId,
		CallerType? type,
		ChatType? chatType,
		string? useCaseProduct,
		UsageRules usageRules) {
		UserEntity? user = context.Set<UserEntity>().FirstOrDefault(f => f.Id == userId);
		if (user == null) return new Tuple<bool, UtilitiesStatusCodes>(true, UtilitiesStatusCodes.UserNotFound);
		try {
			bool overUsed;
			switch (type ?? CallerType.None) {
				case CallerType.CreateGroupChat:
					if (chatType == ChatType.Private) {
						if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
							overUsed = context.Set<GroupChatEntity>().Count(w => w.CreatorUserId == userId && w.CreatedAt > DateTime.Now.AddHours(-1)) >
							           usageRules.MaxChatPerDay;
						else
							overUsed = context.Set<GroupChatEntity>().Count(w => w.CreatorUserId == userId && w.CreatedAt > DateTime.Now.AddHours(-1)) >
							           usageRules.MaxChatPerDay;
					}
					else {
						if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
							overUsed = context.Set<GroupChatEntity>().Count(w => w.CreatorUserId == userId && w.CreatedAt > DateTime.Now.AddHours(-1)) >
							           usageRules.MaxChatPerDay;
						else
							overUsed = false;
					}
					break;
				case CallerType.CreateComment:
					if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
						overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt > DateTime.Now.AddHours(-1)) >
						           usageRules.MaxCommentPerDay;
					else
						overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt > DateTime.Now.AddHours(-1)) >
						           usageRules.MaxCommentPerDay;
					break;
				case CallerType.CreateProduct:
					if (useCaseProduct == "product") {
						if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
							overUsed = context.Set<ProductEntity>().Count(w => w.UserId == userId && w.CreatedAt.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
						else
							overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
					}
					else {
						if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
							overUsed = context.Set<ProductEntity>().Count(w => w.UserId == userId && w.CreatedAt.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
						else
							overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
					}
					break;
				case CallerType.None:
				default:
					overUsed = false;
					break;
			}
			return overUsed
				? new Tuple<bool, UtilitiesStatusCodes>(overUsed, UtilitiesStatusCodes.Overused)
				: new Tuple<bool, UtilitiesStatusCodes>(false, UtilitiesStatusCodes.Success);
		}
		catch (Exception) { return new Tuple<bool, UtilitiesStatusCodes>(true, UtilitiesStatusCodes.BadRequest); }
	}

	public static int CalculatePriceWithDiscount(int? price , int? discountPercent , int? discountPrice)
	{
		if(!price.HasValue) return 0;
		
		if(discountPrice.HasValue && discountPercent.HasValue) return price.Value;

		if (discountPercent.HasValue)
		{
			var result = (price.Value * discountPercent.Value) / 100;
			return price.Value - result;
		}

		if(discountPrice.HasValue)
			return price.Value - discountPrice.Value;

		return -99999;
	}
}