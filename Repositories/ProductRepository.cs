namespace Utilities_aspnet.Repositories;

public interface IProductRepository
{
    Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse<ProductEntity?>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto);
    Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct);
    Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse> Delete(Guid id, CancellationToken ct);
    Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto);
    GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id);
}

public class ProductRepository : IProductRepository
{
    private readonly IConfiguration _config;
    private readonly DbContext _dbContext;
    private readonly IMediaRepository _mediaRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly string? _userId;
    private readonly IUserRepository _userRepository;
    private readonly IFormRepository _formRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductRepository(
        DbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        IMediaRepository mediaRepository,
        IUserRepository userRepository,
        IConfiguration config,
        IPromotionRepository promotionRepository,
        ICategoryRepository categoryRepository,
        IFormRepository formRepository)
    {
        _dbContext = dbContext;
        _mediaRepository = mediaRepository;
        _userRepository = userRepository;
        _config = config;
        _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
        _promotionRepository = promotionRepository;
        _formRepository = formRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GenericResponse<ProductEntity?>> Create(ProductCreateUpdateDto dto, CancellationToken ct)
    {
        AppSettings appSettings = new();
        _config.GetSection("AppSettings").Bind(appSettings);
        Tuple<bool, UtilitiesStatusCodes> overUsedCheck =
            Utils.IsUserOverused(_dbContext, _userId, CallerType.CreateProduct, null, dto.UseCase, appSettings.UsageRules);
        if (overUsedCheck.Item1) return new GenericResponse<ProductEntity?>(null, overUsedCheck.Item2);

        if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

        ProductEntity e = await new ProductEntity().FillData(dto, _dbContext);
        e.UserId = _userId;
        e.CreatedAt = DateTime.Now;

        EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
        await _dbContext.SaveChangesAsync(ct);

        if (dto.Form is not null) await _formRepository.CreateForm(new FormCreateDto { ProductId = i.Entity.Id, Form = dto.Form });

        return new GenericResponse<ProductEntity?>(i.Entity);
    }

    public async Task<GenericResponse<ProductEntity?>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct)
    {
        AppSettings appSettings = new();
        _config.GetSection("AppSettings").Bind(appSettings);
        Tuple<bool, UtilitiesStatusCodes> overUsedCheck =
            Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateProduct, null, dto.UseCase, appSettings.UsageRules);
        if (overUsedCheck.Item1) return new GenericResponse<ProductEntity?>(null, overUsedCheck.Item2);

        if (dto.Upload is not null)
            foreach (UploadDto uploadDto in dto.Upload)
                await _mediaRepository.Upload(uploadDto);

        if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

        ProductEntity e = await new ProductEntity().FillData(dto, _dbContext);
        e.UserId = _userId;
        e.CreatedAt = DateTime.Now;

        EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new GenericResponse<ProductEntity?>(i.Entity);
    }

    public async Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto)
    {
        IQueryable<ProductEntity> q = _dbContext.Set<ProductEntity>().AsNoTracking().Where(x => x.DeletedAt == null);
        if (!dto.ShowExpired) q = q.Where(w => w.ExpireDate == null || w.ExpireDate >= DateTime.Now);

        if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
        if (dto.Subtitle.IsNotNullOrEmpty()) q = q.Where(x => (x.Subtitle ?? "").Contains(dto.Subtitle!));
        if (dto.Tags.IsNotNullOrEmpty()) q = q.Where(x => (x.JsonDetail.Tags ?? "").Contains(dto.Tags!));
        if (dto.Type.IsNotNullOrEmpty()) q = q.Where(x => (x.Type ?? "").Contains(dto.Type!));
        if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => (x.Description ?? "").Contains(dto.Description!));
        if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => x.UseCase!.Contains(dto.UseCase!));
        if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State == dto.State);
        if (dto.StartPriceRange.HasValue) q = q.Where(x => x.Price >= dto.StartPriceRange);
        if (dto.Currency.HasValue) q = q.Where(x => x.Currency == dto.Currency);
        if (dto.HasDiscount.IsTrue()) q = q.Where(x => x.DiscountPercent != null || x.DiscountPrice != null);
        if (dto.EndPriceRange.HasValue) q = q.Where(x => x.Price <= dto.EndPriceRange);
        if (dto.Enabled.HasValue) q = q.Where(x => x.Enabled == dto.Enabled);
        if (dto.Status.HasValue) q = q.Where(x => x.Status == dto.Status);
        if (dto.Query.IsNotNullOrEmpty())
            q = q.Where(x => (x.Title ?? "").Contains(dto.Query!) || (x.Subtitle ?? "").Contains(dto.Query!) || (x.Description ?? "").Contains(dto.Query!));

        if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
        if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));

        if (dto.ShowCategories.IsTrue()) q = q.Include(i => i.Categories);
        if (dto.ShowCategoriesFormFields.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.FormFields);
        if (dto.ShowCategoryMedia.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.Media);
        if (dto.ShowForms.IsTrue()) q = q.Include(i => i.Forms);
        if (dto.ShowFormFields.IsTrue()) q = q.Include(i => i.Forms)!.ThenInclude(i => i.FormField);
        if (dto.ShowMedia.IsTrue()) q = q.Include(i => i.Media);
        if (dto.ShowVisitProducts.IsTrue()) q = q.Include(i => i.VisitProducts);
        if (dto.OrderByVotes.IsTrue()) q = q.OrderBy(x => x.VoteCount);
        if (dto.OrderByVotesDescending.IsTrue()) q = q.OrderByDescending(x => x.VoteCount);
        if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
        if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
        if (dto.OrderByPriceAscending.IsTrue()) q = q.OrderBy(x => x.Price);
        if (dto.OrderByPriceDescending.IsTrue()) q = q.OrderByDescending(x => x.Price);
        if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
        if (dto.OrderByCreatedDateDescending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
        if (dto.OrderByAgeCategory.IsTrue()) q = q.OrderBy(o => o.AgeCategory);
        if (dto.OrderByMostUsedHashtag.IsTrue()) q = q.OrderBy(o => o.Categories!.Count(c => c.UseCase!.ToLower() == "tag"));

        if (dto.OrderByCategory.IsTrue())
            q = q.AsEnumerable().OrderBy(o => o.Categories != null && o.Categories.Any()
                                             ? o.Categories.OrderBy(op => op.Title)
                                                 .Select(s => s.Title)
                                                 .FirstOrDefault() ?? string.Empty
                                             : string.Empty).AsQueryable();
        if (dto.ShowCreator.IsTrue())
        {
            q = q.Include(i => i.User).ThenInclude(x => x!.Media);
            q = q.Include(i => i.User).ThenInclude(x => x!.Categories);
        }

        if (_userId.IsNotNullOrEmpty())
        {
            UserEntity user = (await _userRepository.ReadByIdMinimal(_userId))!;
            if (dto.IsFollowing.IsTrue()) q = q.Where(i => user.FollowingUsers.Contains(i.UserId!));
            if (dto.IsBookmarked.IsTrue()) q = q.Where(i => user.BookmarkedProducts.Contains(i.Id.ToString()));
        }

        if (dto.Boosted) q = q.OrderByDescending(o => o.Boosted);

        q = q.Include(x => x.Parent).ThenInclude(x => x.Categories);
        q = q.Include(x => x.Parent).ThenInclude(x => x.Media);
        q = q.Include(x => x.Parent).ThenInclude(x => x.User).ThenInclude(x => x.Media);


        int totalCount = q.Count();
        q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

        return new GenericResponse<IQueryable<ProductEntity>>(q)
        {
            TotalCount = totalCount,
            PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
            PageSize = dto.PageSize
        };
    }

    public async Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct)
    {
        ProductEntity? i = await _dbContext.Set<ProductEntity>()
            .Include(i => i.Media)
            .Include(i => i.Categories)!.ThenInclude(x => x.Media)
            .Include(i => i.User).ThenInclude(x => x!.Media)
            .Include(i => i.User).ThenInclude(x => x!.Categories)
            .Include(i => i.Forms)!.ThenInclude(x => x.FormField)
            .Include(i => i.VisitProducts)!.ThenInclude(i => i.User)
            .Include(i => i.ProductInsights)
            .Include(i => i.Parent).ThenInclude(i => i.Categories)
            .Include(i => i.Parent).ThenInclude(i => i.Media)
            .Include(i => i.Parent).ThenInclude(i => i.User).ThenInclude(i => i.Media)
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null, ct);
        if (i == null) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.NotFound);

        if (_userId.IsNotNullOrEmpty())
        {
            UserEntity? user = await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == _userId, ct);
            if (!user!.VisitedProducts.Contains(i.Id.ToString()))
                await _userRepository.Update(new UserCreateUpdateDto
                {
                    Id = _userId,
                    VisitedProducts = user.VisitedProducts + "," + i.Id
                });

            VisitProducts? vp = await _dbContext.Set<VisitProducts>().FirstOrDefaultAsync(a => a.UserId == user.Id && a.ProductId == i.Id, ct);
            if (vp is null)
            {
                VisitProducts visitProduct = new()
                {
                    CreatedAt = DateTime.Now,
                    ProductId = i.Id,
                    UserId = user.Id
                };
                await _dbContext.Set<VisitProducts>().AddAsync(visitProduct, ct);
                if (string.IsNullOrEmpty(i.SeenUsers)) i.SeenUsers = user.Id;
                else i.SeenUsers = i.SeenUsers + "," + user.Id;
            }
            _dbContext.Update(i);
            await _dbContext.SaveChangesAsync(ct);
        }

        if (i.ProductInsights?.Any() != null)
        {
            List<IGrouping<ReactionEntity?, ProductInsight>> psGrouping = i.ProductInsights.GroupBy(g => g.Reaction).ToList();
            i.ProductInsights = null;
            List<ProductInsight?> productInsights = new();
            foreach (IGrouping<ReactionEntity?, ProductInsight> item in psGrouping)
            {
                item.FirstOrDefault()!.Count = item.Count();
                productInsights.Add(item.FirstOrDefault());
            }
            i.ProductInsights = productInsights;
        }

        IQueryable<OrderEntity>? completeOrder = _dbContext.Set<OrderEntity>()
            .Where(c => c.OrderDetails != null && c.OrderDetails.Any(w => w.ProductId.HasValue && w.ProductId.Value == i.Id) &&
                        c.Status == OrderStatuses.Complete);
        string? displayOrderComplete = Utils.DisplayCountOfCompleteOrder(completeOrder.Count());
        i.SuccessfulPurchase = displayOrderComplete;

        bool isUserBuyIt = completeOrder.Any(a => a.UserId == _userId);
        if (isUserBuyIt) i.Media?.Select(s => s.JsonDetail.Link == "");

        await _promotionRepository.UserSeened(i.Id);

        return new GenericResponse<ProductEntity?>(i);
    }

    public async Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct)
    {
        ProductEntity? entity = await _dbContext.Set<ProductEntity>()
            .Include(x => x.Categories)
            .Where(x => x.Id == dto.Id)
            .FirstOrDefaultAsync(ct);

        if (entity == null) return new GenericResponse<ProductEntity>(new ProductEntity());

        if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

        ProductEntity e = await entity.FillData(dto, _dbContext);
        _dbContext.Update(e);
        await _dbContext.SaveChangesAsync(ct);

        return new GenericResponse<ProductEntity>(e);
    }

    public async Task<GenericResponse> Delete(Guid id, CancellationToken ct)
    {
        await _dbContext.Set<ProductEntity>().Where(x => x.Id == id || x.ParentId == id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, DateTime.Now), ct);
        return new GenericResponse();
    }

    public async Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto)
    {
        ReactionEntity? reaction = await _dbContext.Set<ReactionEntity>().FirstOrDefaultAsync(f => f.UserId == _userId && f.ProductId == dto.ProductId);
        if (reaction is not null && reaction.Reaction != dto.Reaction)
        {
            reaction.Reaction = dto.Reaction;
            reaction.UpdatedAt = DateTime.UtcNow;
            _dbContext.Update(reaction);
        }
        else
        {
            reaction = new ReactionEntity
            {
                CreatedAt = DateTime.UtcNow,
                ProductId = dto.ProductId,
                Reaction = dto.Reaction,
                UserId = _userId
            };
            await _dbContext.Set<ReactionEntity>().AddAsync(reaction);
        }
        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id)
    {
        IQueryable<ReactionEntity> reactions = _dbContext.Set<ReactionEntity>()
            .Include(i => i.User)
            .Where(w => w.ProductId == id)
            .OrderBy(o => o.Reaction);
        return new GenericResponse<IQueryable<ReactionEntity>>(reactions);
    }
}

