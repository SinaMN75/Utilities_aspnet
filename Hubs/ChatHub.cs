namespace Utilities_aspnet.Hubs;

public class ChatHub : Hub {

	public async Task SendMessage(string userId, string message) {
		await Clients.All.SendAsync("ReceiveMessage", userId, message);
	}
	
}