using System.Diagnostics;

namespace Utilities_aspnet.Utilities;

public class Server {
	private static IHttpContextAccessor? _httpContextAccessor;

	private static string? _serverAddress;

	public static string ServerAddress {
		get {
			if (_serverAddress != null) return _serverAddress;
			HttpRequest? request = _httpContextAccessor?.HttpContext?.Request;
			_serverAddress = $"{request?.Scheme}://{request?.Host.ToUriComponent()}{request?.PathBase.ToUriComponent()}";
			return _serverAddress;
		}
	}

	public static void Configure(IHttpContextAccessor? httpContextAccessor) => _httpContextAccessor = httpContextAccessor;
	
	public static async Task RunCommand(string command, string args) {
		try {
			Process process = new() {
				StartInfo = new ProcessStartInfo {
					FileName = command,
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};
			process.Start();
			string output = await process.StandardOutput.ReadToEndAsync();
			string error = await process.StandardError.ReadToEndAsync();
			Console.WriteLine(output);
			Console.WriteLine(error);
			await process.WaitForExitAsync();
		}
		catch (Exception) { }
	}
}