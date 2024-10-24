namespace Utilities_aspnet.Repositories;

public interface IContentRepository {
	Task<GenericResponse<ContentEntity>> Create(ContentCreateDto dto, CancellationToken ct);
	GenericResponse<IQueryable<ContentEntity>> Read();
	Task<GenericResponse<ContentEntity?>> Update(ContentUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class ContentRepository(DbContext context, IMediaRepository mediaRepository) : IContentRepository {
	public async Task<GenericResponse<ContentEntity>> Create(ContentCreateDto dto, CancellationToken ct) {
		EntityEntry<ContentEntity> e = await context.AddAsync(new ContentEntity {
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
				Email2 = dto.Email2,
				Days = dto.Days,
				Price = dto.Price,
				KeyValues = dto.KeyValues
			}
		}, ct);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<ContentEntity>(e.Entity);
	}

	public GenericResponse<IQueryable<ContentEntity>> Read() => new(context.Set<ContentEntity>().AsNoTracking()
		.Select(x => new ContentEntity {
			Id = x.Id,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Description = x.Description,
			Tags = x.Tags,
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
		ContentEntity e = (await context.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (dto.Title is not null) e.Title = dto.Title;
		if (dto.SubTitle is not null) e.SubTitle = dto.SubTitle;
		if (dto.Description is not null) e.Description = dto.Description;
		if (dto.Tags is not null) e.Tags = dto.Tags;
		if (dto.Address1 is not null) e.JsonDetail.Address1 = dto.Address1;
		if (dto.Address2 is not null) e.JsonDetail.Address2 = dto.Address2;
		if (dto.Address3 is not null) e.JsonDetail.Address3 = dto.Address3;
		if (dto.Telegram is not null) e.JsonDetail.Telegram = dto.Telegram;
		if (dto.Instagram is not null) e.JsonDetail.Instagram = dto.Instagram;
		if (dto.WhatsApp is not null) e.JsonDetail.WhatsApp = dto.WhatsApp;
		if (dto.Dribble is not null) e.JsonDetail.Dribble = dto.Dribble;
		if (dto.Pinterest is not null) e.JsonDetail.Pinterest = dto.Pinterest;
		if (dto.PhoneNumber1 is not null) e.JsonDetail.PhoneNumber1 = dto.PhoneNumber1;
		if (dto.PhoneNumber2 is not null) e.JsonDetail.PhoneNumber2 = dto.PhoneNumber2;
		if (dto.LinkedIn is not null) e.JsonDetail.LinkedIn = dto.LinkedIn;
		if (dto.SoundCloud is not null) e.JsonDetail.SoundCloud = dto.SoundCloud;
		if (dto.Email1 is not null) e.JsonDetail.Email1 = dto.Email1;
		if (dto.Email2 is not null) e.JsonDetail.Email2 = dto.Email2;
		if (dto.Website is not null) e.JsonDetail.Website = dto.Website;
		if (dto.Price is not null) e.JsonDetail.Price = dto.Price;
		if (dto.Days is not null) e.JsonDetail.Days = dto.Days;
		if (dto.KeyValues is not null) e.JsonDetail.KeyValues = dto.KeyValues;

		context.Update(e);
		await context.SaveChangesAsync(ct);
		return new GenericResponse<ContentEntity?>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) {
		ContentEntity e = (await context.Set<ContentEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, ct))!;
		await mediaRepository.DeleteMedia(e.Media);
		context.Set<ContentEntity>().Remove(e);
		await context.SaveChangesAsync(ct);
		return new GenericResponse();
	}
}