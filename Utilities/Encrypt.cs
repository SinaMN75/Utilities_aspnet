using System.Security.Cryptography;
using System.Text;

namespace Utilities_aspnet.Utilities;

public class Encryption {
	public static string EncryptString(string plainText, string key) {
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

	public static string Base64Encode(string plainText) {
		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(plainTextBytes);
	}

	public static string Base64Decode(string base64EncodedData) {
		byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(base64EncodedBytes);
	}
}