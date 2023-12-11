namespace Utilities_aspnet.Repositories;

public interface IMediaRepository {
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model);
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Filter(MediaFilterDto dto);
	Task<GenericResponse<MediaEntity?>> Update(Guid id, UpdateMediaDto model);
	Task<GenericResponse> Delete(Guid id);
	Task DeleteMedia(IEnumerable<MediaEntity?>? media);
}

public class MediaRepository(IWebHostEnvironment env, DbContext dbContext, IAmazonS3Repository amazonS3Repository) : IMediaRepository {
	public async Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model) {
		List<MediaEntity> medias = [];

		if (model.File is not null) {
			List<string> allowedExtensions = [".png", ".gif", ".jpg", ".jpeg", ".mp4", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar"];
			if (!allowedExtensions.Contains(Path.GetExtension(model.File!.FileName.ToLower())))
				return new GenericResponse<IEnumerable<MediaEntity>?>(null, UtilitiesStatusCodes.BadRequest);

			string folderName = "";
			if (model.UserId.IsNotNullOrEmpty()) folderName = "users/";
			else if (model.ProductId.IsNotNullOrEmpty()) folderName = "products/";
			else if (model.ContentId.IsNotNullOrEmpty()) folderName = "contents/";
			else if (model.CategoryId.IsNotNullOrEmpty()) folderName = "categories/";
			else if (model.ChatId.IsNotNullOrEmpty()) folderName = "chats/";
			else if (model.CommentId.IsNotNullOrEmpty()) folderName = "comments/";
			else if (model.BookmarkId.IsNotNullOrEmpty()) folderName = "bookmarks/";
			else if (model.NotificationId.IsNotNullOrEmpty()) folderName = "notifications/";
			else if (model.GroupChatId.IsNotNullOrEmpty()) folderName = "groupChats/";
			else if (model.GroupChatMessageId.IsNotNullOrEmpty()) folderName = "groupChatMessages/";
			string name = $"{folderName}{Guid.NewGuid() + Path.GetExtension(model.File.FileName)}";
			MediaEntity media = new() {
				FileName = name,
				UserId = model.UserId.IsNotNullOrEmpty() ? model.UserId : null,
				ProductId = model.UserId.IsNotNullOrEmpty() ? model.ProductId : null,
				ContentId = model.UserId.IsNotNullOrEmpty() ? model.ContentId : null,
				CategoryId = model.UserId.IsNotNullOrEmpty() ? model.CategoryId : null,
				ChatId = model.UserId.IsNotNullOrEmpty() ? model.ChatId : null,
				CommentId = model.UserId.IsNotNullOrEmpty() ? model.CommentId : null,
				BookmarkId = model.UserId.IsNotNullOrEmpty() ? model.BookmarkId : null,
				CreatedAt = DateTime.UtcNow,
				Order = model.Order,
				NotificationId = model.NotificationId,
				GroupChatId = model.GroupChatId,
				GroupChatMessageId = model.GroupChatMessageId,
				Tags = model.Tags,
				JsonDetail = {
					Title = model.Title,
					IsPrivate = model.PrivacyType,
					Size = model.Size,
					Time = model.Time,
					Artist = model.Artist,
					Album = model.Album
				}
			};
			await dbContext.Set<MediaEntity>().AddAsync(media);
			await dbContext.SaveChangesAsync();
			medias.Add(media);
			string path = SaveMedia(model.File, name);
			AmazonS3Settings amazonS3Settings = AppSettings.GetCurrentSettings().AmazonS3Settings;
			if (amazonS3Settings.UseS3 ?? false)
				await amazonS3Repository.UploadObjectFromFileAsync(amazonS3Settings.DefaultBucket!, name, path);
		}

		if (model.Links != null)
			foreach (MediaEntity media in model.Links.Select(_ => new MediaEntity {
				         UserId = model.UserId,
				         ProductId = model.ProductId,
				         ContentId = model.ContentId,
				         CategoryId = model.CategoryId,
				         ChatId = model.ChatId,
				         CommentId = model.CommentId,
				         CreatedAt = DateTime.UtcNow,
				         NotificationId = model.NotificationId,
				         Order = model.Order,
				         Tags = model.Tags,
				         JsonDetail = {
					         Title = model.Title,
					         IsPrivate = model.PrivacyType,
					         Size = model.Size,
					         Time = model.Time,
					         Artist = model.Artist,
					         Album = model.Album
				         }
			         })) {
				await dbContext.Set<MediaEntity>().AddAsync(media);
				await dbContext.SaveChangesAsync();
				medias.Add(media);
			}

		return new GenericResponse<IEnumerable<MediaEntity>?>(medias, message: Path.Combine(env.WebRootPath, "Medias"));
	}

	public async Task<GenericResponse<IEnumerable<MediaEntity>?>> Filter(MediaFilterDto dto) {
		IQueryable<MediaEntity> q = dbContext.Set<MediaEntity>().AsNoTracking();

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IEnumerable<MediaEntity>?>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> Delete(Guid id) {
		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		try {
			File.Delete(Path.Combine(env.WebRootPath, "Medias", media.FileName!));
		}
		catch (Exception) { }

		AmazonS3Settings amazonS3Settings = AppSettings.GetCurrentSettings().AmazonS3Settings;
		if (amazonS3Settings.UseS3 ?? false)
			await amazonS3Repository.DeleteObject(amazonS3Settings.DefaultBucket!, media.FileName!);

		dbContext.Set<MediaEntity>().Remove(media);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task DeleteMedia(IEnumerable<MediaEntity?>? media) {
		if (media is not null) {
			IEnumerable<MediaEntity?> mediaEntities = media.ToList();
			if (mediaEntities.IsNotNullOrEmpty())
				foreach (MediaEntity? i in mediaEntities)
					if (i is not null)
						await Delete(i.Id);
		}
	}

	public async Task<GenericResponse<MediaEntity?>> Update(Guid id, UpdateMediaDto model) {
		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media is null)
			throw new Exception("media is not found");

		media.JsonDetail.Title = model.Title ?? media.JsonDetail.Title;
		media.JsonDetail.Size = model.Size ?? media.JsonDetail.Size;
		media.JsonDetail.Time = model.Time ?? media.JsonDetail.Time;
		media.JsonDetail.Artist = model.Artist ?? media.JsonDetail.Artist;
		media.JsonDetail.Album = model.Album ?? media.JsonDetail.Album;
		media.UpdatedAt = DateTime.UtcNow;
		media.Tags = model.Tags ?? media.Tags;
		media.Order = model.Order ?? media.Order;

		if (model.RemoveTags.IsNotNullOrEmpty()) {
			model.RemoveTags?.ForEach(item => media.Tags?.Remove(item));
		}

		if (model.AddTags.IsNotNullOrEmpty()) {
			media.Tags?.AddRange(model.AddTags!);
		}

		dbContext.Set<MediaEntity>().Update(media);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<MediaEntity?>(media);
	}

	private string SaveMedia(IFormFile image, string name) {
		string webRoot = env.WebRootPath;
		string path = Path.Combine(webRoot, "Medias", name);
		string uploadDir = Path.Combine(webRoot, "Medias");
		if (!Directory.Exists(uploadDir))
			Directory.CreateDirectory(uploadDir);
		try {
			File.Delete(path);
		}
		catch (Exception ex) {
			throw new ArgumentException("Exception in SaveMedia- Delete! " + ex.Message);
		}

		using FileStream stream = new(path, FileMode.Create);
		image.CopyTo(stream);

		return path;
	}
}