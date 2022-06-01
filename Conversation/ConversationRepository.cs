﻿namespace Utilities_aspnet.Conversation; 

public interface IConversationRepository {
    Task<GenericResponse<ConversationsDto?>> SendConversation(AddConversationDto model);
    Task<GenericResponse<IEnumerable<ConversationsDto>?>> GetConversationByUserId(string id);
    Task<GenericResponse<IEnumerable<ConversationsDto>?>> GetConversatios();
}

public class ConversationRepository : IConversationRepository {
    private readonly DbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConversationRepository(DbContext context, IHttpContextAccessor httpContextAccessor) {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenericResponse<ConversationsDto?>> SendConversation(AddConversationDto model) {
        UserEntity? user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == model.UserId);
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserEntity? myUser = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) return new GenericResponse<ConversationsDto?>(null, UtilitiesStatusCodes.BadRequest);
        ConversationEntity? conversation = new ConversationEntity() {
            CreatedAt = DateTime.Now,
            FromUserId = userId,
            ToUserId = model.UserId,
            MessageText = model.MessageText,
        };
        await _context.Set<ConversationEntity>().AddAsync(conversation);
        await _context.SaveChangesAsync();
        ConversationsDto? conversations = new ConversationsDto
        {
            Id = conversation.Id,
            DateTime = (DateTime)conversation.CreatedAt,
            MessageText = conversation.MessageText,
            FullName = user.FullName,
            ProfileImage = "",
            UserId = conversation.ToUserId,
            Send = true
        };
        return new GenericResponse<ConversationsDto?>(conversations);
    }

    public async Task<GenericResponse<IEnumerable<ConversationsDto>?>> GetConversationByUserId(string id) {
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserEntity? user = await _context.Set<UserEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return new GenericResponse<IEnumerable<ConversationsDto>?>(null, UtilitiesStatusCodes.BadRequest);
        List<ConversationEntity>? conversation = await _context.Set<ConversationEntity>()
            .Where(c => c.ToUserId == userId && c.FromUserId == id).ToListAsync();
        List<ConversationEntity>? conversationToUser = await _context.Set<ConversationEntity>()
            .Where(x => x.FromUserId == userId && x.ToUserId == id).ToListAsync();

        foreach (ConversationEntity? toUser in conversationToUser) conversation.Add(toUser);
        List<ConversationsDto>? conversations = conversation.Select(x => new ConversationsDto {
            Id = x.Id,
            DateTime = (DateTime) x.CreatedAt,
            MessageText = x.MessageText,
            FullName = user.FullName,
            ProfileImage = "",
            UserId = id,
            Send = x.ToUserId == id
        }).OrderByDescending(x => x.Id).ToList();

        if (conversations.Count < 1)
            return new GenericResponse<IEnumerable<ConversationsDto>?>(null, UtilitiesStatusCodes.NotFound);

        return new GenericResponse<IEnumerable<ConversationsDto>?>(conversations);
    }

    public async Task<GenericResponse<IEnumerable<ConversationsDto>?>> GetConversatios() {
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        List<string>? ToUserId = await _context.Set<ConversationEntity>().Where(x => x.FromUserId == userId)
            .Select(x => x.ToUserId).ToListAsync();
        List<string>? FromUserId = await _context.Set<ConversationEntity>().Where(x => x.ToUserId == userId)
            .Select(x => x.FromUserId).ToListAsync();
        foreach (string? fromUser in FromUserId) ToUserId.Add(fromUser);
        List<ConversationsDto>? conversations = new();
        IEnumerable<string>? userIds = ToUserId.Distinct();

        foreach (string? item in userIds) {
            var user = await _context.Set<UserEntity>().Include(x => x.Media).Select(x => new {x.Id, x.FullName, x.Media})
                .FirstOrDefaultAsync(x => x.Id == item);
            ConversationEntity? conversation = await _context.Set<ConversationEntity>()
                .Where(c => c.FromUserId == item && c.ToUserId == userId || c.FromUserId == userId && c.ToUserId == item)
                .OrderByDescending(c => c.CreatedAt).Take(1).FirstOrDefaultAsync();
            conversations.Add(new ConversationsDto {
                Id = conversation.Id,
                DateTime = (DateTime) conversation.CreatedAt,
                MessageText = conversation.MessageText,
                FullName = user.FullName,
                ProfileImage = "",
                UserId = conversation.ToUserId == item ? conversation.ToUserId : conversation.FromUserId,
                Send = conversation.ToUserId == item
            });
        }

        if (conversations.Count < 1)
            return new GenericResponse<IEnumerable<ConversationsDto>?>(null, UtilitiesStatusCodes.NotFound);
        return new GenericResponse<IEnumerable<ConversationsDto>?>(conversations.OrderByDescending(x => x.DateTime));
    }
}