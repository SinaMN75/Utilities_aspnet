namespace Utilities_aspnet.Repositories;

public interface IMediaRepository {
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model);
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Filter(MediaFilterDto dto);
	Task<GenericResponse<MediaEntity?>> Update(UpdateMediaDto model);
	Task<GenericResponse> Delete(Guid id);
	Task DeleteMedia(IEnumerable<MediaEntity?>? media);
}

public class MediaRepository(
	IWebHostEnvironment env,
	DbContext dbContext,
	IAmazonS3Repository amazonS3Repository,
	IConfiguration config
) : IMediaRepository {
	public async Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model) {
		List<MediaEntity> medias = [];

		if (model.File is not null) {
			List<string> allowedExtensions = [".png", ".gif", ".jpg", ".jpeg",".svg", ".mp4", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar", ".mkv"];
			if (!allowedExtensions.Contains(Path.GetExtension(model.File!.FileName.ToLower())))
				return new GenericResponse<IEnumerable<MediaEntity>?>(null, UtilitiesStatusCodes.BadRequest);

			string folderName = "";
			if (model.UserId is not null) folderName = "users/";
			else if (model.ProductId is not null) folderName = "products/";
			else if (model.ContentId is not null) folderName = "contents/";
			else if (model.CategoryId is not null) folderName = "categories/";
			else if (model.ChatId is not null) folderName = "chats/";
			else if (model.CommentId is not null) folderName = "comments/";
			else if (model.BookmarkId is not null) folderName = "bookmarks/";
			else if (model.NotificationId is not null) folderName = "notifications/";
			else if (model.GroupChatId is not null) folderName = "groupChats/";
			else if (model.GroupChatMessageId is not null) folderName = "groupChatsMessages/";
			string name = $"{folderName}{Guid.NewGuid() + Path.GetExtension(model.File.FileName)}";
			MediaEntity media = new() {
				Id = model.Id ?? Guid.NewGuid(),
				FileName = name,
				UserId = model.UserId,
				ProductId = model.ProductId,
				ContentId = model.ContentId,
				CategoryId = model.CategoryId,
				ChatId = model.ChatId,
				CommentId = model.CommentId,
				BookmarkId = model.BookmarkId,
				CreatedAt = DateTime.UtcNow,
				Order = model.Order,
				NotificationId = model.NotificationId,
				GroupChatId = model.GroupChatId,
				GroupChatMessageId = model.GroupChatMessageId,
				ParentId = model.ParentId,
				Tags = model.Tags,
				JsonDetail = {
					Title = model.Title,
					Description = model.Description,
					IsPrivate = model.PrivacyType,
					Size = model.Size,
					Time = model.Time,
					Artist = model.Artist,
					Album = model.Album,
					Link1 = model.Link1,
					Link2 = model.Link2,
					Link3 = model.Link3
				}
			};
			await dbContext.Set<MediaEntity>().AddAsync(media);
			await dbContext.SaveChangesAsync();
			medias.Add(media);
			string path = await SaveMedia(model.File, name);
			AppSettings appSettings = new();
			config.GetSection("AppSettings").Bind(appSettings);
			AmazonS3Settings amazonS3Settings = appSettings.AmazonS3Settings;
			if (amazonS3Settings.UseS3 ?? false)
				await amazonS3Repository.UploadObjectFromFileAsync(amazonS3Settings.DefaultBucket!, name, path);
		}

		if (model.Links == null) return new GenericResponse<IEnumerable<MediaEntity>?>(medias, message: Path.Combine(env.WebRootPath, "Medias"));
		{
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
					         Description = model.Description,
					         IsPrivate = model.PrivacyType,
					         Size = model.Size,
					         Time = model.Time,
					         Artist = model.Artist,
					         Album = model.Album,
					         Link1 = model.Link1,
					         Link2 = model.Link2,
					         Link3 = model.Link3
				         }
			         })) {
				await dbContext.Set<MediaEntity>().AddAsync(media);
				await dbContext.SaveChangesAsync();
				medias.Add(media);
			}
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
		MediaEntity media = (await dbContext.Set<MediaEntity>()
			.Include(x => x.Children)
			.FirstOrDefaultAsync(x => x.Id == id))!;

		foreach (MediaEntity i in media.Children ?? new List<MediaEntity>()) await Delete(i.Id);

		try {
			File.Delete(Path.Combine(env.WebRootPath, "Medias", media.FileName!));
		}
		catch (Exception) { }

		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		AmazonS3Settings amazonS3Settings = appSettings.AmazonS3Settings;
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

	public async Task<GenericResponse<MediaEntity?>> Update(UpdateMediaDto model) {
		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == model.Id);
		if (media is null)
			throw new Exception("media is not found");

		media.JsonDetail.Title = model.Title ?? media.JsonDetail.Title;
		media.JsonDetail.Description = model.Description ?? media.JsonDetail.Description;
		media.JsonDetail.Size = model.Size ?? media.JsonDetail.Size;
		media.JsonDetail.Time = model.Time ?? media.JsonDetail.Time;
		media.JsonDetail.Artist = model.Artist ?? media.JsonDetail.Artist;
		media.JsonDetail.Album = model.Album ?? media.JsonDetail.Album;
		media.JsonDetail.Link1 = model.Link1 ?? media.JsonDetail.Link1;
		media.JsonDetail.Link2 = model.Link2 ?? media.JsonDetail.Link2;
		media.JsonDetail.Link3 = model.Link3 ?? media.JsonDetail.Link3;
		media.UpdatedAt = DateTime.UtcNow;
		media.Tags = model.Tags ?? [];
		media.Order = model.Order ?? media.Order;

		if (model.RemoveTags.IsNotNullOrEmpty()) {
			model.RemoveTags!.ForEach(item => media.Tags.Remove(item));
		}

		if (model.AddTags.IsNotNullOrEmpty()) {
			media.Tags.AddRange(model.AddTags!);
		}

		dbContext.Set<MediaEntity>().Update(media);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<MediaEntity?>(media);
	}

	private async Task<string> SaveMedia(IFormFile image, string name) {
		string webRoot = env.WebRootPath;
		string path = Path.Combine(webRoot, "Medias", name);
		string uploadDir = Path.Combine(webRoot, "Medias");
		if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
		try {
			File.Delete(path);
		}
		catch (Exception ex) {
			throw new ArgumentException("Exception in SaveMedia- Delete! " + ex.Message);
		}

		await using FileStream stream = new(path, FileMode.Create);
		await image.CopyToAsync(stream);
		
		return path;
	}
}