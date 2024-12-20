namespace Utilities_aspnet.Repositories;

// public interface IAmazonS3Repository {
// 	Task UploadObjectFromFileAsync(string bucketName, string objectName, string filePath);
// 	Task UploadObjectAsync(string bucketName, string keyName, string filePath);
// 	Task ReadObjectDataAsync(string bucketName, string keyName);
// 	string? GeneratePreSignedUrl(string bucketName, string keyName);
// 	Task ListingObjectsAsync(string bucketName);
// 	Task DeleteObject(string bucketName, string objectName);
// 	Task DeleteObjects(string bucketName);
// }
//
// public class AmazonS3Repository : IAmazonS3Repository {
// 	private IAmazonS3 Authenticate() {
// 		AmazonS3Settings amazonS3Settings = AppSettings.Settings.AmazonS3Settings;
// 		return new AmazonS3Client(
// 			new BasicAWSCredentials(amazonS3Settings.AccessKey, amazonS3Settings.SecretKey),
// 			new AmazonS3Config { ServiceURL = amazonS3Settings.Url });
// 	}
//
//
// 	public async Task DeleteObjects(string bucketName) {
// 		DeleteObjectsRequest request = new() {
// 			BucketName = bucketName,
// 			Objects = [
// 				new KeyVersion { Key = "Item1" },
// 				new KeyVersion { Key = "Item2", VersionId = "Rej8CiBxcZKVK81cLr39j27Y5FVXghDK" },
// 				new KeyVersion { Key = "Logs/error.txt" }
// 			]
// 		};
// 		try {
// 			await Authenticate().DeleteObjectsAsync(request);
//
// 			Console.WriteLine($"Objects successfully deleted from {bucketName} bucket");
// 		}
// 		catch (AmazonS3Exception amazonS3Exception) {
// 			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
// 		}
// 		catch (Exception e) {
// 			Console.WriteLine("Exception: " + e);
// 		}
// 	}
//
// 	public async Task DeleteObject(string bucketName, string objectName) {
// 		try {
// 			// Create the request
// 			DeleteObjectsRequest request = new() {
// 				BucketName = bucketName,
// 				Objects = [new KeyVersion { Key = objectName, VersionId = null }]
// 			};
//
// 			// Submit the request
// 			DeleteObjectsResponse response = await Authenticate().DeleteObjectsAsync(request);
//
// 			Console.WriteLine(response.EncodeJson());
// 			Console.WriteLine($"Object {objectName} successfully deleted from {bucketName} bucket");
// 		}
// 		catch (AmazonS3Exception amazonS3Exception) {
// 			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
// 		}
// 		catch (Exception e) {
// 			Console.WriteLine("Exception: " + e);
// 		}
// 	}
//
// 	public string? GeneratePreSignedUrl(string bucketName, string objectName) {
// 		try {
// 			GetPreSignedUrlRequest getPreSignedUrlRequest = new() { BucketName = bucketName, Key = objectName, Verb = HttpVerb.GET };
// 			return Authenticate().GetPreSignedURL(getPreSignedUrlRequest);
// 		}
// 		catch (AmazonS3Exception amazonS3Exception) {
// 			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
// 		}
// 		catch (Exception e) {
// 			Console.WriteLine("Exception: " + e);
// 		}
//
// 		return null;
// 	}
//
// 	public async Task ListingObjectsAsync(string bucketName) {
// 		try {
// 			ListObjectsRequest? request = new() { BucketName = bucketName, MaxKeys = 5 };
//
// 			do {
// 				ListObjectsResponse response = await Authenticate().ListObjectsAsync(request);
//
// 				// Process the response.
// 				response.S3Objects.ForEach(obj => Console.WriteLine($"{obj.Key,-35}{obj.LastModified.ToShortDateString(),10}{obj.Size,10}"));
//
// 				// If the response is truncated, set the marker to get the next
// 				// set of keys.
// 				if (response.IsTruncated) {
// 					request.Marker = response.NextMarker;
// 				}
// 				else {
// 					request = null;
// 				}
// 			} while (request != null);
// 		}
// 		catch (AmazonS3Exception ex) {
// 			Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
// 		}
// 	}
//
// 	public async Task ReadObjectDataAsync(string bucketName, string keyName) {
// 		string responseBody = string.Empty;
//
// 		try {
// 			GetObjectRequest request = new() {
// 				BucketName = bucketName,
// 				Key = keyName
// 			};
//
// 			using GetObjectResponse response = await Authenticate().GetObjectAsync(request);
// 			await using Stream responseStream = response.ResponseStream;
// 			using StreamReader reader = new(responseStream);
// 			// Assume you have "title" as medata added to the object.
// 			string title = response.Metadata["x-amz-meta-title"];
// 			string contentType = response.Headers["Content-Type"];
//
// 			Console.WriteLine($"Object metadata, Title: {title}");
// 			Console.WriteLine($"Content type: {contentType}");
//
// 			// Retrieve the contents of the file.
// 			responseBody = await reader.ReadToEndAsync();
//
// 			// Write the contents of the file to disk.
// 			string filePath = keyName;
//
// 			Console.WriteLine("File successfully downloaded");
// 		}
// 		catch (AmazonS3Exception e) {
// 			// If the bucket or the object do not exist
// 			Console.WriteLine($"Error: '{e.Message}'");
// 		}
// 	}
//
// 	public async Task UploadObjectAsync(string bucketName, string keyName, string filePath) {
// 		// Create list to store upload part responses.
// 		List<UploadPartResponse> uploadResponses = [];
//
// 		// Setup information required to initiate the multipart upload.
// 		InitiateMultipartUploadRequest initiateRequest = new() { BucketName = bucketName, Key = keyName };
//
// 		// Initiate the upload.
// 		InitiateMultipartUploadResponse initResponse = await Authenticate().InitiateMultipartUploadAsync(initiateRequest);
//
// 		// Upload parts.
// 		long contentLength = new FileInfo(filePath).Length;
// 		long partSize = 400 * (long)Math.Pow(2, 20); // 400 MB
//
// 		try {
// 			Console.WriteLine("Uploading parts");
//
// 			long filePosition = 0;
// 			for (int i = 1; filePosition < contentLength; i++) {
// 				UploadPartRequest uploadRequest = new() {
// 					BucketName = bucketName,
// 					Key = keyName,
// 					UploadId = initResponse.UploadId,
// 					PartNumber = i,
// 					PartSize = partSize,
// 					FilePosition = filePosition,
// 					FilePath = filePath
// 				};
//
// 				// Upload a part and add the response to our list.
// 				uploadResponses.Add(await Authenticate().UploadPartAsync(uploadRequest));
//
// 				filePosition += partSize;
// 			}
//
// 			// Setup to complete the upload.
// 			CompleteMultipartUploadRequest completeRequest = new() {
// 				BucketName = bucketName,
// 				Key = keyName,
// 				UploadId = initResponse.UploadId
// 			};
// 			completeRequest.AddPartETags(uploadResponses);
//
// 			// Complete the upload.
// 			CompleteMultipartUploadResponse completeUploadResponse = await Authenticate().CompleteMultipartUploadAsync(completeRequest);
//
// 			Console.WriteLine($"Object {keyName} added to {bucketName} bucket");
// 		}
// 		catch (Exception exception) {
// 			Console.WriteLine($"An AmazonS3Exception was thrown: {exception.Message}");
//
// 			// Abort the upload.
// 			AbortMultipartUploadRequest abortMPURequest = new() {
// 				BucketName = bucketName,
// 				Key = keyName,
// 				UploadId = initResponse.UploadId
// 			};
// 			await Authenticate().AbortMultipartUploadAsync(abortMPURequest);
// 		}
// 	}
//
// 	public async Task UploadObjectFromFileAsync(string bucketName, string objectName, string filePath) {
// 		try {
// 			PutObjectRequest putRequest = new() { BucketName = bucketName, Key = objectName, FilePath = filePath, ContentType = "text/plain" };
//
// 			putRequest.Metadata.Add("x-amz-meta-title", "someTitle");
//
// 			PutObjectResponse response = await Authenticate().PutObjectAsync(putRequest);
//
// 			foreach (PropertyInfo prop in response.GetType().GetProperties()) {
// 				Console.WriteLine($"{prop.Name}: {prop.GetValue(response, null)}");
// 			}
//
// 			Console.WriteLine($"Object {objectName} added to {bucketName} bucket");
// 		}
// 		catch (AmazonS3Exception e) {
// 			Console.WriteLine($"Error: {e.Message}");
// 		}
// 	}
// }