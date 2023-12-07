namespace Utilities_aspnet.Repositories;

public interface ITransactionRepository {
	GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto);
	Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct);
	Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct);
	Task<GenericResponse> Delete(Guid id, CancellationToken ct);
}

public class TransactionRepository(DbContext dbContext) : ITransactionRepository {

	public async Task<GenericResponse<TransactionEntity>> Create(TransactionCreateDto dto, CancellationToken ct) {
		TransactionEntity e = new() {
			Amount = dto.Amount,
			Descriptions = dto.Descriptions,
			RefId = dto.RefId,
			CardNumber = dto.CardNumber,
			Tags = dto.Tags,
			UserId = dto.UserId,
			OrderId = dto.OrderId,
			SubscriptionId = dto.SubscriptionId
		};
		await dbContext.Set<TransactionEntity>().AddAsync(e, ct);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse<TransactionEntity>> Update(TransactionUpdateDto dto, CancellationToken ct) {
		TransactionEntity e = (await dbContext.Set<TransactionEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id, ct))!;
		if (dto.Amount.HasValue) e.Amount = dto.Amount;
		if (dto.RefId.IsNotNullOrEmpty()) e.RefId = dto.RefId;
		if (dto.Tags.IsNotNullOrEmpty()) e.Tags = dto.Tags!;
		if (dto.CardNumber.IsNotNullOrEmpty()) e.CardNumber = dto.CardNumber;
		if (dto.Descriptions.IsNotNullOrEmpty()) e.Descriptions = dto.Descriptions;
		dbContext.Update(e);
		await dbContext.SaveChangesAsync(ct);
		return new GenericResponse<TransactionEntity>(e);
	}

	public async Task<GenericResponse> Delete(Guid id, CancellationToken ct) { 
		await dbContext.Set<TransactionEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct);
		return new GenericResponse();
	}


	public GenericResponse<IQueryable<TransactionEntity>> Filter(TransactionFilterDto dto) {
		IQueryable<TransactionEntity> q = dbContext.Set<TransactionEntity>().AsNoTracking().OrderBy(x => x.CreatedAt)
			.Select(x => new TransactionEntity {
				Id = x.Id,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt,
				Amount = x.Amount,
				Descriptions = x.Descriptions,
				RefId = x.RefId,
				CardNumber = x.CardNumber,
				Tags = x.Tags,
				UserId = x.UserId,
				OrderId = x.OrderId,
				SubscriptionId = x.SubscriptionId,
				User = new UserEntity {
					Id = x.User!.Id,
					FirstName = x.User.FirstName,
					LastName = x.User.LastName,
					FullName = x.User.FullName,
					Tags = x.User.Tags,
					PhoneNumber = x.User.PhoneNumber,
					JsonDetail = x.User.JsonDetail,
					Email = x.User.Email,
					AppEmail = x.User.AppEmail,
					AppPhoneNumber = x.User.AppPhoneNumber,
					AppUserName = x.User.AppUserName
				},
				Order = x.Order != null ? new OrderEntity {
					Id = x.Order.Id,
					CreatedAt = x.Order.CreatedAt,
					UpdatedAt = x.Order.UpdatedAt,
					JsonDetail = x.Order.JsonDetail,
					UserId = x.Order.UserId,
					TotalPrice = x.Order.TotalPrice,
					ProductOwnerId = x.Order.ProductOwnerId,
					OrderNumber = x.Order.OrderNumber
				} : x.Order
			});
		if (dto.RefId.IsNotNullOrEmpty()) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsSingleQuery());
	}
}