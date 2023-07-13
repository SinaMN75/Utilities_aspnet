namespace Utilities_aspnet.Hubs;

public class ChatHub : Hub {
	private readonly DbContext _context;
	private readonly string _uploadsFolder;

	public ChatHub(DbContext db, IWebHostEnvironment hostingEnvironment) {
		_context = db;
		_uploadsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "intranet");
	}

	public override async Task OnConnectedAsync() {
		string fileName = Path.Combine(_uploadsFolder, $"OnConnected - {DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
		FileInfo log = new(fileName);
		await using StreamWriter sw = log.CreateText();
		await sw.WriteLineAsync("onConnect Started");
		string? userId = Context.User!.Identity!.Name;
		await sw.WriteLineAsync($"user Id that get from context is : {userId}");
		UserEntity? user = _context.Set<UserEntity>().FirstOrDefault(f => f.Id == userId);
		if (user is not null) {
			await sw.WriteLineAsync($"user finded - user id is : {user.Id}");
			user.IsOnline = true;
			_context.Update(user);
			await _context.SaveChangesAsync();
			await sw.WriteLineAsync("db updated");
		}
		await Clients.All.SendAsync("UserConnected", userId);
		await sw.WriteLineAsync("sended to client");
	}

	public override async Task OnDisconnectedAsync(Exception? exception) {
		string fileName = Path.Combine(_uploadsFolder, $"OnConnected - {DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
		FileInfo log = new(fileName);
		await using StreamWriter sw = log.CreateText();
		await sw.WriteLineAsync("disconnected Started");
		string? userId = Context.User!.Identity!.Name;
		await sw.WriteLineAsync($"user Id that get from context is : {userId}");
		UserEntity? user = _context.Set<UserEntity>().FirstOrDefault(f => f.Id == userId);
		if (user is not null) {
			await sw.WriteLineAsync($"user finded - user id is : {user.Id}");
			user.IsOnline = false;
			_context.Update(user);
			await _context.SaveChangesAsync();
			await sw.WriteLineAsync("db updated");
		}
		await Clients.All.SendAsync("UserDisconnected", userId);
		await sw.WriteLineAsync("sended to client");
	}

	public async Task SendMessageToReceiver(string sender, string receiver, string message) {
		string fileName = Path.Combine(_uploadsFolder, $"send message - {DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
		FileInfo log = new(fileName);
		await using StreamWriter sw = log.CreateText();
		await sw.WriteLineAsync($"sender : {sender}, reviever: {receiver} message :{message}");
		UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id == receiver);
		await sw.WriteLineAsync($"reciever id : {user!.Id}");
		if (user.IsOnline.IsTrue()) {
			await Clients.User(user.Id).SendAsync("MessageReceived", sender, message);
			await sw.WriteLineAsync("sended to client");
		}
		else {
			message += "template";
			await Clients.User(user.Id).SendAsync("MessageReceived", sender, message);
			await sw.WriteLineAsync("sended to client");
		}
		//Todo: else => push notification for receiver
	}

	/// <summary>
	///     about types
	///     1 = is typing
	///     2 = recording voice
	///     3 = sending file
	///     4 = none
	/// </summary>
	public async Task SendingState(string sender, string receiver, int type) {
		string fileName = Path.Combine(_uploadsFolder, $"sending state - {DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
		FileInfo log = new(fileName);
		await using StreamWriter sw = log.CreateText();
		await sw.WriteLineAsync($"sender : {sender}, reviever: {receiver} type :{type}");
		UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id == receiver);
		await sw.WriteLineAsync($"reciever id : {user!.Id}");
		if (user.IsOnline.IsTrue()) {
			await Clients.User(user.Id).SendAsync("MessageState", sender, type);
			await sw.WriteLineAsync("sended to client");
		}
	}

	//public async Task SendEditedMessageToReceiver(
	//	string sender,
	//	string receiver,
	//	string message,
	//	Guid messageId) {
	//	UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id.ToLower() == receiver.ToLower());

	//	if (user != null) {
	//		//Todo: change IsloggedIn to IsOnline
	//		if (user.IsLoggedIn)
	//			await Clients.User(user.Id).SendAsync("MessageEdited", sender, message, messageId);
	//		//Todo: else => push notification for receiver
	//	}
	//}

	//public async Task SendDeletedMessageToReceiver(string sender, string receiver, Guid messageId) {
	//	UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id.ToLower() == receiver.ToLower());

	//	if (user != null) {
	//		//Todo: change IsloggedIn to IsOnline
	//		if (user.IsLoggedIn)
	//			await Clients.User(user.Id).SendAsync("MessageDeleted", sender, messageId);
	//		//Todo: else => push notification for receiver
	//	}
	//}

	//public async Task SendMessageToGroup(string roomId, List<string> receiverList, string message) {
	//	await Clients.Users(receiverList).SendAsync("MessageReceivedFromGroup", roomId, message);
	//}

	//public async Task SendEditedMessageToGroup(
	//	string roomId,
	//	List<string> receiverList,
	//	string message,
	//	string messageId) {
	//	await Clients.Users(receiverList).SendAsync("MessageEditedFromGroup", roomId, message, messageId);
	//}

	//public async Task SendDeletedMessageToGroup(
	//	string roomId,
	//	List<string> receiverList,
	//	string message,
	//	string messageId) {
	//	await Clients.Users(receiverList).SendAsync("MessageDeletedFromGroup", roomId, message, messageId);
	//}

	//public async Task NotifySeenMessage(string senderId, string receiverId, Guid messageId) {
	//	UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id.ToLower() == senderId.ToLower());
	//	string? userId = user?.Id;
	//	if (!string.IsNullOrEmpty(userId)) {
	//		await Clients.User(userId).SendAsync("MessageSeen", senderId, receiverId, messageId);
	//	}
	//}
}