using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace Utilities_aspnet.Repositories;

public interface IArvanStorageRepository {
	IAmazonS3 Authenticate();
	Task UploadObjectFromFileAsync(IAmazonS3 client, string bucketName, string objectName, string filePath);
	Task PutBucketTagsAsync(IAmazonS3 client, string bucketName);
	Task DeleteBucketTaggingAsync(IAmazonS3 client, string bucketName);
	Task UploadObjectAsync(IAmazonS3 client, string bucketName, string keyName, string filePath);
	Task ReadObjectDataAsync(IAmazonS3 client, string bucketName, string keyName);
	Task ListingObjectsAsync(IAmazonS3 client, string bucketName);
	Task DeleteObject(IAmazonS3 client, string bucketName, string objectName);
	Task DeleteObjects(IAmazonS3 client, string bucketName);
}

public class ArvanStorageRepository : IArvanStorageRepository {
	public IAmazonS3 Authenticate() {
		BasicAWSCredentials awsCredentials = new("9236ce6e-ee60-4e1c-8485-dcae1366f45c", "27c91a47b8d8f50b922863c2637870376858c27bb1417000a9a95fb62f995866");
		AmazonS3Config config = new() { ServiceURL = "s3.ir-thr-at1.arvanstorage.ir" };
		return new AmazonS3Client(awsCredentials, config);
	}

