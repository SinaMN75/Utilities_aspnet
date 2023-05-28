namespace Utilities_aspnet.Utilities;

public static class EnumExtension {
	public static IEnumerable<IdTitleDto> GetValues<T>() {
		return (from int itemType in Enum.GetValues(typeof(T))
			select new IdTitleDto {Title = Enum.GetName(typeof(T), itemType), Id = itemType}).ToList();
	}
}

public static class UtilitiesStatusCodesExtension {
	public static int Value(this UtilitiesStatusCodes statusCode) => (int) statusCode;
}

public enum UtilitiesStatusCodes {
	Success = 200,
	BadRequest = 400,
	Forbidden = 403,
	NotFound = 404,
	Unhandled = 900,
	WrongVerificationCode = 601,
	MaximumLimitReached = 602,
	UserAlreadyExist = 603,
	UserSuspended = 604,
	UserNotFound = 605,
	MultipleSeller = 607,
	OrderPayed = 608,
	OutOfStock = 610,
	NotEnoughMoney = 611,
	UserRecieverBlocked = 612,
	UserSenderBlocked = 613,
	MoreThan2UserIsInPrivateChat = 614,
	Overused = 615,
	MoreThanAllowedMoney = 616
}

public enum NotoficationType {
	WalletMoneyGain = 100,
	WalletMoneyLost = 101,
}

public enum OtpResult {
	Ok = 1,
	Incorrect = 2,
	TimeOut = 3
}

public enum DatabaseType {
	SqlServer = 0,
	MySql = 1
}

public enum OrderStatuses {
	Pending = 100,
	Canceled = 101,
	Paid = 102,
	Accept = 103,
	Reject = 104,
	InProgress = 105,
	InProcess = 106,
	Shipping = 107, //not used yet
	Refund = 108, //not used yet
	RefundComplete = 109, //not used yet
	Complete = 110,
	PaidFail = 112,
}

public enum PayType {
	Online,
	PayAtHome,
	Cash = 101,
	Stripe = 102,
	Coin = 103,
	Paypal = 104,
	Visa = 105
}

public enum SendType {
	Pishtaz,
	Custom,
	Tipax
}

public enum FormFieldType {
	SingleLineText,
	MultiLineText,
	MultiSelect,
	SingleSelect,
	Bool,
	Number,
	File,
	Image,
	CarPlack,
	PhoneNumber,
	Password,
	Date,
	Time,
	DateTime
}

public enum TransactionStatus {
	Pending = 100,
	Fail = 101,
	Success = 102
}

public enum ProductStatus {
	Released = 1,
	Expired = 2,
	InQueue = 3,
	Deleted = 4
}

public enum Currency {
	Rial = 100,
	Dolor = 101,
	Lira = 102,
	Euro = 103,
	Btc = 200
}

public enum SeenStatus {
	UnSeen = 100,
	Seen = 101,
	SeenDetail = 102,
	Ignored = 103,
	Deleted = 104
}

public enum ChatStatus {
	Open = 100,
	Closes = 101,
	WaitingForHost = 102,
	Answered = 103,
	Deleted = 104
}

public enum ChatType {
	Private = 100,
	PublicGroup = 101,
	PrivateGroup = 102,
	PublicChannel = 103,
	PrivateChannel = 104
}

public enum Priority {
	VeryHigh = 100,
	High = 101,
	Normal = 102,
	Low = 103
}

public enum OrderType {
	Sale = 100,
	Purchase = 101,
	All = 102
}

public enum Reaction {
	None = 100,
	Like = 101,
	DissLike = 102,
	Funny = 103,
	Awful = 104
}

public enum GenderType {
	Male = 100,
	Female = 101,
	Unknown = 102,
	Company = 103,
	Team = 104,
	Both = 105,
	All = 106,
	None = 107
}

public enum PrivacyType {
	Private = 100,
	Public = 101,
	FollowersOnly = 102
}

public enum NationalityType {
	Iranian = 100,
	NonIranian = 101
}

public enum LegalAuthenticationType {
	Authenticated = 100,
	NotAutenticated = 101
}

public enum AgeCategory {
	None = 100,
	Kids = 101,
	Tennager = 102,
	Young = 103,
	Adult = 104
}

public enum ProductState {
	None = 100,
	New = 101,
	KindOfNew = 102,
	UsedGoods = 103
}

public enum OrderReportType {
	OrderDate = 100,
	OrderDateProductUseCase = 101,
	OrderProductUseCase = 102,
	OrderState = 103,
	OrderStuse = 104,
	All = 105
}

public enum ReportType {
	Insight = 100,
	TopKeyword = 101,
	PercentUsecase = 102,
	All = 105
}

public enum ReferenceIdType {
	Product = 100,
	Category = 101,
	User = 102
}

public enum CallerType {
	CreateGroupChat = 100,
	CreateComment = 101,
	CreateProduct = 102,
	None = 99999
}

public enum WithdrawState {
	None = 100,
	Requested = 101,
	Accepted = 102
}

public enum DisplayType {
	None = 100,
	Exploer = 101,
	Channels = 102
}