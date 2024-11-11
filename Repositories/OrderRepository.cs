namespace Utilities_aspnet.Repositories;

public interface IOrderRepository {
	Task<GenericResponse<IEnumerable<OrderEntity>>> Filter(OrderFilterDto dto);
	Task<GenericResponse> Vote(OrderVoteDto dto);
	Task<GenericResponse<OrderEntity>> ReadById(Guid id);
	Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
	Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto);
	Task<GenericResponse<OrderEntity?>> CreateReservationOrder(ReserveCreateUpdateDto dto);
	Task<GenericResponse<OrderEntity?>> CreateChairReservationOrder(ReserveChairCreateUpdateDto dto);
}

public class OrderRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) : IOrderRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto) {
		OrderEntity e = (await dbContext.Set<OrderEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		e.Description = dto.Description ?? e.Description;
		e.ReceivedDate = dto.ReceivedDate ?? e.ReceivedDate;
		e.Tags = dto.Tags ?? e.Tags;
		e.DiscountCode = dto.DiscountCode ?? e.DiscountCode;
		e.AddressId = dto.AddressId ?? e.AddressId;
		e.JsonDetail.RefCode = dto.RefCode ?? e.JsonDetail.RefCode;
		e.UpdatedAt = DateTime.UtcNow;
		
		await dbContext.SaveChangesAsync();

		return new GenericResponse<OrderEntity?>(e);
	}

	public async Task<GenericResponse<IEnumerable<OrderEntity>>> Filter(OrderFilterDto dto) {
		await UpdateCartPrices();
		IQueryable<OrderEntity> q = dbContext.Set<OrderEntity>()
			.Include(x => x.Address)
			.Include(x => x.OrderDetails)!.ThenInclude(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.OrderDetails)!.ThenInclude(x => x.Product).ThenInclude(x => x!.Parent)
			.ThenInclude(x => x!.Media)
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.ProductOwner).ThenInclude(x => x!.Media)
			.OrderByDescending(x => x.CreatedAt);

