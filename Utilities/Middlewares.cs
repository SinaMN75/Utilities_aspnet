namespace Utilities_aspnet.Utilities;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger) {
	public async Task InvokeAsync(HttpContext context) {
		Stopwatch stopwatch = Stopwatch.StartNew();

		context.Request.EnableBuffering();
		using StreamReader streamReader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
		string req = await streamReader.ReadToEndAsync();
		context.Request.Body.Position = 0;

		Stream originalResponseBodyStream = context.Response.Body;

		using MemoryStream responseBody = new();
		context.Response.Body = responseBody;

		await next(context);

		context.Response.Body.Seek(0, SeekOrigin.Begin);
		string res = await new StreamReader(context.Response.Body).ReadToEndAsync();
		context.Response.Body.Seek(0, SeekOrigin.Begin);

		bool isSuccessful = context.Response.StatusCode is >= 200 and <= 299;

		stopwatch.Stop();
		logger.Log(
			isSuccessful ? LogLevel.Information : LogLevel.Critical,
			$"""
			 {stopwatch.ElapsedMilliseconds} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode}
			 {context.Request.Headers.ToJsonObject()}
			 {req}
			 {res}
			 """
		);

		await responseBody.CopyToAsync(originalResponseBodyStream);
	}
}

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration) {
	public async Task Invoke(HttpContext context) {
		if ((context.Request.Path.Value ?? "").Contains("api"))
			if (!context.Request.Headers.TryGetValue("X-API-Key", out StringValues apiKey) || apiKey != configuration.GetValue<string>("ApiKey")!) {
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				return;
			}

		await next(context);
	}
}