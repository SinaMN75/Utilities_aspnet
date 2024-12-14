namespace Utilities_aspnet.Utilities;

public class EncryptResponseAttribute : ActionFilterAttribute {
	public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next) {
		if (context.Result is ObjectResult { Value: not null } objectResult) {
			string jsonString = JsonConvert.SerializeObject(objectResult.Value,
				new JsonSerializerSettings {
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					NullValueHandling = NullValueHandling.Ignore
				}
			);
			string encryptedData = Encryption.Base64Encode(jsonString);
			context.Result = new ContentResult {
				Content = encryptedData,
				ContentType = "application/text",
				StatusCode = context.HttpContext.Response.StatusCode
			};
			await next();
		}
	}
}

public class EncryptParamsAttribute() : TypeFilterAttribute(typeof(Base64DecodeFilter)) {
	private class Base64DecodeFilter : IAsyncActionFilter {
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
			if (context.HttpContext.Request.Method == HttpMethods.Post || context.HttpContext.Request.Method == HttpMethods.Put) {
				context.HttpContext.Request.EnableBuffering();

				using StreamReader reader = new(context.HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
				string body = await reader.ReadToEndAsync();
				string decryptedBody = Encryption.Base64Decode(body);

				context.HttpContext.Request.Body.Position = 0;
				await using (StreamWriter writer = new(context.HttpContext.Request.Body)) {
					await writer.WriteAsync(decryptedBody);
					await writer.FlushAsync();
				}

				context.HttpContext.Request.Body.Position = 0;
			}

			await next();
		}
	}
}

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger) {
	public async Task InvokeAsync(HttpContext context) {
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
		
		if (context.Response.StatusCode >= 300)
			logger.LogCritical(
				$"""
				 {context.Request.Method} {context.Request.Path} {context.Response.StatusCode}
				 {context.Request.Headers.EncodeJson()}
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