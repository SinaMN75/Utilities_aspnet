namespace Utilities_aspnet.Repositories;

public interface IMediaRepository {
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model);
	Task<GenericResponse<MediaEntity?>> UpdateMedia(Guid id, UpdateMediaDto model);
	Task<GenericResponse> Delete(Guid id);
	Task DeleteMedia(IEnumerable<MediaEntity?>? media);
}

public class MediaRepository(IWebHostEnvironment env, DbContext dbContext) : IMediaRepository {
	public async Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model) {
		List<MediaEntity> medias = new();

		if (model.Files != null)
			foreach (IFormFile file in model.Files) {
				List<string> allowedExtensions = new() { ".png", ".gif", ".jpg", ".jpeg", ".mp4", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar" };
				if (!allowedExtensions.Contains(Path.GetExtension(file.FileName.ToLower())))
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
				else if (model.GroupChatMessageId is not null) folderName = "groupChatMessages/";
				string name = $"{folderName}{Guid.NewGuid() + Path.GetExtension(file.FileName)}";
				MediaEntity media = new() {
					FileName = name,
					UserId = model.UserId,
					ProductId = model.ProductId,
					ContentId = model.ContentId,
					CategoryId = model.CategoryId,
					ChatId = model.ChatId,
					CommentId = model.CommentId,
					BookmarkId = model.BookmarkId,
					CreatedAt = DateTime.Now,
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
				SaveMedia(file, name);
			}

		if (model.Links != null)
			foreach (MediaEntity media in model.Links.Select(_ => new MediaEntity {
				         UserId = model.UserId,
				         ProductId = model.ProductId,
				         ContentId = model.ContentId,
				         CategoryId = model.CategoryId,
				         ChatId = model.ChatId,
				         CommentId = model.CommentId,
				         CreatedAt = DateTime.Now,
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

		return new GenericResponse<IEnumerable<MediaEntity>?>(medias);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		try {
			File.Delete(Path.Combine(env.WebRootPath, "Medias", media.FileName!));
		}
		catch (Exception) { }

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

	public async Task<GenericResponse<MediaEntity?>> UpdateMedia(Guid id, UpdateMediaDto model) {
		MediaEntity? media = await dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media is null)
			throw new Exception("media is not found");

		media.JsonDetail.Title = model.Title ?? media.JsonDetail.Title;
		media.JsonDetail.Size = model.Size ?? media.JsonDetail.Size;
		media.JsonDetail.Time = model.Time ?? media.JsonDetail.Time;
		media.JsonDetail.Artist = model.Artist ?? media.JsonDetail.Artist;
		media.JsonDetail.Album = model.Album ?? media.JsonDetail.Album;
		media.UpdatedAt = DateTime.Now;
		media.Tags = model.Tags ?? media.Tags;
		media.Order = model.Order ?? media.Order;

		if (model.RemoveTags.IsNotNullOrEmpty()) {
			model.RemoveTags.ForEach(item => media.Tags?.Remove(item));
		}

		if (model.AddTags.IsNotNullOrEmpty()) {
			media.Tags.AddRange(model.AddTags);
		}

		dbContext.Set<MediaEntity>().Update(media);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<MediaEntity?>(media);
	}

	public void SaveMedia(IFormFile image, string name) {
		string webRoot = env.WebRootPath;
		string path = Path.Combine(webRoot, "Medias", name);
		string uploadDir = Path.Combine(webRoot, "Medias");
		if (!Directory.Exists(uploadDir))
			Directory.CreateDirectory(uploadDir);
		try {
			try {
				File.Delete(path);
			}
			catch (Exception ex) {
				throw new ArgumentException("Exception in SaveMedia- Delete! " + ex.Message);
			}

			using FileStream stream = new(path, FileMode.Create);
			image.CopyTo(stream);
		}
		catch (Exception ex) {
			File.Copy(Path.Combine(webRoot, "Medias", "null.png"), path);
			throw new ArgumentException("Exception in SaveMedia- NullPath! " + ex.Message);
		}
	}
}