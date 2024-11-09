namespace Utilities_aspnet.EndPoints;

public static class ContentEndPoints {
	public static void UseContentEndPoints(this WebApplication app) {
		app.MapPost("api/v2/filterContents", (IContentRepository repository) => { return repository.Read(); });

		app.MapPost("api/v2/createContents", (IContentRepository repository, ContentCreateDto dto, CancellationToken ct) => {
			return repository.Create(dto, ct);
		}).CacheOutput();
	}
}