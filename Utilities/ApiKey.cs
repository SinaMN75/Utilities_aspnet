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
			string jsonString = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
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