public static class ProductEntityExtension
{
    public static async Task<ProductEntity> FillData(this ProductEntity entity, ProductCreateUpdateDto dto, DbContext context)
    {
        entity.Title = dto.Title ?? entity.Title;
        entity.Subtitle = dto.Subtitle ?? entity.Subtitle;
        entity.Type = dto.Type ?? entity.Type;
        entity.State = dto.State ?? entity.State;
        entity.DiscountPrice = dto.DiscountPrice ?? entity.DiscountPrice;
        entity.DiscountPercent = dto.DiscountPercent ?? entity.DiscountPercent;
        entity.Description = dto.Description ?? entity.Description;
        entity.UseCase = dto.UseCase ?? entity.UseCase;
        entity.Price = dto.Price ?? entity.Price;
        entity.Enabled = dto.Enabled ?? entity.Enabled;
        entity.Stock = dto.Stock ?? entity.Stock;
        entity.Status = dto.Status ?? entity.Status;
        entity.CommentsCount = dto.CommentsCount ?? entity.CommentsCount;
        entity.Currency = dto.Currency ?? entity.Currency;
        entity.ExpireDate = dto.ExpireDate ?? entity.ExpireDate;
        entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;
        entity.ProductState = dto.ProductState ?? entity.ProductState;
        entity.Boosted = dto.Boosted ?? entity.Boosted;
        entity.UpdatedAt = DateTime.Now;
        entity.JsonDetail = new ProductJsonDetail
        {
            Details = dto.Details ?? entity.JsonDetail.Details,
            Author = dto.Author ?? entity.JsonDetail.Author,
            PhoneNumber = dto.PhoneNumber ?? entity.JsonDetail.PhoneNumber,
            Link = dto.Link ?? entity.JsonDetail.Link,
            Website = dto.Website ?? entity.JsonDetail.Website,
            Email = dto.Email ?? entity.JsonDetail.Email,
            Longitude = dto.Longitude ?? entity.JsonDetail.Longitude,
            Latitude = dto.Latitude ?? entity.JsonDetail.Latitude,
            ResponseTime = dto.ResponseTime ?? entity.JsonDetail.ResponseTime,
            OnTimeDelivery = dto.OnTimeDelivery ?? entity.JsonDetail.OnTimeDelivery,
            Length = dto.Length ?? entity.JsonDetail.Length,
            Width = dto.Width ?? entity.JsonDetail.Width,
            Height = dto.Height ?? entity.JsonDetail.Height,
            Weight = dto.Weight ?? entity.JsonDetail.Weight,
            MinOrder = dto.MinOrder ?? entity.JsonDetail.MinOrder,
            MaxOrder = dto.MaxOrder ?? entity.JsonDetail.MaxOrder,
            MinPrice = dto.MinPrice ?? entity.JsonDetail.MinPrice,
            MaxPrice = dto.MaxPrice ?? entity.JsonDetail.MaxPrice,
            Unit = dto.Unit ?? entity.JsonDetail.Unit,
            Address = dto.Address ?? entity.JsonDetail.Address,
            StartDate = dto.StartDate ?? entity.JsonDetail.StartDate,
            EndDate = dto.EndDate ?? entity.JsonDetail.EndDate,
            ShippingCost = dto.ShippingCost ?? entity.JsonDetail.ShippingCost,
            ShippingTime = dto.ShippingTime ?? entity.JsonDetail.ShippingTime,
            Tags = dto.Tags ?? entity.JsonDetail.Tags,
            KeyValue = dto.KeyValue ?? entity.JsonDetail.KeyValue,
            Type1 = dto.Type1 ?? entity.JsonDetail.Type1,
            Type2 = dto.Type2 ?? entity.JsonDetail.Type2,
            KeyValues = dto.KeyValues ?? entity.JsonDetail.KeyValues,
            Attributes = dto.Attributes ?? entity.JsonDetail.Attributes
        };

        if (dto.ScorePlus.HasValue)
        {
            if (entity.VoteCount == null) entity.VoteCount = 1;
            else entity.VoteCount += dto.ScorePlus;
        }

        if (dto.ScoreMinus.HasValue)
        {
            if (entity.VoteCount == null) entity.VoteCount = 1;
            else entity.VoteCount -= dto.ScoreMinus;
        }

        if (dto.Categories.IsNotNull())
        {
            List<CategoryEntity> listCategory = new();
            foreach (Guid item in dto.Categories!)
            {
                CategoryEntity? e = await context.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
                if (e != null) listCategory.Add(e);
            }
            entity.Categories = listCategory;
        }

        if (dto.Teams.IsNotNull())
        {
            string temp = "";
            foreach (string userId in dto.Teams!) temp += userId + ",";
            entity.Teams = temp;
        }

        if (dto.ProductInsight is not null)
        {
            List<ProductInsight> productInsights = new();
            ProductInsightDto? pInsight = dto.ProductInsight;
            UserEntity? e = await context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == pInsight.UserId);
            if (e != null)
            {
                ProductInsight pI;
                ProductInsight? oldProductInsight = await context.Set<ProductInsight>().FirstOrDefaultAsync(f => f.UserId == e.Id && f.ProductId == entity.Id);
                if (oldProductInsight is not null && oldProductInsight.Reaction != pInsight.Reaction)
                    pI = new ProductInsight
                    {
                        UserId = e.Id,
                        Reaction = pInsight.Reaction,
                        UpdatedAt = DateTime.Now
                    };
                else
                    pI = new ProductInsight
                    {
                        UserId = e.Id,
                        Reaction = pInsight.Reaction,
                        CreatedAt = DateTime.Now
                    };
                await context.Set<ProductInsight>().AddAsync(pI);
                productInsights.Add(pI);
            }
            entity.ProductInsights = productInsights;
        }

        if (dto.ParentId is not null)
        {
            entity.ParentId = dto.ParentId ?? entity.ParentId;
            entity.Parent = context.Set<ProductEntity>().FirstOrDefault(f => f.Id == dto.ParentId);
        }

        return entity;
    }
}