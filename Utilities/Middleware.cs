namespace Utilities_aspnet.Utilities;
//Encrypt/Decrypt
//public class EncryptionMiddleware : IMiddleware
//{
//    // The secret key shared between server and client
//    private readonly byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

//    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//    {
//        // Decrypt the request stream
//        await DecryptRequestAsync(context.Request);

//        // Call the next middleware in the pipeline
//        await next(context);

//        // Encrypt the response stream
//        await EncryptResponseAsync(context.Response);
//    }

//    private async Task DecryptRequestAsync(HttpRequest request)
//    {
//        // Create a memory stream to store the decrypted bytes
//        var decryptedStream = new MemoryStream();

//        // Get the original request stream
//        var originalStream = request.Body;

//        // Create an instance of RijndaelManaged
//        using (var rijndael = new RijndaelManaged())
//        {
//            // Set the key size to 128 bits
//            rijndael.KeySize = 128;
//            // Set the block size to 128 bits
//            rijndael.BlockSize = 128;
//            // Set the mode to Cipher Block Chaining
//            rijndael.Mode = CipherMode.CBC;
//            // Set the padding to Zero Byte Padding
//            rijndael.Padding = PaddingMode.Zeros;

//            // Read the IV bytes from the first 16 bytes of the original stream
//            var ivBytes = new byte[16];
//            await originalStream.ReadAsync(ivBytes, 0, ivBytes.Length);

//            // Create a decryptor from the key and the IV
//            using (var decryptor = rijndael.CreateDecryptor(key, ivBytes))
//            {
//                // Create a crypto stream from the original stream and the decryptor
//                using (var cryptoStream = new CryptoStream(originalStream, decryptor, CryptoStreamMode.Read))
//                {
//                    // Copy the decrypted bytes from the crypto stream to the memory stream
//                    await cryptoStream.CopyToAsync(decryptedStream);
//                }
//            }
//        }

//        // Rewind the memory stream
//        decryptedStream.Position = 0;

//        // Replace the request stream with the memory stream
//        request.Body = decryptedStream;
//    }

//    private async Task EncryptResponseAsync(Microsoft.AspNetCore.Http.HttpResponse response)
//    {
//        // Create a memory stream to store the encrypted bytes
//        var encryptedStream = new MemoryStream();

//        // Get the original response stream
//        var originalStream = response.Body;

//        // Create an instance of RijndaelManaged
//        using (var rijndael = new RijndaelManaged())
//        {
//            // Set the key size to 128 bits
//            rijndael.KeySize = 128;
//            // Set the block size to 128 bits
//            rijndael.BlockSize = 128;
//            // Set the mode to Cipher Block Chaining
//            rijndael.Mode = CipherMode.CBC;
//            // Set the padding to Zero Byte Padding
//            rijndael.Padding = PaddingMode.Zeros;

//            // Generate a random IV
//            rijndael.GenerateIV();
//            // Get the IV bytes
//            var ivBytes = rijndael.IV;

//            // Write the IV bytes to the memory stream
//            await encryptedStream.WriteAsync(ivBytes, 0, ivBytes.Length);

//            // Create an encryptor from the key and the IV
//            using (var encryptor = rijndael.CreateEncryptor(key, ivBytes))
//            {
//                // Create a crypto stream from the memory stream and the encryptor
//                using (var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
//                {
//                    // Copy the original bytes from the original stream to the crypto stream
//                    await originalStream.CopyToAsync(cryptoStream);
//                }
//            }
//        }

//        // Rewind the memory stream
//        encryptedStream.Position = 0;

//        // Replace the response stream with the memory stream
//        response.Body = encryptedStream;
//    }
//}