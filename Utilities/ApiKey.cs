using System.Security.Cryptography;
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
	private static readonly string encryptionKey = "dxrytuhkjbmvncbxfdghtgjyhbmvcbxf"; // Should be a 16, 24, or 32 byte key for AES  
	
	public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next) {
		if (context.Result is ObjectResult objectResult && objectResult.Value != null) {
			string jsonString = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
			string encryptedData = EncryptString(jsonString, encryptionKey);
			context.Result = new ContentResult {
				Content = encryptedData,
				ContentType = "application/json",
				StatusCode = 200
			};
			await next();
		}
	}

	private static string EncryptString(string plainText, string key) {
		using Aes aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(key);
		aes.GenerateIV();

		using MemoryStream msEncrypt = new();
		msEncrypt.Write(aes.IV, 0, aes.IV.Length);

		using (CryptoStream cryptoStream = new(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
		using (StreamWriter streamWriter = new(cryptoStream)) {
			streamWriter.Write(plainText);
		}

		byte[] encrypted = msEncrypt.ToArray();
		return Convert.ToBase64String(encrypted);
	}
}