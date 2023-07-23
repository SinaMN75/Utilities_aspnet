namespace Utilities_aspnet.Repositories;

public interface IOrderRepository {
	Task<GenericResponse<IQueryable<OrderEntity>>> Filter(OrderFilterDto dto);
	Task<GenericResponse> Vote(OrderVoteDto dto);
	Task<GenericResponse<OrderEntity>> ReadById(Guid id);
	Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto);
	Task<GenericResponse> Delete(Guid id);
	Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto);
	Task<GenericResponse<OrderEntity?>> ApplyDiscountCode(ApplyDiscountCodeOnOrderDto dto);
}

public class OrderRepository : IOrderRepository {
	private readonly DbContext _dbContext;
	private readonly string? _userId;

	public OrderRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor) {
		_dbContext = dbContext;
		_userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
	}

	public async Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto) {
		OrderEntity? oldOrder = await _dbContext.Set<OrderEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);
		if (oldOrder == null) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.NotFound);

		oldOrder.Description = dto.Description ?? oldOrder.Description;
		oldOrder.ReceivedDate = dto.ReceivedDate ?? oldOrder.ReceivedDate;
		oldOrder.Tags = dto.Tags ?? oldOrder.Tags;
		oldOrder.DiscountCode = dto.DiscountCode ?? oldOrder.DiscountCode;
		oldOrder.AddressId = dto.AddressId ?? oldOrder.AddressId;
		oldOrder.UpdatedAt = DateTime.Now;
		await _dbContext.SaveChangesAsync();

		return new GenericResponse<OrderEntity?>(oldOrder);
	}

	public async Task<GenericResponse<IQueryable<OrderEntity>>> Filter(OrderFilterDto dto) {
		IQueryable<OrderEntity> q = _dbContext.Set<OrderEntity>().Include(x => x.Address);

		if (dto.ShowProducts.IsTrue()) {
			q = q.Include(x => x.OrderDetails)!.ThenInclude(x => x.Product).ThenInclude(x => x!.Media);
			q = q.Include(x => x.OrderDetails)!.ThenInclude(x => x.Product).ThenInclude(x => x!.Parent).ThenInclude(x => x!.Media);
			q = q.Include(x => x.User).ThenInclude(x => x!.Media);
			q = q.Include(x => x.ProductOwner).ThenInclude(x => x!.Media);
		}
		if (dto.Id.HasValue) q = q.Where(x => x.Id == dto.Id);
		if (dto.PayNumber.IsNotNullOrEmpty()) q = q.Where(x => (x.PayNumber ?? "").Contains(dto.PayNumber!));

		if (dto.UserId.IsNotNullOrEmpty() && dto.ProductOwnerId.IsNotNullOrEmpty()) {
			q = q.Where(x => x.ProductOwnerId == dto.ProductOwnerId || x.UserId == dto.UserId);
		}
		else {
			if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);
			if (dto.ProductOwnerId.IsNotNullOrEmpty()) q = q.Where(x => x.ProductOwnerId == dto.ProductOwnerId);
		}
		if (dto.StartDate.HasValue) q = q.Where(x => x.CreatedAt >= dto.StartDate);
		if (dto.EndDate.HasValue) q = q.Where(x => x.CreatedAt <= dto.EndDate);

		if (dto.OrderType.HasValue)
			if (dto.OrderType.Value != OrderType.None)
				q = q.Where(w => w.OrderType == dto.OrderType.Value);
		int totalCount = q.Count();

		/*
		var tempQ = q.Where(w => w.PayDateTime == null).Include(x => x.ProductOwner).ToList();
		List<OrderEntity> finalQ = new();
		foreach (var item in tempQ)
		{
			var order = await UpdateOrderPrice(item, item.OrderDetails);
			_dbContext.Update(order);
			await _dbContext.SaveChangesAsync();
			finalQ.Add(order);
		}
		q = null;
		q = finalQ.AsQueryable();
		*/

		//q = q.AsNoTracking().Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<OrderEntity>>(q.AsSingleQuery()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

    private async Task<OrderEntity> UpdateOrderPrice(OrderEntity order, IEnumerable<OrderDetailEntity>? orderDetails)
    {
		if(orderDetails != null)
		{
            foreach (var item in orderDetails.ToList())
			{
				if (item.Product == null) continue;
				if (order.ProductOwner != null) item.FinalPrice = item.Product.Price + order.ProductOwner.JsonDetail.DeliveryPrice1;  
            }
			order.TotalPrice = orderDetails.Sum(s => s.FinalPrice);
        }
		return order;
    }

    public async Task<GenericResponse<OrderEntity>> ReadById(Guid id) {
		OrderEntity? i = await _dbContext.Set<OrderEntity>()
			.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Media)
			.Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p!.Categories)
			.Include(i => i.Address)
			.Include(i => i.User).ThenInclude(i => i!.Media)
			.Include(i => i.ProductOwner).ThenInclude(i => i!.Media)
			.AsNoTracking()
			.FirstOrDefaultAsync(i => i.Id == id);
		/*
        if(i != null)
		{
            i = await UpdateOrderPrice(i, i.OrderDetails);
            _dbContext.Update(i);
			await _dbContext.SaveChangesAsync();
        }
		*/

		return new GenericResponse<OrderEntity>(i!);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await _dbContext.Set<OrderDetailEntity>().Where(i => i.OrderId == id).ExecuteDeleteAsync();
		await _dbContext.Set<OrderEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}
	
		public async Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) {
		ProductEntity p = (await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;
		OrderEntity? o = await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails)
			.FirstOrDefaultAsync(x => x.UserId == _userId && x.Tags.Contains(TagOrder.Pending) && x.OrderDetails!.Any(y => y.ProductId == p.Id));

		if (dto.Count >= p.Stock) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.OutOfStock);

		if (o is null) {
			EntityEntry<OrderEntity> orderEntity = await _dbContext.Set<OrderEntity>().AddAsync(new OrderEntity {
				Tags = new List<TagOrder> { TagOrder.Pending },
				UserId = _userId,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			});

			EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity {
				OrderId = orderEntity.Entity.Id,
				Count = dto.Count,
				ProductId = dto.ProductId,
				UnitPrice = p.Price,
				FinalPrice = dto.Count * p.Price,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			});
			orderEntity.Entity.TotalPrice = orderDetailEntity.Entity.FinalPrice;

			await _dbContext.SaveChangesAsync();
			return new GenericResponse<OrderEntity?>(orderEntity.Entity);
		}
		if (o.OrderDetails != null && o.OrderDetails.Any(x => p.UserId == x.Product?.UserId)) {
			OrderDetailEntity? od = await _dbContext.Set<OrderDetailEntity>().FirstOrDefaultAsync(x => x.ProductId == dto.ProductId && x.OrderId == o.Id);
			if (od is null) {
				if (dto.Count != 0) {
					EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity {
						OrderId = o.Id,
						Count = dto.Count,
						ProductId = dto.ProductId,
						UnitPrice = p.Price,
						FinalPrice = dto.Count * p.Price,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now
					});
					await _dbContext.Set<OrderDetailEntity>().AddAsync(orderDetailEntity.Entity);
				}
			}
			else {
				if (dto.Count == 0) {
					_dbContext.Remove(od);
					await _dbContext.SaveChangesAsync();
				}
				else {
					od.Count = dto.Count;
					od.FinalPrice = dto.Count * p.Price;
					_dbContext.Update(od);
				}
			}
		}
		else {
			EntityEntry<OrderEntity> orderEntity = await _dbContext.Set<OrderEntity>().AddAsync(new OrderEntity {
				Tags = new List<TagOrder> { TagOrder.Pending },
				UserId = _userId,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			});

			EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity {
				OrderId = orderEntity.Entity.Id,
				Count = dto.Count,
				ProductId = dto.ProductId,
				UnitPrice = p.Price,
				FinalPrice = dto.Count * p.Price,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			});
			orderEntity.Entity.TotalPrice = orderDetailEntity.Entity.FinalPrice;

			await _dbContext.SaveChangesAsync();
			return new GenericResponse<OrderEntity?>(orderEntity.Entity);
		}

		o.TotalPrice = 0;
		foreach (OrderDetailEntity orderDetailEntity in o.OrderDetails) o.TotalPrice += orderDetailEntity.FinalPrice;

		_dbContext.Update(o);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<OrderEntity?>(o);
	}

	// public async Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto) {
	// 	ProductEntity p = (await _dbContext.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProductId))!;
	// 	List<OrderEntity> o = await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails)
	// 		.Where(x => x.UserId == _userId && x.Tags.Contains(TagOrder.Pending)).ToListAsync();
	//
	// 	if (dto.Count > p.Stock) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.OutOfStock);
	//
	// 	if (o.IsNullOrEmpty()) {
	// 		EntityEntry<OrderEntity> orderEntity = await _dbContext.Set<OrderEntity>().AddAsync(new OrderEntity {
	// 			Tags = new List<TagOrder> { TagOrder.Pending },
	// 			UserId = _userId,
	// 			CreatedAt = DateTime.Now,
	// 			UpdatedAt = DateTime.Now
	// 		});
	//
	// 		EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity {
	// 			OrderId = orderEntity.Entity.Id,
	// 			Count = dto.Count,
	// 			ProductId = dto.ProductId,
	// 			UnitPrice = p.Price,
	// 			FinalPrice = dto.Count * p.Price,
	// 			CreatedAt = DateTime.Now,
	// 			UpdatedAt = DateTime.Now
	// 		});
	// 		orderEntity.Entity.TotalPrice = orderDetailEntity.Entity.FinalPrice;
	//
	// 		await _dbContext.SaveChangesAsync();
	// 		return new GenericResponse<OrderEntity?>(orderEntity.Entity);
	// 	}
	//
	// 	foreach (OrderEntity orderEntity in o) {
	// 		foreach (OrderDetailEntity orderDetail in orderEntity.OrderDetails ?? new List<OrderDetailEntity>()) {
	// 			if (orderDetail.ProductId == dto.ProductId) {
	// 				if (dto.Count == 0) {
	// 					_dbContext.Remove(orderDetail);
	// 					await _dbContext.SaveChangesAsync();
	// 				}
	// 				else {
	// 					orderDetail.Count = dto.Count;
	// 					orderDetail.FinalPrice = dto.Count * p.Price;
	// 					_dbContext.Update(orderDetail);
	// 				}
	// 				await _dbContext.SaveChangesAsync();
	// 				return new GenericResponse<OrderEntity?>(orderEntity);
	// 			}
	// 		}
	// 		await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity {
	// 			OrderId = orderEntity.Id,
	// 			Count = dto.Count,
	// 			ProductId = dto.ProductId,
	// 			UnitPrice = p.Price,
	// 			FinalPrice = dto.Count * p.Price,
	// 			CreatedAt = DateTime.Now,
	// 			UpdatedAt = DateTime.Now
	// 		});
	// 		orderEntity.TotalPrice = 0;
	// 		foreach (OrderDetailEntity orderEntityOrderDetail in orderEntity.OrderDetails ?? new List<OrderDetailEntity>())
	// 			orderEntity.TotalPrice += orderEntityOrderDetail.FinalPrice;
	//
	// 		_dbContext.Update(o);
	// 		await _dbContext.SaveChangesAsync();
	// 		return new GenericResponse<OrderEntity?>(orderEntity);
	// 	}
	//
	// 	return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.Unhandled);
	// }

	public async Task<GenericResponse<OrderEntity?>> ApplyDiscountCode(ApplyDiscountCodeOnOrderDto dto) {
		OrderEntity o = (await _dbContext.Set<OrderEntity>()
			.Include(x => x.ProductOwner).FirstOrDefaultAsync(x => x.Id == dto.OrderId))!;
		DiscountEntity? d = await _dbContext.Set<DiscountEntity>().FirstOrDefaultAsync(x => x.Code == dto.Code && x.UserId == o.UserId);
		if (d is null) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.InvalidDiscountCode);
		if (d.StartDate >= DateTime.Now || d.EndDate <= DateTime.Now || d.NumberUses <= 0)
			return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.InvalidDiscountCode);

		o.DiscountCode = d.Code;
		o.DiscountPrice = d.DiscountPrice;
		o.TotalPrice -= d.DiscountPrice;
		d.NumberUses -= 1;
		_dbContext.Update(o);
		_dbContext.Update(d);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse<OrderEntity?>(o);
	}

	public async Task<GenericResponse> Vote(OrderVoteDto dto) {
		OrderDetailEntity? orderDetail = await _dbContext.Set<OrderDetailEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id);
		if (orderDetail is null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound);

		OrderEntity? order = await _dbContext.Set<OrderEntity>()
			.FirstOrDefaultAsync(f => f.Id == orderDetail.OrderId && f.Tags.Contains(TagOrder.Complete) && f.UserId == _userId);
		if (order is null)
			return new GenericResponse(UtilitiesStatusCodes.BadRequest);

		orderDetail.Vote = dto.Vote < 1 ? null : dto.Vote;
		_dbContext.Update(orderDetail);
		await _dbContext.SaveChangesAsync();
		return new GenericResponse();
	}
}