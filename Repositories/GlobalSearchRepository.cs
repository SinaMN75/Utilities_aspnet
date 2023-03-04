namespace Utilities_aspnet.Repositories;

public interface IGlobalSearchRepository {
	GenericResponse<GlobalSearchDto> Filter(GlobalSearchParams filter, string? userId);
}

public class GlobalSearchRepository : IGlobalSearchRepository {
	private readonly DbContext _dbContext;

	public GlobalSearchRepository(DbContext dbContext) => _dbContext = dbContext;

	public GenericResponse<GlobalSearchDto> Filter(GlobalSearchParams filter, string? userId) {
		GlobalSearchDto model = new();

		IQueryable<CategoryEntity> categoryList = Enumerable.Empty<CategoryEntity>().AsQueryable();
		IQueryable<UserEntity> userList = Enumerable.Empty<UserEntity>().AsQueryable();
		IQueryable<ProductEntity> productList = Enumerable.Empty<ProductEntity>().AsQueryable();

		if (filter.Category)
			categoryList = _dbContext.Set<CategoryEntity>().AsNoTracking()
				.Include(x => x.Media)
				.Where(x => x.DeletedAt == null && (x.Title.Contains(filter.Query) ||
				                                    x.Subtitle.Contains(filter.Query) ||
				                                    x.TitleTr1.Contains(filter.Query) ||
				                                    x.TitleTr2.Contains(filter.Query)));

		if (filter.User)
			userList = _dbContext.Set<UserEntity>()
				.Where(x => x.FullName.Contains(filter.Query) ||
				            x.AppUserName.Contains(filter.Query) ||
				            x.FirstName.Contains(filter.Query) ||
				            x.LastName.Contains(filter.Query))
				.Include(u => u.Media)
				.Include(u => u.Categories)
				.Include(u => u.Products)!
				.ThenInclude(u => u.Media)
				.AsNoTracking();

		if (filter.Product) {
			productList = _dbContext.Set<ProductEntity>()
				.Where(x => x.DeletedAt == null && (x.Title.Contains(filter.Query) ||
				                                    x.Subtitle.Contains(filter.Query) ||
				                                    x.Description.Contains(filter.Query) ||
				                                    x.Details.Contains(filter.Query))).AsNoTracking();

			if (filter.Minimal)
				productList = productList
					.Include(i => i.Media)
					.Include(i => i.Categories)
					.Include(i => i.User).ThenInclude(x => x.Media)
					.Include(i => i.User).ThenInclude(x => x.Categories);
			else
				productList = productList
					.Include(i => i.Media)
					.Include(i => i.Categories)
					.Include(i => i.Comments.Where(x => x.ParentId == null)).ThenInclude(x => x.Children)
					.Include(i => i.Comments.Where(x => x.ParentId == null)).ThenInclude(x => x.Media)
					.Include(i => i.Reports)
					.Include(i => i.Votes)
					.Include(i => i.User).ThenInclude(x => x.Media)
					.Include(i => i.User).ThenInclude(x => x.Categories)
					.Include(i => i.Bookmarks)
					.Include(i => i.Forms)!.ThenInclude(x => x.FormField)
					.Include(i => i.Teams)!.ThenInclude(x => x.User)!.ThenInclude(x => x.Media)
					.Include(i => i.VoteFields)!.ThenInclude(x => x.Votes);
		}

		if (filter.Categories.IsNotNullOrEmpty()) {
			productList = productList.Where(x => x.Categories.Any(x => filter.Categories.Contains(x.Id) && x.DeletedAt == null));
			categoryList = categoryList.Where(x => filter.Categories.Contains(x.Id) && x.DeletedAt == null);
			userList = userList.Where(x => x.Categories.Any(x => filter.Categories.Contains(x.Id)) && x.DeletedAt == null);
		}

		if (filter.Oldest) {
			categoryList = categoryList.OrderBy(x => x.CreatedAt);
			userList = userList.OrderBy(x => x.CreatedAt);
			productList = productList.OrderBy(x => x.CreatedAt);
		}

		categoryList = categoryList.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
		userList = userList.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
		productList = productList.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

		model.Categories = categoryList;
		model.Users = userList;
		model.Products = productList;
		return new GenericResponse<GlobalSearchDto>(model);
	}
}