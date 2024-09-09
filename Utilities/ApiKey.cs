using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Utilities_aspnet.Utilities;

//
// Api Response Encryption
//

public interface IApiKeyValidation {
	bool IsValidApiKey(string userApiKey);
}

public class ApiKeyValidation(IConfiguration configuration) : IApiKeyValidation {
	public bool IsValidApiKey(string userApiKey) {
		if (string.IsNullOrWhiteSpace(userApiKey)) return false;
		string? apiKey = configuration.GetValue<string>("ApiKey");
		return apiKey != null && apiKey == userApiKey;
	}
}

public class ApiKeyAttribute() : ServiceFilterAttribute(typeof(ApiKeyAuthFilter));

public class ApiKeyAuthFilter(IApiKeyValidation apiKeyValidation) : IAuthorizationFilter {
	public void OnAuthorization(AuthorizationFilterContext context) {
		string userApiKey = context.HttpContext.Request.Headers["X-API-Key"].ToString();
		if (string.IsNullOrWhiteSpace(userApiKey)) {
			context.Result = new BadRequestResult();
			return;
		}

		if (!apiKeyValidation.IsValidApiKey(userApiKey))
			context.Result = new UnauthorizedResult();
	}
}

//
// Api Response Encryption
//
public class EncryptResponseAttribute : ActionFilterAttribute {
	public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next) {
		if (context.Result is ObjectResult objectResult && objectResult.Value != null) {
			string jsonString = JsonConvert.SerializeObject(objectResult.Value,
				new JsonSerializerSettings {
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					NullValueHandling = NullValueHandling.Ignore,
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