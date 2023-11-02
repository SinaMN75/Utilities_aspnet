namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentReadDto>> Create(ContentCreateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentReadDto>> Read();
	Task<GenericResponse<ContentReadDto?>> Update(ContentUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository(DbContext dbContext, IMediaRepository mediaRepository) : IContentRepository {
	public async Task<GenericResponse<ContentReadDto>> Create(ContentCreateDto dto, CancellationToken ct) {
		EntityEntry<ContentEntity> e = await dbContext.AddAsync(new ContentEntity {
			Description = dto.Description,
			Title = dto.Title,
			SubTitle = dto.SubTitle,
			Tags = dto.Tags,
			JsonDetail = new ContentJsonDetail {
				Instagram = dto.Instagram,
				Telegram = dto.Telegram,
				WhatsApp = dto.WhatsApp,
				LinkedIn = dto.LinkedIn,
				Dribble = dto.Dribble,
				SoundCloud = dto.SoundCloud,
				Pinterest = dto.Pinterest,
				Website = dto.Website,
				PhoneNumber1 = dto.PhoneNumber1,
				PhoneNumber2 = dto.PhoneNumber2,
				Address1 = dto.Address1,
				Address2 = dto.Address2,
				Address3 = dto.Address3,
				Email1 = dto.Email1,
				Email2 = dto.Email2
			}
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<ContentReadDto>(e.Entity.MapReadDto());
	}

	public GenericResponse<IQueryable<ContentReadDto>> Read() => new(dbContext.Set<ContentEntity>().AsNoTracking().MapReadDto());

	public async Task<GenericResponse<ContentReadDto?>> Update(ContentUpdateDto dto, CancellationToken ct) {
		ContentEntity? e = await dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
		if (e is null) return new GenericResponse<ContentReadDto?>(null, UtilitiesStatusCodes.NotFound);
		e.Title = dto.Title ?? e.Title;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		e.UpdatedAt = DateTime.Now;
		e.Tags = dto.Tags ?? e.Tags;

		if (dto.RemoveTags.IsNotNullOrEmpty()) dto.RemoveTags!.ForEach(item => e.Tags?.Remove(item));
		if (dto.AddTags.IsNotNullOrEmpty()) e.Tags!.AddRange(dto.AddTags!);

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<ContentReadDto?>(e.MapReadDto());
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ContentEntity e = (await dbContext.Set<ContentEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, ct))!;
		await mediaRepository.DeleteMedia(e.Media);
		dbContext.Set<ContentEntity>().Remove(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}
}