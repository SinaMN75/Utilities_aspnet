using System.Net;
using Amazon.S3.Transfer;

namespace Utilities_aspnet.Repositories;

public interface IS3Repository {
	Task<GenericResponse> CreateBucketAsync(string bucketName);
	Task<GenericResponse> DeleteBucketAsync(string bucketName);
	Task<GenericResponse> UploadFileAsync(string bucketName, string filePath);
	Task<GenericResponse> GetObjectFromS3Async(string bucketName, string keyName);
	Task<GenericResponse> DeleteObjectFromS3Async(string bucketName, string keyName);
}

public class S3Repository : IS3Repository {
	private readonly IAmazonS3 _awsS3Client;

	// You need to manually create the file in this path or change it yourself
	// private const string FilePathToUpload = @"C:\Test-Files\test.txt";

	// You need to manually create this directory in your OS, or change the path
	private const string FilePathToDownload = "C:\\Test-Files\\";

	public S3Repository(IAmazonS3 awsS3Client) {
		_awsS3Client = awsS3Client;
	}

	/// <summary>
	///     Create a bucket sending the Name
	/// </summary>
	/// <param name="bucketName">Bucket Name (string)</param>
	/// <returns></returns>
	public async Task<GenericResponse> CreateBucketAsync(string bucketName) {
		try {
			bool existBucket = await AmazonS3Util.DoesS3BucketExistV2Async(_awsS3Client, bucketName);

			if (existBucket)
				return new GenericResponse(UtilitiesStatusCodes.Conflict, "The bucket already exists");

			PutBucketRequest putBucketRequest = new() {
				BucketName = bucketName,
				UseClientRegion = true
			};

			PutBucketResponse? response = await _awsS3Client.PutBucketAsync(putBucketRequest);

			return new GenericResponse(UtilitiesStatusCodes.S3Error, $"Bucket with name {bucketName} was successfully created");
		}
		catch (AmazonS3Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
		catch (Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
	}

	/// <summary>
	///     Deleting a bucket sending the Name
	/// </summary>
	/// <param name="bucketName">Bucket name to delete</param>
	/// <returns></returns>
	public async Task<GenericResponse> DeleteBucketAsync(string bucketName) {
		try {
			bool existBucket = await AmazonS3Util.DoesS3BucketExistV2Async(_awsS3Client, bucketName);

			if (!existBucket)
				return new GenericResponse(UtilitiesStatusCodes.NotFound, $"Bucket with name {bucketName} was not found");

			DeleteBucketResponse? response = await _awsS3Client.DeleteBucketAsync(bucketName);

			return new GenericResponse(UtilitiesStatusCodes.S3Error, response.ResponseMetadata.RequestId);
		}
		catch (AmazonS3Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
		catch (Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
	}

	/// <summary>
	///     Upload a file to an S3, here four files are uploaded in four different ways
	/// </summary>
	/// <param name="bucketName">Upload a file to a bucket</param>
	/// <returns></returns>
	public async Task<GenericResponse> UploadFileAsync(string bucketName, string filePath) {
		try {
			TransferUtility fileTransferUtility = new(_awsS3Client);
			await fileTransferUtility.UploadAsync(filePath, bucketName);
		}
		catch (AmazonS3Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
		catch (Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}

		return new GenericResponse(UtilitiesStatusCodes.S3Error, "File uploaded Successfully");
	}

	/// <summary>
	///     Get a file from S3
	/// </summary>
	/// <param name="bucketName">Bucket where the file is stored</param>
	/// <param name="keyName">Key name of the file (file name including extension)</param>
	/// <returns></returns>
	public async Task<GenericResponse> GetObjectFromS3Async(string bucketName, string keyName) {
		if (string.IsNullOrEmpty(keyName))
			keyName = "test.txt";

		try {
			GetObjectRequest request = new() {
				BucketName = bucketName,
				Key = keyName
			};
			string responseBody;

			using (GetObjectResponse? response = await _awsS3Client.GetObjectAsync(request))
			using (Stream? responseStream = response.ResponseStream)
			using (StreamReader reader = new(responseStream)) {
				string? title = response.Metadata["x-amz-meta-title"];
				string? contentType = response.Headers["Content-Type"];
				responseBody = await reader.ReadToEndAsync();
			}

			string createText = responseBody;
			await File.WriteAllTextAsync(FilePathToDownload + keyName, createText);
		}
		catch (AmazonS3Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
		catch (Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}

		return new GenericResponse(UtilitiesStatusCodes.S3Error, "Success");
	}

	/// <summary>
	///     Delete a file from an S3 bucket
	/// </summary>
	/// <param name="bucketName">Bucket where file is stored</param>
	/// <param name="keyName">Key name of the file (file name including extension)</param>
	/// <returns></returns>
	public async Task<GenericResponse> DeleteObjectFromS3Async(string bucketName, string keyName) {
		if (string.IsNullOrEmpty(keyName))
			keyName = "test.txt";

		try {
			GetObjectRequest request = new() { BucketName = bucketName, Key = keyName };

			GetObjectResponse? response = await _awsS3Client.GetObjectAsync(request);

			if (response is not { HttpStatusCode: HttpStatusCode.OK })
				return new GenericResponse(UtilitiesStatusCodes.S3Error, "Error getting the object from the bucket");

			await _awsS3Client.DeleteObjectAsync(bucketName, keyName);

			return new GenericResponse(UtilitiesStatusCodes.S3Error, "The file was successfully deleted");
		}
		catch (AmazonS3Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
		catch (Exception e) {
			return new GenericResponse(UtilitiesStatusCodes.S3Error, e.Message);
		}
	}
}