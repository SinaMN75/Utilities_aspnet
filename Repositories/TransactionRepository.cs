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
			Tags = dto.Tags ?? [],
			BuyerId = dto.BuyerId,
			SellerId = dto.SellerId,
			OrderId = dto.OrderId
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
		IQueryable<TransactionEntity> q = dbContext.Set<TransactionEntity>().AsNoTracking().OrderByDescending(x => x.CreatedAt)
			.Select(x => new TransactionEntity {
				Id = x.Id,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt,
				Amount = x.Amount,
				Descriptions = x.Descriptions,
				RefId = x.RefId,
				CardNumber = x.CardNumber,
				Tags = x.Tags,
				BuyerId = x.BuyerId,
				SellerId = x.SellerId,
				OrderId = x.OrderId,
				Code = x.Code,
				Buyer = new UserEntity {
					Id = x.Buyer!.Id,
					FirstName = x.Buyer.FirstName,
					LastName = x.Buyer.LastName,
					FullName = x.Buyer.FullName,
					Tags = x.Buyer.Tags,
					PhoneNumber = x.Buyer.PhoneNumber,
					JsonDetail = x.Buyer.JsonDetail,
					Email = x.Buyer.Email
				},
				Seller = new UserEntity {
					Id = x.Buyer!.Id,
					FirstName = x.Buyer.FirstName,
					LastName = x.Buyer.LastName,
					FullName = x.Buyer.FullName,
					Tags = x.Buyer.Tags,
					PhoneNumber = x.Buyer.PhoneNumber,
					JsonDetail = x.Buyer.JsonDetail,
					Email = x.Buyer.Email
				},
				Order = x.Order != null
					? new OrderEntity {
						Id = x.Order.Id,
						CreatedAt = x.Order.CreatedAt,
						UpdatedAt = x.Order.UpdatedAt,
						JsonDetail = x.Order.JsonDetail,
						UserId = x.Order.UserId,
						TotalPrice = x.Order.TotalPrice,
						ProductOwnerId = x.Order.ProductOwnerId,
						OrderNumber = x.Order.OrderNumber
					}
					: x.Order
			});
		if (dto.RefId.IsNotNullOrEmpty()) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.DateTimeStart.HasValue) q = q.Where(x => x.CreatedAt > dto.DateTimeStart);
		if (dto.DateTimeEnd.HasValue) q = q.Where(x => x.CreatedAt < dto.DateTimeEnd);
		if (dto.RefId.IsNotNullOrEmpty()) q = q.Where(x => x.RefId == dto.RefId);
		if (dto.BuyerId.IsNotNullOrEmpty()) q = q.Where(x => x.BuyerId == dto.BuyerId);
		if (dto.SellerId.IsNotNullOrEmpty()) q = q.Where(x => x.BuyerId == dto.SellerId);
		if (dto.Amount is not null) q = q.Where(x => x.Amount == dto.Amount);
		if (dto.OrderId is not null) q = q.Where(x => x.OrderId == dto.OrderId);
		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.All(y => x.Tags.Contains(y)));

		return new GenericResponse<IQueryable<TransactionEntity>>(q.AsSingleQuery());
	}
}