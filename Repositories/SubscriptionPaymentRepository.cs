using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities_aspnet.Utilities;

namespace Utilities_aspnet.Repositories
{
    public interface ISubscriptionPaymentRepository
    {
        Task<GenericResponse<SubscriptionPaymentEntity?>> Create(SubscriptionPaymentCreateUpdateDto dto);
        GenericResponse<IEnumerable<SubscriptionPaymentEntity>> Filter(SubscriptionPaymentFilter dto);
        Task<GenericResponse<SubscriptionPaymentEntity?>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct);
        Task<GenericResponse> Delete(Guid id, CancellationToken ct);
    }
    public class SubscriptionPaymentRepository : ISubscriptionPaymentRepository
    {
        private readonly DbContext _dbContext;
        private readonly string? _userId;
        public SubscriptionPaymentRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
        }

        public async Task<GenericResponse<SubscriptionPaymentEntity?>> Create(SubscriptionPaymentCreateUpdateDto dto)
        {
            UserEntity? userUpgraded = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == dto.UserId);
            PromotionEntity? promotionEntity = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.Id == dto.PromotionId);
            if (userUpgraded == null && promotionEntity == null) return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.Unhandled);
            if (userUpgraded != null && userUpgraded?.Id != _userId) return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.UserNotFound); // Is It Ok?

            SubscriptionPaymentEntity? e = new()
            {
                PromotionId = dto.PromotionId,
                UserId = _userId,
                CreatedAt = DateTime.Now,
                SubscriptionType = promotionEntity != null ? SubscriptionType.Promotion : SubscriptionType.UpgradeAccount,
                Status = OrderStatuses.Pending,
            };
            e.FillData(dto);

            await _dbContext.AddAsync(e);
            await _dbContext.SaveChangesAsync();
            return new GenericResponse<SubscriptionPaymentEntity?>(e);
        }

        public async Task<GenericResponse> Delete(Guid id, CancellationToken ct)
        {
            var subscription = await _dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(f => f.Id == id, ct);
            if (subscription == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
            _dbContext.Remove(subscription);
            await _dbContext.SaveChangesAsync();
            return new GenericResponse();
        }

        public GenericResponse<IEnumerable<SubscriptionPaymentEntity>> Filter(SubscriptionPaymentFilter dto)
        {
            IQueryable<SubscriptionPaymentEntity> q = _dbContext.Set<SubscriptionPaymentEntity>().AsNoTracking();

            if (dto.ShowPromotion.IsTrue()) q = q.Include(x => x.Promotion);
            if (dto.ShowUser.IsTrue()) q = q.Include(x => x.User);
            if (dto.OrderByAmount.IsTrue()) q = q.OrderBy(x => x.Amount);
            if (dto.OrderBySubscriptionType.IsTrue()) q = q.OrderBy(x => x.SubscriptionType);
            if (dto.OrderByStatus.IsTrue()) q = q.OrderBy(x => x.Status);

            return new GenericResponse<IEnumerable<SubscriptionPaymentEntity>>(q);
        }

        public async Task<GenericResponse<SubscriptionPaymentEntity?>> Update(SubscriptionPaymentCreateUpdateDto dto, CancellationToken ct)
        {
            var subscription = await _dbContext.Set<SubscriptionPaymentEntity>().FirstOrDefaultAsync(f => f.Id == dto.Id, ct);
            if (subscription == null) return new GenericResponse<SubscriptionPaymentEntity?>(null, UtilitiesStatusCodes.NotFound);
            subscription.FillData(dto);
            _dbContext.Update(subscription);
            await _dbContext.SaveChangesAsync();
            return new GenericResponse<SubscriptionPaymentEntity?>(subscription);
        }
    }
}

public static class SubscriptionPaymentEntityExtension
{
    public static SubscriptionPaymentEntity FillData(this SubscriptionPaymentEntity entity, SubscriptionPaymentCreateUpdateDto dto)
    {
        entity.Status = dto.Status ?? entity.Status;
        entity.SubscriptionType = dto.SubscriptionType ?? entity.SubscriptionType;
        entity.Amount = dto.Amount ?? entity.Amount;
        entity.UpdatedAt = DateTime.Now;
        entity.Description = dto.Description ?? entity.Description;
        return entity;
    }
}
