namespace Utilities_aspnet.Utilities;

public static class EnumExtension {
	public static IEnumerable<IdTitleDto> GetValues<T>() => (from int itemType in Enum.GetValues(typeof(T))
		select new IdTitleDto { Title = Enum.GetName(typeof(T), itemType), Id = itemType }).ToList();
}

public static class UtilitiesStatusCodesExtension {
	public static int Value(this UtilitiesStatusCodes statusCode) => (int)statusCode;
}

public enum UtilitiesDatabaseType {
	SqlServer = 100,
	Postgres = 101
}

public enum UtilitiesStatusCodes {
	Success = 200,
	BadRequest = 400,
	UnAuthorized = 401,
	Forbidden = 403,
	NotFound = 404,
	Conflict = 409,
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
	InvalidDiscountCode = 618,
	S3Error = 619
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

public enum Reaction {
	None = 100,
	Like = 101,
	DisLike = 102,
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
	SendPost = 103,
	None = 99999
}

public enum DisplayType {
	None = 100,
	Explore = 101,
	Channels = 102
}

public enum TagTransaction {
	DepositToWallet = 101,
	WithdrawFromTheWallet = 102,
	Buy = 103,
	Sell = 104,
	Recharge = 105,
	WalletToWallet = 106,
	Return = 107,
	Premium = 108,
	Pending = 200,
	Fail = 201,
	Success = 202,
	WithdrawAccepted = 301,
	WithdrawRejected = 302,
	WithdrawRequested = 303
}

public enum TagCategory {
	Category = 100,
	YooNote = 101,
	Speciality = 102,
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
	Service = 123,
	SubProduct = 124,
	Image = 125,
	Video = 126,
	Audio = 127,
	Pdf = 128,
	Apk = 129,
	Game = 130,
	Goods = 131,
	Job = 132,
	Physical = 133,
	Digital = 134,
	UserStatus = 135,
	JobType = 136,
	JobPlace = 137,
	Story = 138,
	Blog = 139,
	SubBlog = 140,
	Music = 141,
	Podcast = 142,
	AdEmployee = 143,
	App = 144,
	All = 145,
	Text = 146,
	AdProject = 147,
	AdHiring = 148,
	Highlight = 149,
	PhysicalProduct = 150,
	DigitalProduct = 151,
	Project = 152,
	Children = 153,
	Recruitment = 154,
	Freelancing = 155,
	Store = 156,
	Artwork = 157,
	DigitalEquipment = 158,
	ToolsAndAccessories = 159,
	Book = 160,
	Cooperation = 161,
	ConcertFestival = 162,
	CinemaTheater = 163,
	Gathering = 164,
	AcademyEducation = 165,
	Event = 166,
	Award = 167,
	Adult = 168,
	Teenager = 169,
	Media = 170,
	Explore = 171,
	Reference = 172,
	File = 173,
	Model = 174,
	Function = 175,
	Country = 176,
	City = 177,
	Province = 178,
	Free = 180,
	Payment = 181,
	Favorites = 182,
	Special = 183,
	Recommended = 184,
	Sport = 185,
	Enabled = 201
}

public enum TagContent {
	Terms = 101,
	AboutUs = 102,
	HomeBanner1 = 103,
	HomeBanner2 = 104,
	Info1 = 105,
	Qa = 106,
	HomeBannerSmall1 = 107,
	HomeBannerSmall2 = 108,
	ContactInfo = 109,
	SmallDetail1 = 110,
	SmallDetail2 = 111,
	News = 112,
	Premium = 113,
	Premium1Month = 114,
	Premium3Month = 115,
	Premium6Month = 116,
	Premium12Month = 117
}

public enum TagComment {
	Released = 100,
	InQueue = 101,
	Rejected = 102,
	Private = 501
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
	Post = 115,
	File = 116,
	Participants = 117
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
	Blog = 125,
	SubBlog = 126,
	Music = 127,
	Podcast = 128,
	AdEmployee = 129,
	Target = 130,
	App = 131,
	All = 132,
	Text = 133,
	AdProject = 134,
	AdHiring = 135,
	Highlight = 136,
	PhysicalProduct = 137,
	DigitalProduct = 138,
	Project = 139,
	Children = 140,
	Recruitment = 141,
	Freelancing = 142,
	Store = 143,
	Artwork = 144,
	DigitalEquipment = 145,
	ToolsAndAccessories = 146,
	Book = 147,
	Cooperation = 148,
	ConcertFestival = 149,
	CinemaTheater = 150,
	Gathering = 151,
	AcademyEducation = 152,
	Event = 153,
	Award = 154,
	Adult = 155,
	Teenager = 156,
	Auction = 157,
	Consultant = 158,
	Service = 159,
	FullTime = 160,
	PartTime = 161,
	Contractual = 162,
	Free = 163,
	Payment = 164,
	Participants = 165,
	File = 166,
	Remote = 168,
	OnSite = 169,
	PricePerDay = 170,
	PricePerPerson = 171,
	PricePerPage = 172,
	PricePerCount = 173,
	PricePerHour = 174,
	PricePerMinute = 175,
	Reserve = 176,
	MicroBlog = 177,
	DeliveryCost = 178,
	Appointment = 179,
	Internal = 180,
	Foreign = 181,
	Journal = 182,
	Link = 183,
	Premium1Month = 184,
	Premium3Month = 185,
	Premium6Month = 186,
	Premium12Month = 187,
	Post = 189,
	Tournament = 190,
	New = 201,
	KindOfNew = 202,
	Used = 203,
	Released = 301,
	Expired = 302,
	InQueue = 303,
	Deleted = 304,
	NotAccepted = 305,
	AwaitingPayment = 306,
	Special = 401,
	Private = 501,
	None = 502
}

public enum TagOrder {
	Physical = 100,
	Digital = 101,
	Donate = 102,
	Reserve = 103,
	Premium = 104,
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
	Paid = 402,
	InProcess = 406,
	Sent = 408,
	Conflict = 409,
	Complete = 410,
	Cancelled = 411,
	Private = 501,
	Visible = 601
}

public enum TagUser {
	Authorized = 100,
	Private = 101,
	Public = 102,
	Male = 200,
	Female = 201,
	Unknown = 202,
	Legal = 203,
	Guest = 204,
	AdminCategoryRead = 300,
	AdminCategoryUpdate = 301,
	AdminProductRead = 302,
	AdminProductUpdate = 303,
	AdminUserRead = 304,
	AdminUserUpdate = 305,
	AdminReportRead = 306,
	AdminReportUpdate = 307,
	AdminTransactionRead = 308,
	AdminTransactionUpdate = 309,
	AdminOrderRead = 310,
	AdminOrderUpdate = 311,
	AdminContentRead = 312,
	AdminContentUpdate = 313,
	AdminCommentRead = 314,
	AdminCommentUpdate = 315,
	ProductPublic = 401,
	BuysPublic = 402,
	SellsPublic = 403,
	FollowingsPublic = 404,
	FollowersPublic = 405,
	ProductPrivate = 406,
	BuysPrivate = 407,
	SellsPrivate = 408,
	FollowingsPrivate = 409,
	FollowersPrivate = 410
}

public enum TagPayment {
	PayOrder = 101,
	PayWallet = 102,
	Premium = 103
}

public enum TagReservationChair {
	Blank = 101,
	NotAvailable = 102,
	Free = 103,
	Reserved = 104
}

public enum TagNotification {
	ReceivedComment = 101,
	ReceivedChat = 102,
	Followed = 103,
	ReceivedReactionOnProduct = 104
}

public enum TagSubscription {
	Complete = 101,
	AwaitingPayment = 102
}