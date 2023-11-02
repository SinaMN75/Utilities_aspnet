namespace Utilities_aspnet.Repositories;

public interface IContentRepository2 {
	Task<GenericResponse<ContentReadDto>> Create(ContentCreateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentReadDto>> Read();
	Task<GenericResponse<ContentReadDto?>> Update(ContentUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository(DbContext dbContext, IMediaRepository mediaRepository) : IContentRepository2 {
	public async Task<GenericResponse<ContentReadDto>> Create(ContentCreateDto dto, CancellationToken ct) {
		EntityEntry<ContentEntity> response = await dbContext.AddAsync(new ContentEntity {
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
		return new GenericResponse<ContentReadDto>(new ContentReadDto {
			Id = response.Entity.Id,
			Title = response.Entity.Title,
			SubTitle = response.Entity.SubTitle,
			Description = response.Entity.Description,
			Tags = response.Entity.Tags,
			JsonDetail = response.Entity.JsonDetail
		});
	}

	public GenericResponse<IQueryable<ContentReadDto>> Read() {
		IQueryable<ContentReadDto> q = dbContext.Set<ContentEntity>().AsNoTracking().Select(x => new ContentReadDto {
			Id = x.Id,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Description = x.Description,
			Tags = x.Tags,
			JsonDetail = x.JsonDetail,
			Media = x.Media!.Select(y => new MediaReadDto {
				Id = y.Id,
				FileName = y.FileName,
				Order = y.Order,
				JsonDetail = y.JsonDetail,
				Tags = y.Tags,
			})
		});
		return new GenericResponse<IQueryable<ContentReadDto>>(q);
	}

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
		return new GenericResponse<ContentReadDto?>(new ContentReadDto {
			Id = e.Id,
			Title = e.Title,
			SubTitle = e.SubTitle,
			Description = e.Description,
			Tags = e.Tags,
			JsonDetail = e.JsonDetail,
		});
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ContentEntity e = (await dbContext.Set<ContentEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, ct))!;
		await mediaRepository.DeleteMedia(e.Media);
		dbContext.Set<ContentEntity>().Remove(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}
}