		if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => dto.Tags!.Any(y => x.Tags.Contains(y)));

		if (dto.Id.HasValue) q = q.Where(x => x.Id == dto.Id);
		if (dto.PayNumber.IsNotNullOrEmpty()) q = q.Where(x => (x.PayNumber ?? "").Contains(dto.PayNumber!));
		if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);
		if (dto.ProductOwnerId.IsNotNullOrEmpty()) q = q.Where(x => x.ProductOwnerId == dto.ProductOwnerId);
		if (dto.StartDate.HasValue) q = q.Where(x => x.CreatedAt >= dto.StartDate);
		if (dto.EndDate.HasValue) q = q.Where(x => x.CreatedAt <= dto.EndDate);

		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IEnumerable<OrderEntity>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<OrderEntity>> ReadById(Guid id) {
		OrderEntity? i = await dbContext.Set<OrderEntity>()
			.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Media)
			.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Categories)
			.Include(i => i.Address)
			.Include(i => i.User).ThenInclude(i => i!.Media)
			.Include(i => i.ProductOwner).ThenInclude(i => i!.Media)
			.AsNoTracking()
			.FirstOrDefaultAsync(i => i.Id == id);

		return new GenericResponse<OrderEntity>(i!);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await dbContext.Set<TransactionEntity>().Where(i => i.OrderId == id).ExecuteDeleteAsync();
		await dbContext.Set<OrderDetailEntity>().Where(i => i.OrderId == id).ExecuteDeleteAsync();
		await dbContext.Set<OrderEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) {
		ProductEntity p = (await dbContext.Set<ProductEntity>().Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;
		OrderEntity? o = await dbContext.Set<OrderEntity>().Include(x => x.OrderDetails)!.ThenInclude(x => x.Product)
			.FirstOrDefaultAsync(x =>
				x.UserId == _userId && x.Tags.Contains(TagOrder.Pending) &&
				x.OrderDetails!.Any(y => y.Product!.UserId == p.UserId));

		if (dto.Count > (p.Stock ?? 0)) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.OutOfStock);

		if (o is null) {
			EntityEntry<OrderEntity> orderEntity = await dbContext.Set<OrderEntity>().AddAsync(new OrderEntity {
				OrderNumber = new Random().Next(10000, 99999),
				Tags = p.Tags.Contains([TagProduct.Premium1Month, TagProduct.Premium3Month, TagProduct.Premium6Month, TagProduct.Premium12Month])
					? [TagOrder.Pending, TagOrder.Premium]
					: [TagOrder.Pending],
				UserId = _userId,
				CreatedAt = DateTime.UtcNow,
				ProductOwnerId = p.UserId,
				UpdatedAt = DateTime.UtcNow
			});

			EntityEntry<OrderDetailEntity> orderDetailEntity = await dbContext.Set<OrderDetailEntity>().AddAsync(
				new OrderDetailEntity {
					OrderId = orderEntity.Entity.Id,
					Count = dto.Count,
					ProductId = dto.ProductId,
					UnitPrice = p.Price,
					FinalPrice = dto.Count * p.Price,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				});
			orderEntity.Entity.TotalPrice = orderDetailEntity.Entity.FinalPrice;

			await dbContext.SaveChangesAsync();
			return new GenericResponse<OrderEntity?>(orderEntity.Entity);
		}

		if (o.OrderDetails != null && o.OrderDetails.Any(x => p.UserId != x.Product?.UserId)) {
			EntityEntry<OrderEntity> orderEntity = await dbContext.Set<OrderEntity>().AddAsync(new OrderEntity {
				OrderNumber = new Random().Next(10000, 99999),
				Tags = [TagOrder.Pending],
				UserId = _userId,
				CreatedAt = DateTime.UtcNow,
				ProductOwnerId = p.UserId,
				UpdatedAt = DateTime.UtcNow
			});

			EntityEntry<OrderDetailEntity> orderDetailEntity = await dbContext.Set<OrderDetailEntity>().AddAsync(
				new OrderDetailEntity {
					OrderId = orderEntity.Entity.Id,
					Count = dto.Count,
					ProductId = dto.ProductId,
					UnitPrice = p.Price,
					FinalPrice = dto.Count * p.Price,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				});
			orderEntity.Entity.TotalPrice = orderDetailEntity.Entity.FinalPrice;

			await dbContext.SaveChangesAsync();
			return new GenericResponse<OrderEntity?>(orderEntity.Entity);
		}

		OrderDetailEntity? od = await dbContext.Set<OrderDetailEntity>()
			.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId && x.OrderId == o.Id);
		if (od is null) {
			if (dto.Count != 0) {
				EntityEntry<OrderDetailEntity> orderDetailEntity = await dbContext.Set<OrderDetailEntity>().AddAsync(
					new OrderDetailEntity {
						OrderId = o.Id,
						Count = dto.Count,
						ProductId = dto.ProductId,
						UnitPrice = p.Price,
						FinalPrice = dto.Count * p.Price,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					});
				await dbContext.Set<OrderDetailEntity>().AddAsync(orderDetailEntity.Entity);
			}
		}
		else {
			if (dto.Count == 0) {
				dbContext.Remove(od);
				await dbContext.SaveChangesAsync();
			}
			else {
				od.Count = dto.Count;
				od.FinalPrice = dto.Count * p.Price;
				dbContext.Update(od);
			}
		}

		if (o.OrderDetails.IsNullOrEmpty()) {
			dbContext.Remove(o);
		}
		else {
			o.TotalPrice = 0;
			foreach (OrderDetailEntity orderDetailEntity in o.OrderDetails!)
				o.TotalPrice += orderDetailEntity.FinalPrice;
			dbContext.Update(o);
		}

		await dbContext.SaveChangesAsync();
		return new GenericResponse<OrderEntity?>(o);
	}

	public async Task<GenericResponse<OrderEntity?>> CreateReservationOrder(ReserveCreateUpdateDto dto) {
		ProductEntity p = (await dbContext.Set<ProductEntity>().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;
		long totalPrice = 0;
		foreach (ReserveDto reserveDto in dto.ReserveDto) {
			totalPrice += reserveDto.Price + reserveDto.PriceForAnyExtra * reserveDto.ExtraMemberCount;
		}

		OrderEntity e = new() {
			OrderNumber = new Random().Next(10000, 99999),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			ProductOwnerId = p.UserId,
			JsonDetail = new OrderJsonDetail { ReservationTimes = dto.ReserveDto },
			Tags = [TagOrder.Pending, TagOrder.Reserve],
			UserId = _userId,
			TotalPrice = totalPrice
		};
		EntityEntry<OrderEntity> orderEntity = await dbContext.AddAsync(e);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<OrderEntity?>(orderEntity.Entity);
	}

	public async Task<GenericResponse<OrderEntity?>> CreateChairReservationOrder(ReserveChairCreateUpdateDto dto) {
		ProductEntity p = (await dbContext.Set<ProductEntity>().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;

		List<Seat> seats = [];

		foreach (string s in dto.SeatsId) {
			seats.Add(p.JsonDetail.Seats!.FirstOrDefault(x => x.ChairId == s)!);
		}

		long totalPrice = 0;
		foreach (Seat seat in seats) {
			totalPrice += seat.Price ?? 0;
		}

		OrderEntity e = new() {
			OrderNumber = new Random().Next(10000, 99999),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			ProductOwnerId = p.UserId,
			JsonDetail = new OrderJsonDetail { Seats = seats, ProductId = dto.ProductId.ToString() },
			Tags = [TagOrder.Pending, TagOrder.Reserve],
			UserId = _userId,
			TotalPrice = totalPrice
		};
		EntityEntry<OrderEntity> orderEntity = await dbContext.AddAsync(e);
		await dbContext.SaveChangesAsync();

		return new GenericResponse<OrderEntity?>(orderEntity.Entity);
	}

	public async Task<GenericResponse> Vote(OrderVoteDto dto) {
		OrderDetailEntity? orderDetail =
			await dbContext.Set<OrderDetailEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id);
		if (orderDetail is null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound);

		OrderEntity? order = await dbContext.Set<OrderEntity>()
			.FirstOrDefaultAsync(f =>
				f.Id == orderDetail.OrderId && f.Tags.Contains(TagOrder.Complete) && f.UserId == _userId);
		if (order is null)
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		orderDetail.Vote = dto.Vote < 1 ? null : dto.Vote;
		dbContext.Update(orderDetail);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	private async Task UpdateCartPrices() {
		List<OrderEntity> q = await dbContext.Set<OrderEntity>()
			.Where(x => x.Tags.Contains(TagOrder.Pending))
			.Where(x => !x.Tags.Contains(TagOrder.Reserve))
			.Include(x => x.OrderDetails)!.ThenInclude(x => x.Product)
			.Include(x => x.ProductOwner)
			.ToListAsync();

		foreach (OrderEntity orderEntity in q) {
			if (!orderEntity.JsonDetail.ReservationTimes.IsNotNullOrEmpty()) continue;
			if (orderEntity.OrderDetails.IsNullOrEmpty())
				dbContext.Remove(orderEntity);
			await dbContext.SaveChangesAsync();

			foreach (OrderDetailEntity orderEntityOrderDetail in orderEntity.OrderDetails!) {
				orderEntityOrderDetail.FinalPrice = orderEntityOrderDetail.Product!.Price * orderEntityOrderDetail.Count;
				dbContext.Set<OrderDetailEntity>().Update(orderEntityOrderDetail);
				await dbContext.SaveChangesAsync();
			}

			orderEntity.TotalPrice = orderEntity.OrderDetails.Select(x => x.FinalPrice!).Sum();
			dbContext.Set<OrderEntity>().Update(orderEntity);
			await dbContext.SaveChangesAsync();
		}
	}
}