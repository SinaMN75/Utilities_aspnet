using Microsoft.EntityFrameworkCore;
using Utilities_aspnet.Utilities;

namespace Utilities_aspnet.Repositories;

public interface IProductRepository
{
    Task<GenericResponse<ProductEntity>> Create(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse<ProductEntity>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto);
    Task<GenericResponse<ProductEntity?>> ReadById(Guid id, CancellationToken ct);
    Task<GenericResponse<ProductEntity>> Update(ProductCreateUpdateDto dto, CancellationToken ct);
    Task<GenericResponse> Delete(Guid id, CancellationToken ct);
    Task<GenericResponse> SimpleSell(SimpleSellDto dto);
    Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto);
    GenericResponse<IQueryable<ReactionEntity>> ReadReactionsById(Guid id);
}

public class ProductRepository : IProductRepository
{
    private readonly IConfiguration _config;
    private readonly DbContext _dbContext;
    private readonly IMediaRepository _mediaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly string? _userId;

    public ProductRepository(
        DbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        IMediaRepository mediaRepository,
        IUserRepository userRepository,
        IConfiguration config,
        IPromotionRepository promotionRepository)
    {
        _dbContext = dbContext;
        _mediaRepository = mediaRepository;
        _userRepository = userRepository;
        _config = config;
        _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
        _promotionRepository = promotionRepository;
    }

    public async Task<GenericResponse<ProductEntity>> Create(ProductCreateUpdateDto dto, CancellationToken ct)
    {
        AppSettings appSettings = new();
        _config.GetSection("AppSettings").Bind(appSettings);
        var overUsedCheck = Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateProduct , null , dto.UseCase, appSettings.UsageRules!);
        if (overUsedCheck.Item1)
            return new GenericResponse<ProductEntity>(null,overUsedCheck.Item2);

        ProductEntity entity = new();

        if (dto.ProductInsight is not null) dto.ProductInsight.UserId = _userId;

        ProductEntity e = await entity.FillData(dto, _dbContext);
        e.VisitsCount = 1;
        e.UserId = _userId;
        e.CreatedAt = DateTime.Now;

        EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new GenericResponse<ProductEntity>(i.Entity);
    }

    public async Task<GenericResponse<ProductEntity>> CreateWithFiles(ProductCreateUpdateDto dto, CancellationToken ct)
    {
        AppSettings appSettings = new();
        _config.GetSection("AppSettings").Bind(appSettings);
        var overUsedCheck = Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, CallerType.CreateProduct, null, dto.UseCase, appSettings.UsageRules!);
        if (overUsedCheck.Item1)
            return new GenericResponse<ProductEntity>(null, overUsedCheck.Item2);

        ProductEntity entity = new();
        if (dto.Upload is not null)
        {
            foreach (UploadDto uploadDto in dto.Upload) await _mediaRepository.Upload(uploadDto);
        }

        if (dto.ProductInsight is not null)
            dto.ProductInsight.UserId = _userId;

        ProductEntity e = await entity.FillData(dto, _dbContext);
        e.VisitsCount = 1;
        e.UserId = _userId;
        e.CreatedAt = DateTime.Now;

        EntityEntry<ProductEntity> i = await _dbContext.Set<ProductEntity>().AddAsync(e, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new GenericResponse<ProductEntity>(i.Entity);
    }

    public async Task<GenericResponse<IQueryable<ProductEntity>>> Filter(ProductFilterDto dto)
    {
        IQueryable<ProductEntity> q = _dbContext.Set<ProductEntity>();
        q = q.Where(x => x.DeletedAt == null);
        if (!dto.ShowExpired) q = q.Where(w => w.ExpireDate == null || w.ExpireDate >= DateTime.Now);

        if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => (x.Title ?? "").Contains(dto.Title!));
        if (dto.Subtitle.IsNotNullOrEmpty()) q = q.Where(x => (x.Subtitle ?? "").Contains(dto.Subtitle!));
        if (dto.Type.IsNotNullOrEmpty()) q = q.Where(x => (x.Type ?? "").Contains(dto.Type!));
        if (dto.Details.IsNotNullOrEmpty()) q = q.Where(x => (x.Details ?? "").Contains(dto.Details!));
        if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => (x.Description ?? "").Contains(dto.Description!));
        if (dto.Author.IsNotNullOrEmpty()) q = q.Where(x => (x.Author ?? "").Contains(dto.Author!));
        if (dto.Email.IsNotNullOrEmpty()) q = q.Where(x => (x.Email ?? "").Contains(dto.Email!));
        if (dto.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => (x.PhoneNumber ?? "").Contains(dto.PhoneNumber!));
        if (dto.Address.IsNotNullOrEmpty()) q = q.Where(x => (x.Address ?? "").Contains(dto.Address!));
        if (dto.KeyValues1.IsNotNullOrEmpty()) q = q.Where(x => (x.KeyValues1 ?? "").Contains(dto.KeyValues1!));
        if (dto.KeyValues2.IsNotNullOrEmpty()) q = q.Where(x => (x.KeyValues2 ?? "").Contains(dto.KeyValues2!));
        if (dto.Unit.IsNotNullOrEmpty()) q = q.Where(x => x.Unit == dto.Unit);
        if (dto.UseCase.IsNotNullOrEmpty()) q = q.Where(x => x.UseCase.Contains(dto.UseCase));
        if (dto.State.IsNotNullOrEmpty()) q = q.Where(x => x.State == dto.State);
        if (dto.StateTr1.IsNotNullOrEmpty()) q = q.Where(x => x.StateTr1 == dto.StateTr2);
        if (dto.StateTr2.IsNotNullOrEmpty()) q = q.Where(x => x.StateTr2 == dto.StateTr2);
        if (dto.StartPriceRange.HasValue) q = q.Where(x => x.Price >= dto.StartPriceRange);
        if (dto.Currency.HasValue) q = q.Where(x => x.Currency == dto.Currency);
        if (dto.HasComment.IsTrue()) q = q.Where(x => x.Comments.Any());
        if (dto.HasOrder.IsTrue()) q = q.Where(x => x.OrderDetails.Any());
        if (dto.HasDiscount.IsTrue()) q = q.Where(x => x.DiscountPercent != null || x.DiscountPrice != null);
        if (dto.EndPriceRange.HasValue) q = q.Where(x => x.Price <= dto.EndPriceRange);
        if (dto.Enabled.HasValue) q = q.Where(x => x.Enabled == dto.Enabled);
        if (dto.VisitsCount.HasValue) q = q.Where(x => x.VisitsCount == dto.VisitsCount);
        if (dto.Length.HasValue) q = q.Where(x => x.Length.ToInt() == dto.Length.ToInt());
        if (dto.ResponseTime.HasValue) q = q.Where(x => x.ResponseTime.ToInt() == dto.ResponseTime.ToInt());
        if (dto.OnTimeDelivery.HasValue) q = q.Where(x => x.OnTimeDelivery.ToInt() == dto.OnTimeDelivery.ToInt());
        if (dto.Length.HasValue) q = q.Where(x => x.Length.ToInt() == dto.Length.ToInt());
        if (dto.Status.HasValue) q = q.Where(x => x.Status == dto.Status);
        if (dto.Width.HasValue) q = q.Where(x => x.Width.ToInt() == dto.Width.ToInt());
        if (dto.Height.HasValue) q = q.Where(x => x.Height.ToInt() == dto.Height.ToInt());
        if (dto.Weight.HasValue) q = q.Where(x => x.Weight.ToInt() == dto.Weight.ToInt());
        if (dto.MinOrder.HasValue) q = q.Where(x => x.MinOrder >= dto.MinOrder);
        if (dto.MaxOrder.HasValue) q = q.Where(x => x.MaxOrder <= dto.MaxOrder);
        if (dto.MinPrice.HasValue) q = q.Where(x => x.MinPrice >= dto.MinPrice);
        if (dto.MaxPrice.HasValue) q = q.Where(x => x.MaxPrice <= dto.MaxPrice);
        if (dto.StartDate.HasValue) q = q.Where(x => x.StartDate >= dto.StartDate);
        if (dto.EndDate.HasValue) q = q.Where(x => x.EndDate <= dto.EndDate);
        if (dto.Query.IsNotNullOrEmpty())
            q = q.Where(x => (x.Title ?? "").Contains(dto.Query!) ||
                             (x.Subtitle ?? "").Contains(dto.Query!) ||
                             (x.Author ?? "").Contains(dto.Query!) ||
                             (x.Details ?? "").Contains(dto.Query!) ||
                             (x.Description ?? "").Contains(dto.Query!));

        if (dto.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => dto.Categories!.ToList().Contains(y.Id)));
        if (dto.UserIds.IsNotNullOrEmpty()) q = q.Where(x => dto.UserIds!.Contains(x.UserId));

        if (dto.ShowCategories.IsTrue()) q = q.Include(i => i.Categories);
        if (dto.ShowCategoriesFormFields.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.FormFields);
        if (dto.ShowCategoryMedia.IsTrue()) q = q.Include(i => i.Categories)!.ThenInclude(i => i.Media);
        if (dto.ShowComments.IsTrue()) q = q.Include(i => i.Comments)!.ThenInclude(i => i.LikeComments);
        if (dto.ShowOrders.IsTrue()) q = q.Include(i => i.OrderDetails);
        if (dto.ShowForms.IsTrue()) q = q.Include(i => i.Forms);
        if (dto.ShowFormFields.IsTrue()) q = q.Include(i => i.Forms)!.ThenInclude(i => i.FormField);
        if (dto.ShowMedia.IsTrue()) q = q.Include(i => i.Media);
        if (dto.ShowVisitProducts.IsTrue()) q = q.Include(i => i.VisitProducts);
        if (dto.OrderByVotes.IsTrue()) q = q.OrderBy(x => x.VoteCount);
        if (dto.OrderByVotesDecending.IsTrue()) q = q.OrderByDescending(x => x.VoteCount);
        if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
        if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
        if (dto.OrderByPriceAccending.IsTrue()) q = q.OrderBy(x => x.Price);
        if (dto.OrderByPriceDecending.IsTrue()) q = q.OrderByDescending(x => x.Price);
        if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
        if (dto.OrderByCreaedDateDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
        if (dto.OrderByAgeCategory.IsTrue()) q = q.OrderBy(o => o.AgeCategory);
        if (dto.OrderByMostUsedHashtag.IsTrue()) q = q.OrderBy(o => o.Categories.Count(c => c.UseCase.ToLower() == "tag"));
        //if (dto.OrderByFavorites.IsTrue())
        //{
        //    q = q.Include(i => i.Bookmarks);
        //    var user = _dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == _userId);
        //    if (user.BookmarkedProducts.IsNotNullOrEmpty())
        //    {
        //        var bookmarked = user?.BookmarkedProducts.Split(",").ToList();
        //        bookmarked.RemoveAt(0);
        //        if (bookmarked is not null)
        //        {
        //            foreach (var item in q.Where(w=>w.Bookmarks != null && w.Bookmarks.Count() != 0))
        //            {
        //                item.Bookmarks = item.Bookmarks.OrderBy(o => bookmarked.IndexOf(o.Id.ToString()));
        //            }
        //            q = q.OrderBy(o => o.Bookmarks);
        //        }
        //    }
        //}
        if (dto.OrderByCategory.IsTrue()) q = q.AsEnumerable().OrderBy(o => o.Categories != null && o.Categories.Any()
                                               ? o.Categories.OrderBy(op => op.Title)
                                               .Select(s=>s.Title)
                                               .FirstOrDefault() ?? string.Empty : string.Empty).AsQueryable();
        if (dto.ShowCreator.IsTrue())
        {
            q = q.Include(i => i.User).ThenInclude(x => x!.Media);
            q = q.Include(i => i.User).ThenInclude(x => x!.Categories);
        }

        //ToCheck : in query besyar query sanginiye va momkene moshkel saz beshe ba in structure i ke darim
        //     if (!dto.ShowBlockedUsers.IsTrue())
        //     {
        //		   var list = q.ToList();
        //		   var tempList = q.ToList();
        //         foreach (var product in list)
        //         {
        //             var blockedState = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == product.UserId), _dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == _userId));
        //             if (blockedState.Item1) tempList.Remove(product);
        //         }
        //         q = tempList.AsQueryable();
        //     }

        if (_userId.IsNotNullOrEmpty())
        {
            UserEntity user = (await _userRepository.ReadByIdMinimal(_userId))!;
            if (dto.IsFollowing.IsTrue()) q = q.Where(i => user.FollowingUsers.Contains(i.UserId!));
            if (dto.IsBookmarked.IsTrue()) q = q.Where(i => user.BookmarkedProducts.Contains(i.Id.ToString()));
            if (dto.IsMyBoughtList.IsTrue()) q = q.Where(i => user.BoughtProduts.Contains(i.Id.ToString()));
        }

        if (dto.IsBoosted) q = q.OrderBy(o => o.IsBoosted);

        int totalCount = q.Count();
        q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

        return new GenericResponse<IQueryable<ProductEntity>>(q.AsSingleQuery())
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
            .Include(i => i.Reports)
            .Include(i => i.Comments)!.ThenInclude(x => x.LikeComments)
            .Include(i => i.User).ThenInclude(x => x.Media)
            .Include(i => i.User).ThenInclude(x => x.Categories)
            .Include(i => i.Forms)!.ThenInclude(x => x.FormField)
            .Include(i => i.VisitProducts)!.ThenInclude(i => i.User)
            .Include(i => i.ProductInsights)
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null, ct);
        if (i == null) return new GenericResponse<ProductEntity?>(null, UtilitiesStatusCodes.NotFound);

        if (_userId.IsNotNullOrEmpty())
        {
            UserEntity? user = await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == _userId, ct);
            if (!user.VisitedProducts.Contains(i.Id.ToString()))
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
                    UserId = user.Id,
                };
                await _dbContext.Set<VisitProducts>().AddAsync(visitProduct, ct);
                if (string.IsNullOrEmpty(i.SeenUsers)) i.SeenUsers = user.Id;
                else i.SeenUsers = i.SeenUsers + "," + user.Id;
            }
            if (i.VisitProducts != null && !i.VisitProducts.Any()) i.VisitsCount = 1;
            else if (i.VisitProducts != null) i.VisitsCount = i.VisitProducts.Count() + 1;
            _dbContext.Update(i);
            await _dbContext.SaveChangesAsync(ct);
        }

        if (i.ProductInsights?.Any() != null)
        {
            List<IGrouping<ReactionEntity?, ProductInsight>> psGrouping = i.ProductInsights.GroupBy(g => g.Reaction).ToList();
            i.ProductInsights = null;
            List<ProductInsight> productInsights = new();
            foreach (IGrouping<ReactionEntity?, ProductInsight> item in psGrouping)
            {
                item.FirstOrDefault().Count = item.Count();
                productInsights.Add(item.FirstOrDefault());
            }
            i.ProductInsights = productInsights;
        }

        i.Comments = _dbContext.Set<CommentEntity>().Where(w => w.ProductId == i.Id && w.DeletedAt == null);

        var completeOrder = _dbContext.Set<OrderEntity>().Where(c => c.OrderDetails != null && c.OrderDetails.Any(w => w.ProductId.HasValue && w.ProductId.Value == i.Id) && c.Status == OrderStatuses.Complete);
        var displayOrderComplete = Utils.DisplayCountOfCompleteOrder(completeOrder.Count());
        i.SuccessfulPurchase = displayOrderComplete;

        var isUserBuyIt = completeOrder.Any(a => a.UserId == _userId);
        if (isUserBuyIt) i.Media?.Select(s => s.Link == "");

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
        ProductEntity? i = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i != null)
        {
            i.DeletedAt = DateTime.Now;
            _dbContext.Update(i);
            await _dbContext.SaveChangesAsync(ct);
            return new GenericResponse(message: "Deleted");
        }
        return new GenericResponse(UtilitiesStatusCodes.NotFound, "Notfound");
    }

    public async Task<GenericResponse> SimpleSell(SimpleSellDto dto)
    {
        UserEntity buyer = (await _userRepository.ReadByIdMinimal(dto.BuyerUserId))!;
        if (!buyer.BoughtProduts.Contains(dto.ProductId.ToString()))
        {
            await _userRepository.Update(new UserCreateUpdateDto
            {
                Id = dto.BuyerUserId,
                BoughtProduts = buyer.BoughtProduts + "," + dto.ProductId
            });
        }
        return new GenericResponse();
    }

    public async Task<GenericResponse> CreateReaction(ReactionCreateUpdateDto dto)
    {
        var reaction = await _dbContext.Set<ReactionEntity>().FirstOrDefaultAsync(f => f.UserId == _userId && f.ProductId == dto.ProductId);
        if (reaction is not null && reaction.Reaction != dto.Reaction)
        {
            reaction.Reaction = dto.Reaction;
            reaction.UpdatedAt = DateTime.UtcNow;
            _dbContext.Update(reaction);
        }
        else
        {
            reaction = new()
            {
                CreatedAt = DateTime.UtcNow,
                ProductId = dto.ProductId,
                Reaction = dto.Reaction,
                UserId = _userId,
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
        entity.Details = dto.Details ?? entity.Details;
        entity.Author = dto.Author ?? entity.Author;
        entity.PhoneNumber = dto.PhoneNumber ?? entity.PhoneNumber;
        entity.Link = dto.Link ?? entity.Link;
        entity.Website = dto.Website ?? entity.Website;
        entity.Email = dto.Email ?? entity.Email;
        entity.Type = dto.Type ?? entity.Type;
        entity.State = dto.State ?? entity.State;
        entity.StateTr1 = dto.StateTr1 ?? entity.StateTr1;
        entity.StateTr2 = dto.StateTr2 ?? entity.StateTr2;
        entity.Latitude = dto.Latitude ?? entity.Latitude;
        entity.RelatedIds = dto.RelatedIds ?? entity.RelatedIds;
        entity.ResponseTime = dto.ResponseTime ?? entity.ResponseTime;
        entity.OnTimeDelivery = dto.OnTimeDelivery ?? entity.OnTimeDelivery;
        entity.DiscountPrice = dto.DiscountPrice ?? entity.DiscountPrice;
        entity.DiscountPercent = dto.DiscountPercent ?? entity.DiscountPercent;
        entity.Longitude = dto.Longitude ?? entity.Longitude;
        entity.Description = dto.Description ?? entity.Description;
        entity.UseCase = dto.UseCase ?? entity.UseCase;
        entity.Price = dto.Price ?? entity.Price;
        entity.Enabled = dto.Enabled ?? entity.Enabled;
        entity.VisitsCount = dto.VisitsCount ?? entity.VisitsCount;
        entity.Length = dto.Length ?? entity.Length;
        entity.Width = dto.Width ?? entity.Width;
        entity.Height = dto.Height ?? entity.Height;
        entity.Weight = dto.Weight ?? entity.Weight;
        entity.MinOrder = dto.MinOrder ?? entity.MinOrder;
        entity.MaxOrder = dto.MaxOrder ?? entity.MaxOrder;
        entity.MinPrice = dto.MinPrice ?? entity.MinPrice;
        entity.KeyValues1 = dto.KeyValues1 ?? entity.KeyValues1;
        entity.KeyValues2 = dto.KeyValues2 ?? entity.KeyValues2;
        entity.MaxPrice = dto.MaxPrice ?? entity.MaxPrice;
        entity.Unit = dto.Unit ?? entity.Unit;
        entity.Stock = dto.Stock ?? entity.Stock;
        entity.Address = dto.Address ?? entity.Address;
        entity.StartDate = dto.StartDate ?? entity.StartDate;
        entity.EndDate = dto.EndDate ?? entity.EndDate;
        entity.Status = dto.Status ?? entity.Status;
        entity.Currency = dto.Currency ?? entity.Currency;
        entity.DeletedAt = dto.DeletedAt ?? entity.DeletedAt;
        entity.UpdatedAt = DateTime.Now;
        entity.ExpireDate = dto.ExpireDate ?? entity.ExpireDate;
        entity.AgeCategory = dto.AgeCategory ?? entity.AgeCategory;
        entity.ProductState = dto.ProductState ?? entity.ProductState;
        entity.ShippingCost = dto.ShippingCost ?? entity.ShippingCost;
        entity.ShippingTime = dto.ShippingTime ?? entity.ShippingTime;
        entity.IsPhysical = dto.IsPhysical;
        entity.IsBoosted = dto.IsBoosted;

        if (dto.VisitsCountPlus.HasValue)
        {
            if (entity.VisitsCount == null) entity.VisitsCount = 1;
            else entity.VisitsCount += dto.VisitsCountPlus;
        }

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
                {
                    pI = new ProductInsight
                    {
                        UserId = e.Id,
                        Reaction = pInsight.Reaction,
                        UpdatedAt = DateTime.Now
                    };
                }
                else
                {
                    pI = new ProductInsight
                    {
                        UserId = e.Id,
                        Reaction = pInsight.Reaction,
                        CreatedAt = DateTime.Now
                    };
                }
                await context.Set<ProductInsight>().AddAsync(pI);
                productInsights.Add(pI);
            }
            entity.ProductInsights = productInsights;
        }

        if(dto.ParentId is not null)
        {
            entity.ParentId = dto.ParentId ?? entity.ParentId;
            entity.Product = context.Set<ProductEntity>().FirstOrDefault(f => f.Id == dto.ParentId);
        }

        return entity;
    }
}