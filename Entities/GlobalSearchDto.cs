namespace Utilities_aspnet.Entities;

public class GlobalSearchDto {
	public IQueryable<UserEntity>? Users { get; set; }
	public IQueryable<ProductEntity>? Products { get; set; }
	public IQueryable<CategoryEntity>? Categories { get; set; }
}

public class GlobalSearchParams {
	public string Query { get; set; } = "";
	public IEnumerable<Guid>? Categories { get; set; }
	public int PageSize { get; set; } = 1000;
	public int PageNumber { get; set; } = 1;
	public bool Oldest { get; set; } = false;
	public bool User { get; set; } = true;
	public bool Product { get; set; } = true;
	public bool Category { get; set; } = true;
	public bool Minimal { get; set; } = false;
}