	public async Task DeleteObjects(IAmazonS3 client, string bucketName) {
		DeleteObjectsRequest request = new() {
			BucketName = bucketName,
			Objects = new List<KeyVersion> {
				new() { Key = "Item1" },
				// Versioned item
				new() { Key = "Item2", VersionId = "Rej8CiBxcZKVK81cLr39j27Y5FVXghDK", },
				// Item in subdirectory
				new() { Key = "Logs/error.txt" }
			}
		};
		try {
			await client.DeleteObjectsAsync(request);

			Console.WriteLine($"Objects successfully deleted from {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	public async Task DeleteObject(IAmazonS3 client, string bucketName, string objectName) {
		try {
			// Create the request
			DeleteObjectsRequest request = new() {
				BucketName = bucketName,
				Objects = new List<KeyVersion> { new() { Key = objectName, VersionId = null } }
			};

			// Submit the request
			DeleteObjectsResponse response = await client.DeleteObjectsAsync(request);

			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
			Console.WriteLine($"Object {objectName} successfully deleted from {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	public async Task ListingObjectsAsync(IAmazonS3 client, string bucketName) {
		try {
			ListObjectsRequest? request = new() { BucketName = bucketName, MaxKeys = 5, };

			do {
				ListObjectsResponse response = await client.ListObjectsAsync(request);

				// Process the response.
				response.S3Objects.ForEach(obj => Console.WriteLine($"{obj.Key,-35}{obj.LastModified.ToShortDateString(),10}{obj.Size,10}"));

				// If the response is truncated, set the marker to get the next
				// set of keys.
				if (response.IsTruncated) {
					request.Marker = response.NextMarker;
				}
				else {
					request = null;
				}
			} while (request != null);
		}
		catch (AmazonS3Exception ex) {
			Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
		}
	}

	public async Task ReadObjectDataAsync(IAmazonS3 client, string bucketName, string keyName) {
		string responseBody = string.Empty;

		try {
			GetObjectRequest request = new() {
				BucketName = bucketName,
				Key = keyName,
			};

			using GetObjectResponse response = await client.GetObjectAsync(request);
			await using Stream responseStream = response.ResponseStream;
			using StreamReader reader = new(responseStream);
			// Assume you have "title" as medata added to the object.
			string title = response.Metadata["x-amz-meta-title"];
			string contentType = response.Headers["Content-Type"];

			Console.WriteLine($"Object metadata, Title: {title}");
			Console.WriteLine($"Content type: {contentType}");

			// Retrieve the contents of the file.
			responseBody = await reader.ReadToEndAsync();

			// Write the contents of the file to disk.
			string filePath = keyName;

			Console.WriteLine("File successfully downloaded");
		}
		catch (AmazonS3Exception e) {
			// If the bucket or the object do not exist
			Console.WriteLine($"Error: '{e.Message}'");
		}
	}

	public async Task UploadObjectAsync(IAmazonS3 client, string bucketName, string keyName, string filePath) {
		// Create list to store upload part responses.
		List<UploadPartResponse> uploadResponses = new();

		// Setup information required to initiate the multipart upload.
		InitiateMultipartUploadRequest initiateRequest = new() { BucketName = bucketName, Key = keyName, };

		// Initiate the upload.
		InitiateMultipartUploadResponse initResponse = await client.InitiateMultipartUploadAsync(initiateRequest);

		// Upload parts.
		long contentLength = new FileInfo(filePath).Length;
		long partSize = 400 * (long)Math.Pow(2, 20); // 400 MB

		try {
			Console.WriteLine("Uploading parts");

			long filePosition = 0;
			for (int i = 1; filePosition < contentLength; i++) {
				UploadPartRequest uploadRequest = new() {
					BucketName = bucketName,
					Key = keyName,
					UploadId = initResponse.UploadId,
					PartNumber = i,
					PartSize = partSize,
					FilePosition = filePosition,
					FilePath = filePath,
				};

				// Upload a part and add the response to our list.
				uploadResponses.Add(await client.UploadPartAsync(uploadRequest));

				filePosition += partSize;
			}

			// Setup to complete the upload.
			CompleteMultipartUploadRequest completeRequest = new() {
				BucketName = bucketName,
				Key = keyName,
				UploadId = initResponse.UploadId,
			};
			completeRequest.AddPartETags(uploadResponses);

			// Complete the upload.
			CompleteMultipartUploadResponse completeUploadResponse = await client.CompleteMultipartUploadAsync(completeRequest);

			Console.WriteLine($"Object {keyName} added to {bucketName} bucket");
		}
		catch (Exception exception) {
			Console.WriteLine($"An AmazonS3Exception was thrown: {exception.Message}");

			// Abort the upload.
			AbortMultipartUploadRequest abortMPURequest = new() {
				BucketName = bucketName,
				Key = keyName,
				UploadId = initResponse.UploadId,
			};
			await client.AbortMultipartUploadAsync(abortMPURequest);
		}
	}

	public async Task UploadObjectFromFileAsync(IAmazonS3 client, string bucketName, string objectName, string filePath) {
		try {
			PutObjectRequest putRequest = new() {
				BucketName = bucketName,
				Key = objectName,
				FilePath = filePath,
				ContentType = "text/plain"
			};

			putRequest.Metadata.Add("x-amz-meta-title", "someTitle");

			PutObjectResponse response = await client.PutObjectAsync(putRequest);

			foreach (PropertyInfo prop in response.GetType().GetProperties()) {
				Console.WriteLine($"{prop.Name}: {prop.GetValue(response, null)}");
			}

			Console.WriteLine($"Object {objectName} added to {bucketName} bucket");
		}
		catch (AmazonS3Exception e) {
			Console.WriteLine($"Error: {e.Message}");
		}
	}

	public async Task DeleteBucketTaggingAsync(IAmazonS3 client, string bucketName) {
		try {
			DeleteBucketTaggingRequest request = new() { BucketName = bucketName, };
			DeleteBucketTaggingResponse response = await client.DeleteBucketTaggingAsync(request);
			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
			Console.WriteLine($"Bucket tagging successfully deleted from {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	public async Task PutBucketTagsAsync(IAmazonS3 client, string bucketName) {
		try {
			List<Tag> tagList = new() {
				new Tag { Key = "Key1", Value = "Value1" },
				new Tag { Key = "Key2", Value = "Value2" }
			};

			PutBucketTaggingResponse response = await client.PutBucketTaggingAsync(bucketName, tagList);

			foreach (PropertyInfo prop in response.GetType().GetProperties()) {
				Console.WriteLine($"{prop.Name}: {prop.GetValue(response, null)}");
			}

			Console.WriteLine($"Tags added to {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task GetBucketTagsAsync(IAmazonS3 client, string bucketName) {
		try {
			GetBucketTaggingRequest request = new() {
				BucketName = bucketName,
			};

			GetBucketTaggingResponse response = await client.GetBucketTaggingAsync(request);

			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task GetBucketVersioningAsync(IAmazonS3 client, string bucketName) {
		try {
			GetBucketVersioningResponse response = await client.GetBucketVersioningAsync(new GetBucketVersioningRequest {
				BucketName = bucketName,
			});

			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task EnableBucketVersioningAsync(IAmazonS3 client, string bucketName) {
		try {
			PutBucketVersioningResponse response = await client.PutBucketVersioningAsync(new PutBucketVersioningRequest {
				BucketName = bucketName,
				VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled }
			});

			Console.WriteLine($"Versioning enabled in {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task PutBucketAclAsync(IAmazonS3 client, string bucketName, string acl = "public-read") {
		//acl = public-read OR private
		try {
			PutACLResponse response = await client.PutACLAsync(new PutACLRequest {
				BucketName = bucketName,
				CannedACL = acl == "private" ? S3CannedACL.Private : S3CannedACL.PublicRead, // S3CannedACL.PublicRead or S3CannedACL.Private
			});

			Console.WriteLine($"Access-list {acl} added to {bucketName} bucket");
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task GetBucketAclAsync(IAmazonS3 client, string bucketName) {
		try {
			GetACLResponse response = await client.GetACLAsync(new GetACLRequest { BucketName = bucketName, });

			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task DeleteBucketPolicy(IAmazonS3 client, string bucketName) {
		DeleteBucketPolicyRequest deleteRequest = new() { BucketName = bucketName, };
		object policy = await client.DeleteBucketPolicyAsync(deleteRequest);

		foreach (PropertyInfo prop in policy.GetType().GetProperties()) {
			Console.WriteLine($"{prop.Name}: {prop.GetValue(policy, null)}");
		}

		Console.WriteLine($"Policy successfully deleted from {bucketName} bucket");
	}

	private static async Task PutBucketPolicy(IAmazonS3 client, string bucketName) {
		const string newPolicy = @"{
              ""Statement"":[{
              ""Sid"":""PolicyName"",
              ""Effect"":""Allow"",
              ""Principal"": { ""AWS"": ""*"" },
              ""Action"":[""s3:PutObject"",""s3:GetObject""],
              ""Resource"":[""arn:aws:s3:::rezvani/user_*""]
          }]}";

		PutBucketPolicyRequest putRequest = new() { BucketName = bucketName, Policy = newPolicy };
		object policy = await client.PutBucketPolicyAsync(putRequest);
		foreach (PropertyInfo prop in policy.GetType().GetProperties()) {
			Console.WriteLine($"{prop.Name}: {prop.GetValue(policy, null)}");
		}

		Console.WriteLine($"Policy successfully added to {bucketName} bucket");
	}

	private static async Task GetBucketPolicyStatus(IAmazonS3 client, string bucketName) {
		try {
			GetBucketPolicyStatusResponse response = await client.GetBucketPolicyStatusAsync(new GetBucketPolicyStatusRequest { BucketName = bucketName });
			Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
		}
		catch (AmazonS3Exception amazonS3Exception) {
			Console.WriteLine("An AmazonS3Exception was thrown. Exception: " + amazonS3Exception);
		}
		catch (Exception e) {
			Console.WriteLine("Exception: " + e);
		}
	}

	private static async Task GetBucketPolicy(IAmazonS3 client, string bucketName) {
		GetBucketPolicyRequest getRequest = new() { BucketName = bucketName };
		object policy = await client.GetBucketPolicyAsync(getRequest);

		foreach (PropertyInfo prop in policy.GetType().GetProperties()) {
			Console.WriteLine($"{prop.Name}: {prop.GetValue(policy, null)}");
		}
	}

	private static async Task CreateBucket(IAmazonS3 client, string bucketName) {
		try {
			PutBucketRequest putBucketRequest = new() { BucketName = bucketName, UseClientRegion = true };
			PutBucketResponse? putBucketResponse = await client.PutBucketAsync(putBucketRequest);
		}
		catch (AmazonS3Exception ex) {
			Console.WriteLine($"Error creating bucket: '{ex.Message}'");
		}
	}

	private static async Task<bool> CheckBucketExist(IAmazonS3 client, string bucketName) => await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);

	private static async Task<ListBucketsResponse> GetBuckets(IAmazonS3 client) => await client.ListBucketsAsync();

	private static async Task<bool> DeleteBucket(IAmazonS3 client, string bucketName) {
		try {
			DeleteBucketResponse? deleteResponse = await client.DeleteBucketAsync(bucketName);
			Console.WriteLine($"\nResult: {deleteResponse.HttpStatusCode.ToString()}");
			return true;
		}
		catch (AmazonS3Exception ex) {
			Console.WriteLine($"Error: {ex.Message}");
			return false;
		}
	}
}