namespace Utilities_aspnet.Repositories;

public interface IMediaRepository {
	Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model);
	Task<GenericResponse<MediaEntity?>> UpdateMedia(Guid id, UpdateMediaDto model);
	Task<GenericResponse> Delete(Guid id);
}

public class MediaRepository : IMediaRepository {
	private readonly DbContext _dbContext;
	private readonly IWebHostEnvironment _env;

	public MediaRepository(IWebHostEnvironment env, DbContext dbContext) {
		_env = env;
		_dbContext = dbContext;
	}

	public async Task<GenericResponse<IEnumerable<MediaEntity>?>> Upload(UploadDto model) {
		List<MediaEntity> medias = new();

		if (model.Files != null)
			foreach (IFormFile file in model.Files) {
				string name = Guid.NewGuid() + Path.GetExtension(file.FileName);

				List<string> allowedExtensions = new() { ".png", ".gif", ".jpg", ".jpeg", ".mp4", ".mp3", ".pdf", ".aac", ".apk", ".zip" };
				if (!allowedExtensions.Contains(Path.GetExtension(file.FileName.ToLower())))
					return new GenericResponse<IEnumerable<MediaEntity>?>(null, UtilitiesStatusCodes.BadRequest);

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
					UseCase = model.UseCase,
					NotificationId = model.NotificationId,
					GroupChatId = model.GroupChatId,
					GroupChatMessageId = model.GroupChatMessageId,
					JsonDetail = {
						Title = model.Title,
						IsPrivate = model.PrivacyType,
						Size = model.Size,
						Time = model.Time,
						Artist = model.Artist,
						Album = model.Album
					}
				};
				await _dbContext.Set<MediaEntity>().AddAsync(media);
				await _dbContext.SaveChangesAsync();
				medias.Add(media);
				SaveMedia(file, name);
			}
		if (model.Links != null)
			foreach (MediaEntity media in model.Links.Select(link => new MediaEntity {
				         UserId = model.UserId,
				         ProductId = model.ProductId,
				         ContentId = model.ContentId,
				         CategoryId = model.CategoryId,
				         ChatId = model.ChatId,
				         CommentId = model.CommentId,
				         CreatedAt = DateTime.Now,
				         UseCase = model.UseCase,
				         NotificationId = model.NotificationId,
				         Order = model.Order,
				         JsonDetail = {
					         Title = model.Title,
					         IsPrivate = model.PrivacyType,
					         Size = model.Size,
					         Time = model.Time,
					         Artist = model.Artist,
					         Album = model.Album
				         }
			         })) {
				await _dbContext.Set<MediaEntity>().AddAsync(media);
				await _dbContext.SaveChangesAsync();
				medias.Add(media);
			}

		return new GenericResponse<IEnumerable<MediaEntity>?>(medias);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		MediaEntity? media = await _dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
		try { File.Delete(Path.Combine(_env.WebRootPath, "Medias", media.FileName!)); }
		catch (Exception) { }
		_dbContext.Set<MediaEntity>().Remove(media);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<MediaEntity?>> UpdateMedia(Guid id, UpdateMediaDto model) {
		MediaEntity? media = await _dbContext.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == id);
		if (media is null)
			throw new Exception("media is not found");

		media.JsonDetail.Title = model.Title ?? media.JsonDetail.Title;
		media.JsonDetail.Size = model.Size ?? media.JsonDetail.Size;
		media.JsonDetail.Time = model.Time ?? media.JsonDetail.Time;
		media.JsonDetail.Artist = model.Artist ?? media.JsonDetail.Artist;
		media.JsonDetail.Album = model.Album ?? media.JsonDetail.Album;
		media.UpdatedAt = DateTime.Now;
		media.UseCase = model.UseCase ?? media.UseCase;
		media.Order = model.Order ?? media.Order;

		_dbContext.Set<MediaEntity>().Update(media);
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<MediaEntity?>(media);
	}

	public void SaveMedia(IFormFile image, string name) {
		string webRoot = _env.WebRootPath;
		string nullPath = Path.Combine(webRoot, "Medias", "null.png");
		string path = Path.Combine(webRoot, "Medias", name);
		string uploadDir = Path.Combine(webRoot, "Medias");
		if (!Directory.Exists(uploadDir))
			Directory.CreateDirectory(uploadDir);
		try {
			try { File.Delete(path); }
			catch (Exception ex) { throw new ArgumentException("Exception in SaveMedia- Delete! " + ex.Message); }

			using FileStream stream = new(path, FileMode.Create);
			image.CopyTo(stream);
		}
		catch (Exception ex) {
			File.Copy(nullPath, path);
			throw new ArgumentException("Exception in SaveMedia- NullPath! " + ex.Message);
		}
	}
}