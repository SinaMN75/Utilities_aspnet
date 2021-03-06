namespace Utilities_aspnet.Repositories;

public interface IMediaRepository {
	bool SaveMedia(IFormFile image, string name);
	bool SaveMedia(IFormFile image, string name, string folder);
	string GetFileName(Guid guid, string ext = ".png");
	string GetFileName(Guid guid, string folder, string ext);
	string GetFileUrl(string name, string folder);
}

public class MediaRepository : IMediaRepository {
	private readonly IWebHostEnvironment _env;

	public MediaRepository(IWebHostEnvironment env) => _env = env;

	public string GetFileName(Guid guid, string folder, string ext) => folder + guid.ToString("N") + ext;

	public string GetFileName(Guid guid, string ext = ".png") => guid.ToString("N") + ext;

	public string GetFileUrl(string name, string folder) => Path.Combine(folder, name);

	public bool SaveMedia(IFormFile image, string name, string folder) {
		try {
			string webRoot = _env.WebRootPath;
			string nullPath = Path.Combine(webRoot, "Medias", "null.png");
			string path = Path.Combine(webRoot, "Medias", folder, name);
			string uploadDir = Path.Combine(webRoot, "Medias", folder);
			if (!Directory.Exists(uploadDir))
				Directory.CreateDirectory(uploadDir);
			try {
				try {
					File.Delete(path);
				}
				catch (Exception) {
					// ignored
				}

				using FileStream stream = new(path, FileMode.Create);
				image.CopyTo(stream);
				return true;
			}
			catch {
				File.Copy(nullPath, path);
				return false;
			}
		}
		catch {
			return false;
		}
	}

	public bool SaveMedia(IFormFile image, string name) => SaveMedia(image, name, "");
}