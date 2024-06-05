namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity?>> Update(ContentUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository(DbContext dbContext, IMediaRepository mediaRepository) : IContentRepository {
	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateDto dto, CancellationToken ct) {
		EntityEntry<ContentEntity> e = await dbContext.AddAsync(new ContentEntity {
			Description = dto.Description,
			Title = dto.Title,
			SubTitle = dto.SubTitle,
			Tags = dto.Tags ?? [],
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
				Email2 = dto.Email2,
				Days = dto.Days,
				Price = dto.Price,
				KeyValues = dto.KeyValues
			}
		}, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<ContentEntity>(e.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() => new(dbContext.Set<ContentEntity>().AsNoTracking()
		.Select(x => new ContentEntity {
			Id = x.Id,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Description = x.Description,
			Tags = x.Tags,
			DeletedAt = x.DeletedAt,
			JsonDetail = x.JsonDetail,
			Media = x.Media!.Select(y => new MediaEntity {
				Id = y.Id,
				FileName = y.FileName,
				Order = y.Order,
				JsonDetail = y.JsonDetail,
				Tags = y.Tags
			})
		}));

	public async Task<GenericResponse<ContentEntity?>> Update(ContentUpdateDto dto, CancellationToken ct) {
		ContentEntity? e = await dbContext.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
		if (e is null) return new GenericResponse<ContentEntity?>(null, UtilitiesStatusCodes.NotFound);
		e.Title = dto.Title ?? e.Title;
		e.SubTitle = dto.SubTitle ?? e.SubTitle;
		e.Description = dto.Description ?? e.Description;
		e.UpdatedAt = DateTime.UtcNow;
		e.Tags = dto.Tags ?? e.Tags;
		e.DeletedAt = dto.DeletedAt ?? e.DeletedAt;
		e.JsonDetail.Address1 = dto.Address1 ?? e.JsonDetail.Address1;
		e.JsonDetail.Address2 = dto.Address2 ?? e.JsonDetail.Address2;
		e.JsonDetail.Address3 = dto.Address3 ?? e.JsonDetail.Address3;
		e.JsonDetail.Telegram = dto.Telegram ?? e.JsonDetail.Telegram;
		e.JsonDetail.Instagram = dto.Instagram ?? e.JsonDetail.Instagram;
		e.JsonDetail.WhatsApp = dto.WhatsApp ?? e.JsonDetail.WhatsApp;
		e.JsonDetail.Dribble = dto.Dribble ?? e.JsonDetail.Dribble;
		e.JsonDetail.Pinterest = dto.Pinterest ?? e.JsonDetail.Pinterest;
		e.JsonDetail.PhoneNumber1 = dto.PhoneNumber1 ?? e.JsonDetail.PhoneNumber1;
		e.JsonDetail.PhoneNumber2 = dto.PhoneNumber2 ?? e.JsonDetail.PhoneNumber2;
		e.JsonDetail.LinkedIn = dto.LinkedIn ?? e.JsonDetail.LinkedIn;
		e.JsonDetail.SoundCloud = dto.SoundCloud ?? e.JsonDetail.SoundCloud;
		e.JsonDetail.Email1 = dto.Email1 ?? e.JsonDetail.Email1;
		e.JsonDetail.Email2 = dto.Email2 ?? e.JsonDetail.Email2;
		e.JsonDetail.Website = dto.Website ?? e.JsonDetail.Website;
		e.JsonDetail.Price = dto.Price ?? e.JsonDetail.Price;
		e.JsonDetail.Days = dto.Days ?? e.JsonDetail.Days;
		e.JsonDetail.KeyValues = dto.KeyValues ?? e.JsonDetail.KeyValues;

		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<ContentEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ContentEntity e = (await dbContext.Set<ContentEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, ct))!;
		await mediaRepository.DeleteMedia(e.Media);
		dbContext.Set<ContentEntity>().Remove(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse();
	}
}