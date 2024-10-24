using System.Security.Cryptography;
using System.Text;

namespace Utilities_aspnet.Utilities;

public class Encryption {
	[Obsolete("Obsolete")]
	private void Example() {
		byte[] aesKey = new byte[32]; // 256-bit key for AES  
		byte[] aesIv = new byte[16]; // 128-bit IV for AES  
		new RNGCryptoServiceProvider().GetBytes(aesKey);
		new RNGCryptoServiceProvider().GetBytes(aesIv);

		string encrypted = EncryptAes("Hello, World!", aesKey, aesIv);
		string decrypted = DecryptAes(encrypted, aesKey, aesIv);
		Console.WriteLine($"Encrypted: {encrypted}");
		Console.WriteLine($"Decrypted: {decrypted}");
	}

	// Base64 Encryption/Decryption  
	public static string Base64Encode(string plainText) {
		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(plainTextBytes);
	}

	public static string Base64Decode(string base64EncodedData) {
		byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(base64EncodedBytes);
	}

	// AES Encryption/Decryption  
	public static string EncryptAes(string plainText, byte[] key, byte[] iv) {
		using Aes aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
		using (MemoryStream ms = new()) {
			using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
			using (StreamWriter writer = new(cs)) {
				writer.Write(plainText);
			}

			return Convert.ToBase64String(ms.ToArray());
		}
	}

	public static string DecryptAes(string cipherText, byte[] key, byte[] iv) {
		using Aes aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
		using (MemoryStream ms = new(Convert.FromBase64String(cipherText)))
		using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
		using (StreamReader reader = new(cs)) {
			return reader.ReadToEnd();
		}
	}

	// RSA Encryption/Decryption  
	public static string EncryptRsa(string plainText, RSAParameters publicKey) {
		using RSACryptoServiceProvider rsa = new();
		rsa.ImportParameters(publicKey);
		byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
		byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
		return Convert.ToBase64String(encryptedData);
	}

	public static string DecryptRsa(string cipherText, RSAParameters privateKey) {
		using RSACryptoServiceProvider rsa = new();
		rsa.ImportParameters(privateKey);
		byte[] dataToDecrypt = Convert.FromBase64String(cipherText);
		byte[] decryptedData = rsa.Decrypt(dataToDecrypt, false);
		return Encoding.UTF8.GetString(decryptedData);
	}
}