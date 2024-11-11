using Microsoft.AspNetCore.Routing;

namespace Utilities_aspnet.Apis;

public static class ContentEndpoints {
	public static void MapContentApi(this IEndpointRouteBuilder app) {
		RouteGroupBuilder group = app.MapGroup("api/v2/contents").WithTags("v2/Content");

		group.MapPost("", [ApiKey] async (IContentRepository repository, ContentCreateDto dto, CancellationToken ct) => {
			GenericResponse<ContentEntity> i = await repository.Create(dto, ct);
			return Results.Ok(i);
		}).WithName("Create");

		group.MapGet("", (IContentRepository repository) => {
			GenericResponse<IQueryable<ContentEntity>> i = repository.Read();
			return Results.Ok(i);
		}).WithName("Read");
		;

		group.MapPut("", async (IContentRepository repository, ContentUpdateDto dto, CancellationToken ct) => {
			GenericResponse<ContentEntity> i = await repository.Update(dto, ct);
			return Results.Ok(i);
		}).WithName("Update");
		;

		group.MapDelete("", async (IContentRepository repository, Guid id, CancellationToken ct) => {
			GenericResponse i = await repository.Delete(id, ct);
			return Results.Ok(i);
		}).WithName("Delete");
		;
	}
}