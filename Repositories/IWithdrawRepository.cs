using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Repositories
{
    public interface IWithdrawRepository
    {
        Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto);
    }

    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly DbContext _dbContext;
        private readonly string? _userId;
        public WithdrawRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
        }
        public async Task<GenericResponse> WalletWithdrawal(WalletWithdrawalDto dto)
        {
            var user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId && f.Suspend != true);
            if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);
            var sheba = dto.ShebaNumber.GetShebaNumber();

            if (dto.Amount < 100000) return new GenericResponse(UtilitiesStatusCodes.NotEnoughMoney);
            if (dto.Amount > 5000000) return new GenericResponse(UtilitiesStatusCodes.MoreThanAllowedMoney);
            if (sheba is null) return new GenericResponse(UtilitiesStatusCodes.BadRequest);

            var withdraw = new WithdrawEntity
            {
                Amount = dto.Amount,
                ApplicantUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ShebaNumber = dto.ShebaNumber,
                WithdrawState = WithdrawState.Requested
            };

            await _dbContext.Set<WithdrawEntity>().AddAsync(withdraw);
            await _dbContext.SaveChangesAsync();

            return new GenericResponse();
        }
    }
}
