using System.Security.Cryptography;
using System.Text;

namespace Utilities_aspnet.Utilities;

public class Encryption {

	public static string Base64Encode(string plainText) {
		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(plainTextBytes);
	}

	public static string Base64Decode(string base64EncodedData) {
		byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(base64EncodedBytes);
	}
}