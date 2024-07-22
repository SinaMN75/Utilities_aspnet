namespace Utilities_aspnet.Hubs;

public class ChatHub : Hub {
	public async Task SendMessage(string user, string message) {
		// Broadcast the received message to all connected clients
		await Clients.All.SendAsync("ReceiveMessage", user, message);
	}

	public override async Task OnConnectedAsync() {
		// You can add custom logic here when a client connects
		// For example, notifying other clients about the new connection
		await Clients.Others.SendAsync("UserConnected", Context.ConnectionId);

		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception exception) {
		// You can add custom logic here when a client disconnects
		// For example, notifying other clients about the disconnection
		await Clients.Others.SendAsync("UserDisconnected", Context.ConnectionId);

		await base.OnDisconnectedAsync(exception);
	}
}