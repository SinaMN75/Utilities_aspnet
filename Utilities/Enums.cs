namespace Utilities_aspnet.Utilities;

public static class EnumExtension {
	public static IEnumerable<IdTitleDto> GetValues<T>() => (from int itemType in Enum.GetValues(typeof(T))
		select new IdTitleDto { Title = Enum.GetName(typeof(T), itemType), Id = itemType }).ToList();
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
	MoreThanAllowedMoney = 616,
	WrongPassword = 617,
	InvalidDiscountCode = 618
}

public enum TagFormField {
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
	None = 100,
	Physical = 101,
	Digital = 102
}

public enum SubscriptionType {
	None = 100,
	Promotion = 101,
	UpgradeAccount = 102
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
	FollowersOnly = 102,
	Business = 103
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

public enum TransactionType {
	None = 100,
	DepositToWallet = 101,
	WithdrawFromTheWallet = 102,
	Buy = 103,
	Sell = 104,
	Recharge = 105
}

public enum TagCategory {
	Category = 100,
	YooNote = 101,
	Specialty = 102,
	SpecializedArt = 103,
	Colors = 104,
	Brand = 105,
	Tag = 106,
	User = 107,
	Target = 108,
	Tutorial = 109,
	Attribute = 110,
	ShopCategory = 111,
	Magazine = 112,
	Insurance = 113,
	Learn = 114,
	Company = 115,
	Consultant = 116,
	Ad = 117,
	DailyPrice = 118,
	Tender = 119,
	Channel = 120,
	Group = 121,
	Auction = 122,
	Service = 123
}

public enum TagContent {
	Terms = 101,
	AboutUs = 102,
	HomeBanner1 = 103,
	HomeBanner2 = 104
}

public enum TagMedia {
	All = 100,
	Image = 101,
	Video = 102,
	Audio = 103,
	Pdf = 104,
	Apk = 105,
	Profile = 106,
	Document = 107,
	License = 108,
	Zip = 109,
	Bio = 110,
	Cover = 111,
	Media = 112,
	Text = 113,
	Chat = 114,
	Post = 115
}

public enum TagProduct {
	Product = 100,
	YooNote = 101,
	SubProduct = 102,
	Image = 103,
	Video = 104,
	Audio = 105,
	Pdf = 106,
	Apk = 107,
	Game = 108,
	Goods = 109,
	Job = 110,
	Attribute = 111,
	Physical = 112,
	Digital = 113,
	UserStatus = 114,
	JobType = 115,
	JobPlace = 116,
	Chanel = 117,
	Story = 118,
	Ad = 119,
	Company = 120,
	DailyPrice = 121,
	Tender = 122,
	Tutorial = 123,
	Magazine = 124,
	New = 201,
	KindOfNew = 202,
	Used = 203,
	Released = 301,
	Expired = 302,
	InQueue = 303,
	Deleted = 304,
	NotAccepted = 305
}

public enum TagOrder {
	Physical = 100,
	Digital = 101,
	Donate = 102,
	PishtazDelivery = 201,
	SendTipaxDelivery = 202,
	CustomDelivery = 203,
	OnlinePay = 301,
	OnSitePay = 302,
	CashPay = 303,
	StripePay = 304,
	CoinPay = 305,
	PaypalPay = 306,
	Pending = 400,
	Canceled = 401,
	Paid = 402,
	Accept = 403,
	Reject = 404,
	InProgress = 405,
	InProcess = 406,
	Shipping = 407,
	Refund = 408,
	RefundComplete = 409,
	Complete = 410,
	PaidFail = 412
}

public enum TagUser {
	Authorized = 100,
	Private = 101,
	Public = 102
}

public enum TagNotification { }