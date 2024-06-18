using Microsoft.AspNetCore.Mvc.Filters;

namespace Utilities_aspnet.Utilities;

public class ApiKeyConstant {
	public const string ApiKeyHeaderName = "X-API-Key";
	public const string ApiKeyName = "ApiKey";
}

public interface IApiKeyValidation {
	bool IsValidApiKey(string userApiKey);
}

public class ApiKeyValidation(IConfiguration configuration) : IApiKeyValidation {
	public bool IsValidApiKey(string userApiKey) {
		if (string.IsNullOrWhiteSpace(userApiKey)) return false;
		string? apiKey = configuration.GetValue<string>(ApiKeyConstant.ApiKeyName);
		return apiKey != null && apiKey == userApiKey;
	}
}

public class ApiKeyAttribute() : ServiceFilterAttribute(typeof(ApiKeyAuthFilter));

public class ApiKeyAuthFilter(IApiKeyValidation apiKeyValidation) : IAuthorizationFilter {
	public void OnAuthorization(AuthorizationFilterContext context) {
		string userApiKey = context.HttpContext.Request.Headers[ApiKeyConstant.ApiKeyHeaderName].ToString();
		if (string.IsNullOrWhiteSpace(userApiKey)) {
			context.Result = new BadRequestResult();
			return;
		}

		if (!apiKeyValidation.IsValidApiKey(userApiKey))
			context.Result = new UnauthorizedResult();
	}
}