namespace Utilities_aspnet.Repositories;

public interface IOrderRepository
{
    Task<GenericResponse<OrderEntity?>> Create(OrderCreateUpdateDto dto);
    GenericResponse<IQueryable<OrderEntity>> Filter(OrderFilterDto dto);
    Task<GenericResponse> Vote(OrderVoteDto dto);
    Task<GenericResponse<OrderEntity>> ReadById(Guid id);
    Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto);
    Task<GenericResponse> Delete(Guid id);
    Task<GenericResponse<OrderDetailEntity?>> CreateOrderDetail(OrderDetailCreateUpdateDto dto);
    Task<GenericResponse<OrderDetailEntity?>> UpdateOrderDetail(OrderDetailCreateUpdateDto dto);
    Task<GenericResponse> DeleteOrderDetail(Guid id);
    Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto);
}

public class OrderRepository : IOrderRepository
{
    private readonly DbContext _dbContext;
    private readonly string? _userId;

    public OrderRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
    }

    public async Task<GenericResponse<OrderEntity?>> Create(OrderCreateUpdateDto dto)
    {
        double totalPrice = 0;

        List<ProductEntity> products = new();
        foreach (OrderDetailCreateUpdateDto item in dto.OrderDetails!)
            products.Add((await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == item.ProductId))!);

        IEnumerable<string?> q = products.GroupBy(x => x.UserId).Select(z => z.Key);
        if (q.Count() > 1) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.MultipleSeller);

        OrderEntity entityOrder = new()
        {
            Description = dto.Description,
            ReceivedDate = dto.ReceivedDate,
            UserId = _userId,
            DiscountCode = dto.DiscountCode,
            PayType = dto.PayType,
            SendType = dto.SendType,
            AddressId = dto.AddressId,
            Status = dto.Status,
            OrderType = dto.OrderType,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ProductOwnerId = products.First().UserId
        };

        await _dbContext.Set<OrderEntity>().AddAsync(entityOrder);
        await _dbContext.SaveChangesAsync();

        foreach (OrderDetailCreateUpdateDto item in dto.OrderDetails)
        {
            CategoryEntity? categoryEntity = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item.CategoryId);
            if (categoryEntity != null && categoryEntity.Stock < item.Count) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.OutOfStock);

            OrderDetailEntity orderDetailEntity = new()
            {
                OrderId = entityOrder.Id,
                ProductId = item.ProductId,
                Price = categoryEntity?.Price * item.Count,
                Count = item.Count,
                CategoryId = item.CategoryId,
                UnitPrice = categoryEntity?.Price
            };

            await _dbContext.Set<OrderDetailEntity>().AddAsync(orderDetailEntity);
            await _dbContext.SaveChangesAsync();

            totalPrice += Convert.ToDouble((categoryEntity?.Price ?? 0) * item.Count);
        }
        entityOrder.TotalPrice = totalPrice;
        _dbContext.Set<OrderEntity>().Update(entityOrder);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<OrderEntity?>(entityOrder);
    }

    public async Task<GenericResponse<OrderEntity?>> Update(OrderCreateUpdateDto dto)
    {
        OrderEntity? oldOrder = await _dbContext.Set<OrderEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);
        if (oldOrder == null) return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.NotFound);

        oldOrder.Description = dto.Description ?? oldOrder.Description;
        oldOrder.ReceivedDate = dto.ReceivedDate ?? oldOrder.ReceivedDate;
        oldOrder.Status = dto.Status ?? oldOrder.Status;
        oldOrder.PayType = dto.PayType ?? oldOrder.PayType;
        oldOrder.SendType = dto.SendType ?? oldOrder.SendType;
        oldOrder.DiscountCode = dto.DiscountCode ?? oldOrder.DiscountCode;
        oldOrder.AddressId = dto.AddressId ?? oldOrder.AddressId;
        oldOrder.UpdatedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();

        return new GenericResponse<OrderEntity?>(oldOrder);
    }

    public GenericResponse<IQueryable<OrderEntity>> Filter(OrderFilterDto dto)
    {
        IQueryable<OrderEntity> q = _dbContext.Set<OrderEntity>()
            .Include(x => x.Address)
            .Include(x => x.OrderDetails.Where(y => y.DeletedAt == null)).ThenInclude(x => x.Category)
            .Include(x => x.Address);

        if (dto.ShowProducts.IsTrue())
        {
            q = q.Include(x => x.OrderDetails).ThenInclude(x => x.Product).ThenInclude(x => x.Media);
            q = q.Include(x => x.OrderDetails).ThenInclude(x => x.Product).ThenInclude(i => i.Categories);
            q = q.Include(x => x.OrderDetails).ThenInclude(x => x.Product).ThenInclude(i => i.User);
            q = q.Include(x => x.User).ThenInclude(x => x.Media);
        }
        if (dto.Id.HasValue) q = q.Where(x => x.Id == dto.Id);
        if (dto.Status.HasValue) q = q.Where(x => x.Status == dto.Status);
        if (dto.SendType.HasValue) q = q.Where(x => x.SendType == dto.SendType);
        if (dto.PayType.HasValue) q = q.Where(x => x.PayType == dto.PayType);
        if (dto.PayNumber.IsNotNullOrEmpty()) q = q.Where(x => (x.PayNumber ?? "").Contains(dto.PayNumber!));

        if (dto.UserId.IsNotNullOrEmpty() && dto.ProductOwnerId.IsNotNullOrEmpty())
        {
            q = q.Where(x => x.ProductOwnerId == dto.ProductOwnerId || x.UserId == dto.UserId);
        }
        else
        {
            if (dto.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == dto.UserId);
            if (dto.ProductOwnerId.IsNotNullOrEmpty()) q = q.Where(x => x.ProductOwnerId == dto.ProductOwnerId);
        }
        if (dto.StartDate.HasValue) q = q.Where(x => x.CreatedAt >= dto.StartDate);
        if (dto.EndDate.HasValue) q = q.Where(x => x.CreatedAt <= dto.EndDate);

        if (dto.OrderType.HasValue)
        {
            if (dto.OrderType.Value != OrderType.None)
            {
                q = q.Where(w => w.OrderType == dto.OrderType.Value);
            }
        }
        int totalCount = q.Count();

        q = q.AsNoTracking().Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

        return new GenericResponse<IQueryable<OrderEntity>>(q.AsSingleQuery())
        {
            TotalCount = totalCount,
            PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
            PageSize = dto.PageSize
        };
    }

    public async Task<GenericResponse<OrderEntity>> ReadById(Guid id)
    {
        OrderEntity? i = await _dbContext.Set<OrderEntity>()
            .Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p.Media)
            .Include(i => i.OrderDetails)!.ThenInclude(p => p.Product).ThenInclude(p => p.Categories)
            .Include(i => i.OrderDetails)!.ThenInclude(p => p.Category)
            .Include(i => i.Address)
            .Include(i => i.User).ThenInclude(i => i.Media)
            .Include(i => i.ProductOwner).ThenInclude(i => i.Media)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);

        if (i != null && i.Status == OrderStatuses.Pending && i.OrderDetails != null)
        {
            foreach (var item in i.OrderDetails)
            {
                if (item.Category != null)
                {
                    item.Category.Price = item.Product?.Price ?? item.Price;
                    _dbContext.Update(item.Category);
                    await _dbContext.SaveChangesAsync();
                }

            }
        }
        return new GenericResponse<OrderEntity>(i);
    }

    public async Task<GenericResponse> Delete(Guid id)
    {
        await _dbContext.Set<OrderDetailEntity>().Where(i => i.OrderId == id).ExecuteDeleteAsync();
        await _dbContext.Set<OrderEntity>().Where(i => i.Id == id).ExecuteDeleteAsync();
        return new GenericResponse();
    }

    public async Task<GenericResponse<OrderDetailEntity?>> CreateOrderDetail(OrderDetailCreateUpdateDto dto)
    {
        ProductEntity p = (await _dbContext.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProductId))!;
        CategoryEntity c = (await _dbContext.Set<CategoryEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.CategoryId))!;
        OrderEntity o = await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails.Where(x => x.DeletedAt == null))
            .FirstOrDefaultAsync(x => x.Id == dto.OrderId);

        IEnumerable<string?>? q = o.OrderDetails?.GroupBy(x => x.Product?.UserId).Select(z => z.Key);
        if (q.Count() > 1) return new GenericResponse<OrderDetailEntity?>(null, UtilitiesStatusCodes.MultipleSeller);

        EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity
        {
            ProductId = dto.ProductId,
            Count = dto.Count,
            OrderId = dto.OrderId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CategoryId = dto.CategoryId,
            Price = c.Price * dto.Count
        });

        o.OrderDetails.Append(orderDetailEntity.Entity);

        o.TotalPrice = o.OrderDetails.Where(o => o.DeletedAt == null).Sum(x => x.Price ?? 0);
        o.UpdatedAt = DateTime.Now;
        o.ProductOwnerId = q.First() ?? p.UserId;
        _dbContext.Set<OrderEntity>().Update(o);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<OrderDetailEntity?>(orderDetailEntity.Entity);
    }

    public async Task<GenericResponse<OrderDetailEntity?>> UpdateOrderDetail(OrderDetailCreateUpdateDto dto)
    {
        OrderDetailEntity? ode = await _dbContext.Set<OrderDetailEntity>()
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == dto.OrderDetailId);

        if (ode.Category.Stock >= dto.Count)
        {
            ode.Count = dto.Count;
            ode.Price += ode.Price * ode.Count;
            _dbContext.Set<OrderDetailEntity>().Update(ode);
            await _dbContext.SaveChangesAsync();

            OrderEntity? oe = await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails.Where(x => x.DeletedAt == null))
                .FirstOrDefaultAsync(x => x.Id == dto.OrderId);

            foreach (OrderDetailEntity i in oe.OrderDetails) oe.TotalPrice = i.Price;

            _dbContext.Set<OrderEntity>().Update(oe);
            await _dbContext.SaveChangesAsync();
            return new GenericResponse<OrderDetailEntity?>(ode);
        }
        return new GenericResponse<OrderDetailEntity?>(null, UtilitiesStatusCodes.OutOfStock);
    }

    public async Task<GenericResponse> DeleteOrderDetail(Guid id)
    {
        OrderDetailEntity? e = await _dbContext.Set<OrderDetailEntity>()
            .Include(e => e.Order).ThenInclude(d => d.OrderDetails).ThenInclude(d1 => d1.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
        e.DeletedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync();
        e.Order.TotalPrice = e.Order.OrderDetails?.Where(o => o.DeletedAt == null).Sum(x => x.Price ?? 0);
        e.Order.UpdatedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public async Task<GenericResponse<OrderEntity?>> CreateUpdateOrderDetail(OrderDetailCreateUpdateDto dto)
    {
        ProductEntity p = (await _dbContext.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProductId))!;
        CategoryEntity c = (await _dbContext.Set<CategoryEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.CategoryId))!;
        OrderEntity o = await _dbContext.Set<OrderEntity>().Include(x => x.OrderDetails.Where(x => x.DeletedAt == null)).ThenInclude(x => x.Category)
            .FirstOrDefaultAsync(f => f.ProductOwnerId == p.UserId && f.UserId == _userId);

        if (o is null)
        {
            var address = _dbContext.Set<AddressEntity>().Where(f => f.UserId == _userId && f.DeletedAt == null);

            EntityEntry<OrderEntity> orderEntity = await _dbContext.Set<OrderEntity>().AddAsync(new OrderEntity
            {
                UserId = _userId,
                AddressId = address != null && address.Count() >= 1 ? address.FirstOrDefault(f => f.IsDefault)?.Id ?? null : null,
                Status = OrderStatuses.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ProductOwnerId = p.UserId,
                OrderType = dto.OrderType
            });

            EntityEntry<OrderDetailEntity> oDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity
            {
                ProductId = dto.ProductId,
                Count = dto.Count,
                OrderId = dto.OrderId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CategoryId = dto.CategoryId,
                Price = c.Price * dto.Count
            });

            await _dbContext.SaveChangesAsync();
            return new GenericResponse<OrderEntity?>(orderEntity.Entity);
        }

        if (o.OrderDetails?.Any(a => a.Category?.Type != c.Type) ?? false || o.OrderType != dto.OrderType)
            return new GenericResponse<OrderEntity?>(null, UtilitiesStatusCodes.OrderCantHaveBothTypeOfOrderCategory);

        EntityEntry<OrderDetailEntity> orderDetailEntity = await _dbContext.Set<OrderDetailEntity>().AddAsync(new OrderDetailEntity
        {
            ProductId = dto.ProductId,
            Count = dto.Count,
            OrderId = dto.OrderId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CategoryId = dto.CategoryId,
            Price = c.Price * dto.Count
        });

        o.OrderDetails.Append(orderDetailEntity.Entity);
        o.TotalPrice = o.OrderDetails.Where(o => o.DeletedAt == null).Sum(x => x.Price ?? 0);
        o.UpdatedAt = DateTime.Now;
        o.ProductOwnerId = p.UserId;
        _dbContext.Set<OrderEntity>().Update(o);
        await _dbContext.SaveChangesAsync();

        return new GenericResponse<OrderEntity?>(o);
    }

    public async Task<GenericResponse> Vote(OrderVoteDto dto)
    {
        var orderDetail = await _dbContext.Set<OrderDetailEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id && f.DeletedAt == null);
        if (orderDetail is null)
            return new GenericResponse(UtilitiesStatusCodes.NotFound);

        var order = await _dbContext.Set<OrderEntity>().FirstOrDefaultAsync(f => f.Id == orderDetail.OrderId && f.Status == OrderStatuses.Complete && f.DeletedAt == null && f.UserId == _userId);
        if (order is null)
            return new GenericResponse(UtilitiesStatusCodes.BadRequest);

        orderDetail.Vote = dto.Vote < 1 ? null : dto.Vote;
        _dbContext.Update(orderDetail);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse(UtilitiesStatusCodes.Success);
    }
}