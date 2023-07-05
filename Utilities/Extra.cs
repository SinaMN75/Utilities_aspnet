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

	public static int ToInt(this double? value) => (int) (value ?? 0.0);
}

public static class StringExtension {
	public static bool IsEmail(this string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$", RegexOptions.IgnoreCase);

	public static bool IsNotNullOrEmpty(this string? s) => s is { Length: > 0 };

	public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

	public static string? GetShebaNumber(this string s) {
		s = s.Replace(" ", "");
		s = s.Replace("IR", "");
		s = s.Replace("ir", "");
		if (s.Length == 24) return s;
		return null;
	}

	public static string? AddCommaSeperatorUsers(this string? baseString, string? userId) {
		if (string.IsNullOrEmpty(baseString)) return userId;
		if (baseString.Contains(userId ?? "")) return baseString;
		return baseString + "," + userId;
	}
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
		bool overUsed;
		try {
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
							overUsed = context.Set<ProductEntity>().Count(w => w.UserId == userId && w.CreatedAt.Value.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
						else
							overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt.Value.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
					}
					else {
						if (user.ExpireUpgradeAccount == null || user.ExpireUpgradeAccount < DateTime.Now)
							overUsed = context.Set<ProductEntity>().Count(w => w.UserId == userId && w.CreatedAt.Value.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
						else
							overUsed = context.Set<CommentEntity>().Count(w => w.UserId == userId && w.CreatedAt.Value.Date == DateTime.Today) >
							           usageRules.MaxProductPerDay;
					}
					break;
				default:
					overUsed = false;
					break;
			}
			if (overUsed) return new Tuple<bool, UtilitiesStatusCodes>(overUsed, UtilitiesStatusCodes.Overused);
			return new Tuple<bool, UtilitiesStatusCodes>(false, UtilitiesStatusCodes.Success);
		}
		catch (Exception) { return new Tuple<bool, UtilitiesStatusCodes>(true, UtilitiesStatusCodes.BadRequest); }
	}

	public static string DisplayCountOfCompleteOrder(int countOfCompleteOrder) {
		switch (countOfCompleteOrder) {
			case <= 50: return countOfCompleteOrder.ToString();
			case > 50 and <= 100: return "+50";
			case > 100 and <= 200: return "+100";
			case > 200 and <= 300: return "+200";
			case > 300 and <= 400: return "+300";
			case > 400 and <= 500: return "+400";
			case > 500 and <= 600: return "+500";
			case > 600 and <= 700: return "+600";
			case > 700 and <= 800: return "+700";
			case > 800 and <= 900: return "+800";
			case > 900 and <= 1000: return "+900";
			case > 1000 and <= 2000: return "+1000";
			case > 2000 and <= 3000: return "+2000";
			case > 3000 and <= 4000: return "+3000";
			case > 4000 and <= 5000: return "+4000";
			case > 5000 and <= 6000: return "+5000";
			case > 6000 and <= 7000: return "+6000";
			case > 7000 and <= 8000: return "+7000";
			case > 8000 and <= 9000: return "+8000";
			case > 9000 and <= 10000: return "+9000";
			case > 10000: return "+10000";
		}
	}
}