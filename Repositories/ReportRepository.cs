namespace Utilities_aspnet.Repositories;

public interface IReportRepository {
	Task<GenericResponse<ReportEntity?>> Create(ReportCreateUpdateDto dto);
	GenericResponse<IQueryable<ReportEntity>> Filter();
	Task<GenericResponse<ReportEntity?>> ReadById(Guid id);
	Task<GenericResponse> Delete(Guid id);
}

public class ReportRepository(DbContext context, IHttpContextAccessor httpContextAccessor) : IReportRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<ReportEntity?>> Create(ReportCreateUpdateDto dto) {
		ReportEntity entity = new() {
			CreatorUserId = _userId,
			Title = dto.Title,
			Description = dto.Description
		};
		if (dto.ProductId.HasValue) entity.ProductId = dto.ProductId;
		if (dto.CommentId.HasValue) entity.CommentId = dto.CommentId;
		if (!dto.UserId.IsNotNullOrEmpty()) entity.UserId = dto.UserId;
		await context.Set<ReportEntity>().AddAsync(entity);
		await context.SaveChangesAsync();
		return await ReadById(entity.Id);
	}

	public GenericResponse<IQueryable<ReportEntity>> Filter() {
		IQueryable<ReportEntity> e = context.Set<ReportEntity>().AsNoTracking().Select(x => new ReportEntity {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Title = x.Title,
			Description = x.Description,
			Comment = new CommentEntity {
				Id = x.Comment!.Id,
				Comment = x.Comment.Comment,
				UserId = x.Comment.UserId,
				Tags = x.Comment.Tags,
				JsonDetail = x.Comment.JsonDetail,
				TargetUserId = x.Comment.TargetUserId,
				ProductId = x.Comment.ProductId
			},
			Product = new ProductEntity {
				Id = x.Product!.Id,
				Title = x.Product.Title,
				Tags = x.Product.Tags,
				JsonDetail = x.Product.JsonDetail,
				ParentId = x.Product.ParentId
			},
			User = new UserEntity {
				Id = x.User!.Id,
				Tags = x.User.Tags,
				JsonDetail = x.User.JsonDetail,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				FullName = x.User.FullName,
				AppUserName = x.User.AppUserName,
				AppEmail = x.User.AppEmail,
				AppPhoneNumber = x.User.AppPhoneNumber,
				Email = x.User.Email,
				PhoneNumber = x.User.PhoneNumber
			},
			CreatorUser = new UserEntity {
				Id = x.CreatorUser!.Id,
				Tags = x.CreatorUser.Tags,
				JsonDetail = x.CreatorUser.JsonDetail,
				FirstName = x.CreatorUser.FirstName,
				LastName = x.CreatorUser.LastName,
				FullName = x.CreatorUser.FullName,
				AppUserName = x.CreatorUser.AppUserName,
				AppEmail = x.CreatorUser.AppEmail,
				AppPhoneNumber = x.CreatorUser.AppPhoneNumber,
				Email = x.CreatorUser.Email,
				PhoneNumber = x.CreatorUser.PhoneNumber
			}
		});
		return new GenericResponse<IQueryable<ReportEntity>>(e);
	}

	public async Task<GenericResponse> Delete(Guid id) {
		await context.Set<ReportEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<ReportEntity?>> ReadById(Guid id) {
		ReportEntity? entity = await context.Set<ReportEntity>()
			.Include(x => x.User)
			.Include(x => x.Product).ThenInclude(x => x!.Media)
			.Include(x => x.Comment)
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == id);
		return new GenericResponse<ReportEntity?>(entity);
	}
}