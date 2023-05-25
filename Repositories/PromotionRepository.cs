using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Repositories
{
    public interface IPromotionRepository
    {
        Task<GenericResponse> CreatePromotion(CreateUpdatePromotionDto dto);
        Task<GenericResponse> UserSeened(Guid id);
        Task<GenericResponse<PromotionDetail?>> ReadPromotion(Guid id);
    }

    public class PromotionRepository : IPromotionRepository
    {
        private readonly DbContext _dbContext;
        private readonly string? _userId;

        public PromotionRepository(
            DbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
        }
        public async Task<GenericResponse> CreatePromotion(CreateUpdatePromotionDto dto)
        {
            var promotion = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.ProductId == dto.ProductId || f.GroupChatId == dto.GroupChatId);
            if (promotion is not null)
                return new GenericResponse(UtilitiesStatusCodes.BadRequest);

            promotion = new()
            {
                CreatedAt = DateTime.UtcNow,
                ProductId = dto.ProductId,
                UserId = _userId,
                DisplayType = dto.DisplayType,
                Gender = dto.Gender is not null ? string.Join(",", dto.Gender) : "",
                States = dto.States is not null ? string.Join(",", dto.States) : "",
                Skills = dto.Gender is not null ? string.Join(",", dto.Skills) : "",
                AgeCategories = dto.AgeCategories is not null ? string.Join(",", dto.AgeCategories) : "",
            };
            await _dbContext.Set<PromotionEntity>().AddAsync(promotion);

            var product = await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(f => f.Id == dto.ProductId);
            if (product is not null)
            {
                product.ProductJsonDetail.IsBoosted = true;
                _dbContext.Update(product);
            }

            var groupChat = await _dbContext.Set<GroupChatEntity>().FirstOrDefaultAsync(f => f.Id == dto.GroupChatId);
            if (groupChat is not null)
            {
                groupChat.GroupChatJsonDetail.IsBoosted = true;
                _dbContext.Update(groupChat);
            }
            await _dbContext.SaveChangesAsync();
            return new GenericResponse();
        }

        public async Task<GenericResponse<PromotionDetail?>> ReadPromotion(Guid id)
        {
            var promotion = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.ProductId == id || f.GroupChatId == id);
            if (promotion is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.NotFound);

            TimeSpan timeDifference = DateTime.UtcNow - promotion.CreatedAt!.Value;
            double hoursPassed = timeDifference.TotalHours;

            var usersId = promotion?.Users.IsNotNullOrEmpty() ?? false ? promotion.Users.Split(",") : null;
            if (usersId is null) return new GenericResponse<PromotionDetail?>(null, UtilitiesStatusCodes.BadRequest);
            List<UserEntity> users = new();
            foreach (var item in usersId)
            {
                var user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == item);
                if (item is null || user is null) continue;
                users.Add(user);
            }

            double userPerHour = (users.Count() / hoursPassed) * 100;
            
            List<StatePerUser> statePerUsers = users
                .GroupBy(u => u.UserJsonDetail.State)
                .Select(g => new StatePerUser { State = g.Key, UserCount = g.Count() })
                .ToList();

            List<SkillPerUser> skillPerUsers = users
                .SelectMany(u => u.Categories)
                .GroupBy(c => new { c.UseCase, c.Title })
                .Select(g => new SkillPerUser { Skill = g?.Key.Title ?? "", UserCount = g?.Count() ?? 0 })
                .ToList();


            List<AgeCatgPerUser> ageCatgPerUsers = users
                .GroupBy(u => u.AgeCategory)
                .Select(g => new AgeCatgPerUser { AgeCategory = ((int)g.Key).ToString(), UserCount = g.Count() })
                .ToList();

            return new GenericResponse<PromotionDetail?>(new PromotionDetail
            {
                AgeCatgPerUsers = ageCatgPerUsers,
                SkillPerUsers = skillPerUsers,
                StatePerUsers = statePerUsers,
                TotalSeen = userPerHour
            });
        }

        public async Task<GenericResponse> UserSeened(Guid id)
        {
            var promotion = await _dbContext.Set<PromotionEntity>().FirstOrDefaultAsync(f => f.ProductId == id || f.GroupChatId == id);
            if (promotion is null)
                return new GenericResponse(UtilitiesStatusCodes.NotFound);
            promotion.Users = promotion.Users.AddCommaSeperatorUsers(_userId);
            _dbContext.Update(promotion);
            await _dbContext.SaveChangesAsync();
            return new GenericResponse();
        }
    }
}
