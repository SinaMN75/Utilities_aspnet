namespace Utilities_aspnet.Repositories;

public interface IChatRepository
{
    Task<GenericResponse<ChatReadDto?>> Create(ChatCreateUpdateDto model);
    Task<GenericResponse<IEnumerable<ChatReadDto>?>> Read();
    Task<GenericResponse<IEnumerable<ChatReadDto>?>> ReadByUserId(string id);
    Task<GenericResponse<IEnumerable<ChatReadDto>>> FilterByUserId(ChatFilterDto dto);
    Task<GenericResponse<ChatEntity?>> Update(ChatCreateUpdateDto dto);
    Task<GenericResponse> Delete(Guid id);

    Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto);
    Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto);
    Task<GenericResponse> DeleteGroupChat(Guid id);
    Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
    Task<GenericResponse<IQueryable<GroupChatEntity>?>> ReadMyGroupChats();
    Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
    Task<GenericResponse> DeleteGroupChatMessage(Guid id);
    GenericResponse<IQueryable<GroupChatEntity>> FilterGroupChats(GroupChatFilterDto dto);
    Task<GenericResponse<GroupChatEntity>> ReadGroupChatById(Guid id);
    GenericResponse<IQueryable<GroupChatMessageEntity>?> ReadGroupChatMessages(Guid id, int pageSize, int pageNumber);
    Task<GenericResponse> AddReactionToMessage(Reaction reaction, Guid messageId);
    Task<GenericResponse> SeenGroupChatMessage(Guid messageId);
    Task<GenericResponse> ExitFromGroup(Guid id);
    Task<GenericResponse> Mute(Guid id);
}

public class ChatRepository : IChatRepository
{
    private readonly DbContext _dbContext;
    private readonly string? _userId;

    public ChatRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;
    }

    public async Task<GenericResponse<ChatReadDto?>> Create(ChatCreateUpdateDto model)
    {
        UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == model.UserId);
        if (user == null) return new GenericResponse<ChatReadDto?>(null, UtilitiesStatusCodes.BadRequest);

        List<UserEntity?> users = new();
        foreach (string id in model.Users ?? new List<string>()) users.Add(await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id));

        List<ProductEntity?> products = new();
        foreach (Guid id in model.Products ?? new List<Guid>()) products.Add(await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id));
        ChatEntity conversation = new()
        {
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            FromUserId = _userId!,
            ToUserId = model.UserId,
            MessageText = model.MessageText,
            ReadMessage = false,
            Products = products,
            ParentId = model.ParentId
        };
        await _dbContext.Set<ChatEntity>().AddAsync(conversation);
        await _dbContext.SaveChangesAsync();
        ChatReadDto conversations = new()
        {
            Id = conversation.Id,
            DateTime = conversation.CreatedAt,
            MessageText = conversation.MessageText,
            User = user,
            Media = conversation.Media,
            UserId = conversation.ToUserId,
            ParentId = model.ParentId,
            Products = conversation.Products
        };

        return new GenericResponse<ChatReadDto?>(conversations);
    }

    public async Task<GenericResponse<IEnumerable<ChatReadDto>>> FilterByUserId(ChatFilterDto dto)
    {
        string? userId = _userId;
        UserEntity? user = await _dbContext.Set<UserEntity>()
            .Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == dto.UserId);
        if (user == null) return new GenericResponse<IEnumerable<ChatReadDto>>(null, UtilitiesStatusCodes.BadRequest);
        List<ChatEntity> conversation = await _dbContext.Set<ChatEntity>()
            .Where(c => c.ToUserId == userId && c.FromUserId == userId)
            .Include(x => x.Media).OrderByDescending(x => x.CreatedAt).ToListAsync();

        foreach (ChatEntity? item in conversation.Where(item => item.ReadMessage == false))
        {
            item.ReadMessage = true;
            await _dbContext.SaveChangesAsync();
        }

        IEnumerable<ChatEntity> conversationToUser = await _dbContext.Set<ChatEntity>()
            .Where(x => x.FromUserId == userId && x.ToUserId == dto.UserId)
            .Include(x => x.Media).ToListAsync();

        conversation.AddRange(conversationToUser);
        List<ChatReadDto> conversations = conversation.Select(x => new ChatReadDto
        {
            Id = x.Id,
            DateTime = x.CreatedAt,
            MessageText = x.MessageText,
            User = user,
            UserId = dto.UserId,
            Media = x.Media,
        }).OrderByDescending(x => x.DateTime).ToList();

        int totalCount = conversation.Count;
        conversations = conversations.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).ToList();

        return new GenericResponse<IEnumerable<ChatReadDto>>(conversations)
        {
            TotalCount = totalCount,
            PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
            PageSize = dto?.PageSize
        };
    }

    public async Task<GenericResponse<ChatEntity?>> Update(ChatCreateUpdateDto dto)
    {
        ChatEntity? e = await _dbContext.Set<ChatEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);
        if (e == null) return new GenericResponse<ChatEntity?>(null, UtilitiesStatusCodes.NotFound);
        e.MessageText = dto.MessageText;
        _dbContext.Update(e);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<ChatEntity?>(e);
    }

    public async Task<GenericResponse> Delete(Guid id)
    {
        ChatEntity? e = await _dbContext.Set<ChatEntity>().FirstOrDefaultAsync(x => x.Id == id);
        e.DeletedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public async Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto)
    {
        var overUsedCheck = Utils.IsUserOverused(_dbContext, _userId ?? string.Empty, nameof(CreateGroupChat) , dto.Type , null);
        if (overUsedCheck.Item1)
            return new GenericResponse<GroupChatEntity?>(null, overUsedCheck.Item2);

        if (dto.Type == ChatType.Private && dto.UserIds!.Count() == 2)
        {
            string firstUserId = dto.UserIds!.ToList()[0];
            string secondUserId = dto.UserIds!.ToList()[1];

            var blockedState = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == firstUserId), _dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == secondUserId));
            if (blockedState.Item1)
                return new GenericResponse<GroupChatEntity?>(null, blockedState.Item2);

            GroupChatEntity? e = await _dbContext.Set<GroupChatEntity>().AsNoTracking()
                .Include(x => x.Users)!.ThenInclude(x => x.Media)
                .Include(x => x.Products)!.ThenInclude(x => x.Media)
                .Include(x => x.Products)!.ThenInclude(x => x.Categories)
                .Include(x => x.Media)
                .FirstOrDefaultAsync(x => x.Users.Count() == 2 &&
                                          x.Users.Any(x => x.Id == firstUserId) &&
                                          x.Users.Any(x => x.Id == secondUserId) &&
                                          x.Type == ChatType.Private &&
                                          x.DeletedAt == null);
            if (e == null) return await CreateGroupChatLogic(dto);
            return new GenericResponse<GroupChatEntity?>(e);
        }
        return await CreateGroupChatLogic(dto);
    }

    public async Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto)
    {
        GroupChatEntity? e = await _dbContext.Set<GroupChatEntity>()
            .Include(x => x.Users)
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (dto.UserIds.IsNotNull())
        {
            List<UserEntity> users = new();
            foreach (string id in dto.UserIds!)
            {
                var isBlocked = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == id), _dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == _userId));
                if (!isBlocked.Item1) users.Add((await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
            }
            e.Users = users;
        }

        if (dto.Products.IsNotNull())
        {
            List<ProductEntity> products = new();
            foreach (Guid id in dto.Products!) products.Add((await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
            e.Products = products;
        }

        if (dto.Categories.IsNotNull())
        {
            List<CategoryEntity> list = new();
            foreach (Guid id in dto.Categories!) list.Add((await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
            e.Categories = list;
        }

        e.Department = dto.Department ?? e.Department;
        e.Priority = dto.Priority ?? e.Priority;
        e.Title = dto.Title ?? e.Title;
        e.Type = dto.Type ?? e.Type;
        e.Value = dto.Value ?? e.Value;
        e.UpdatedAt = DateTime.Now;
        e.Description = dto.Description ?? e.Description;
        e.ChatStatus = dto.ChatStatus ?? e.ChatStatus;

        EntityEntry<GroupChatEntity> entity = _dbContext.Set<GroupChatEntity>().Update(e);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<GroupChatEntity?>(entity.Entity);
    }

    public async Task<GenericResponse> DeleteGroupChat(Guid id)
    {
        GroupChatEntity? e = await _dbContext.Set<GroupChatEntity>().FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
        e.DeletedAt = DateTime.Now;
        _dbContext.Update(e);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public async Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
    {
        List<ProductEntity?> products = new();
        foreach (Guid id in dto.Products ?? new List<Guid>()) products.Add(await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id));
        GroupChatMessageEntity entity = new()
        {
            Message = dto.Message,
            Type = dto.Type,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UseCase = dto.UseCase,
            GroupChatId = dto.GroupChatId,
            ParentId = dto.ParentId,
            UserId = _userId,
            Products = products,
            ForwardedMessageId = dto.ForwardedMessageId,
        };

        EntityEntry<GroupChatMessageEntity> e = await _dbContext.Set<GroupChatMessageEntity>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<GroupChatMessageEntity?>(e.Entity);
    }

    public async Task<GenericResponse<IQueryable<GroupChatEntity>?>> ReadMyGroupChats()
    {
        List<GroupChatEntity> e = await _dbContext.Set<GroupChatEntity>().AsNoTracking().Where(x => x.DeletedAt == null)
            .Where(x => x.DeletedAt == null && x.Users!.Any(y => y.Id == _userId))
            .Include(x => x.Users)!.ThenInclude(x => x.Media)
            .Include(x => x.GroupChatMessage!.OrderByDescending(y => y.CreatedAt).Take(1)).ThenInclude(x => x.Media)
            .ToListAsync();

        foreach (GroupChatEntity groupChatEntity in e.Where(groupChatEntity => groupChatEntity.Type == ChatType.Private))
        {
            if (groupChatEntity.Users!.First().Id == _userId)
            {
                UserEntity u = (await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.Last().Id))!;
                groupChatEntity.Title = u.AppUserName;
            }
            else
            {
                UserEntity u = (await _dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.First().Id))!;
                groupChatEntity.Title = u.AppUserName;
            }
        }

        var myGroupChats = new List<GroupChatEntity>();

        foreach (var item in e)
        {
            int countOfMessage = 0;
            var seenUsers = _dbContext.Set<SeenUsers>().FirstOrDefault(w => w.Fk_GroupChat == item.Id && w.Fk_UserId == _userId);
            var groupchatMessages = _dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == item.Id);
            if (seenUsers is null)
                countOfMessage = groupchatMessages.Count();
            else
            {
                var LastSeenMessage = await groupchatMessages.Where(w => w.Id == seenUsers.Fk_GroupChatMessage).FirstOrDefaultAsync();
                countOfMessage = await groupchatMessages.Where(w => w.CreatedAt > LastSeenMessage.CreatedAt).CountAsync();
            }
            item.CountOfUnreadMessages = countOfMessage;
            myGroupChats.Add(item);
        }

        return new GenericResponse<IQueryable<GroupChatEntity>?>(myGroupChats.AsQueryable());
    }

    public async Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto)
    {
        GroupChatMessageEntity? e = await _dbContext.Set<GroupChatMessageEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        e.Message = dto.Message ?? e.Message;
        e.Type = dto.Type ?? e.Type;
        e.UpdatedAt = DateTime.Now;
        e.UseCase = dto.UseCase ?? e.UseCase;

        EntityEntry<GroupChatMessageEntity> entity = _dbContext.Set<GroupChatMessageEntity>().Update(e);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<GroupChatMessageEntity?>(entity.Entity);
    }

    public async Task<GenericResponse> DeleteGroupChatMessage(Guid id)
    {
        GroupChatMessageEntity? e = await _dbContext.Set<GroupChatMessageEntity>().FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return new GenericResponse(UtilitiesStatusCodes.NotFound);
        e.DeletedAt = DateTime.Now;
        _dbContext.Update(e);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public GenericResponse<IQueryable<GroupChatEntity>> FilterGroupChats(GroupChatFilterDto dto)
    {
        IQueryable<GroupChatEntity> q = _dbContext.Set<GroupChatEntity>()
            .Where(x => x.Users.Any(y => y.Id == _userId));

        if (dto.UsersIds.IsNotNullOrEmpty()) q = q.Where(x => x.Users.Any(x => x.Id == dto.UsersIds.FirstOrDefault()));
        if (dto.ProductsIds.IsNotNullOrEmpty()) q = q.Where(x => x.Products.Any(x => x.Id == dto.ProductsIds.FirstOrDefault()));
        if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == dto.Title);
        if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => x.Description == dto.Description);
        if (dto.Type.HasValue) q = q.Where(x => x.Type == dto.Type);
        if (dto.Value.IsNotNullOrEmpty()) q = q.Where(x => x.Value == dto.Value);
        if (dto.Department.IsNotNullOrEmpty()) q = q.Where(x => x.Department == dto.Department);
        if (dto.ChatStatus.HasValue) q = q.Where(x => x.ChatStatus == dto.ChatStatus);
        if (dto.Priority.HasValue) q = q.Where(x => x.Priority == dto.Priority);

        if (dto.ShowProducts.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Media);
        if (dto.ShowUsers.IsTrue()) q = q.Include(x => x.Users)!.ThenInclude(x => x.Media);
        if (dto.ShowCategories.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Categories);

        if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
        if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
        if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
        if (dto.OrderByCreaedDateDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

        int totalCount = q.Count();
        q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

        return new GenericResponse<IQueryable<GroupChatEntity>>(q.AsNoTracking())
        {
            TotalCount = totalCount,
            PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
            PageSize = dto.PageSize
        };
    }

    public async Task<GenericResponse<GroupChatEntity>> ReadGroupChatById(Guid id)
    {
        GroupChatEntity? e = await _dbContext.Set<GroupChatEntity>()
            .Include(x => x.Users)!.ThenInclude(x => x.Media)
            .Include(x => x.Products)!.ThenInclude(x => x.Media)
            .Include(x => x.Products)!.ThenInclude(x => x.Categories)
            .Include(x => x.GroupChatMessage)
            .Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);

        if (e != null)
        {
            int countOfMessage = 0;
            var seenUsers = _dbContext.Set<SeenUsers>().FirstOrDefault(w => w.Fk_GroupChat == e.Id && w.Fk_UserId == _userId);
            var groupchatMessages = _dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == e.Id);
            if (seenUsers is null)
                countOfMessage = groupchatMessages.Count();
            else
            {
                var LastSeenMessage = groupchatMessages.Where(w => w.Id == seenUsers.Fk_GroupChatMessage).FirstOrDefault();
                countOfMessage = groupchatMessages.Where(w => w.CreatedAt > LastSeenMessage.CreatedAt).Count();
            }
            e.CountOfUnreadMessages = countOfMessage;
        }

        return new GenericResponse<GroupChatEntity>(e);
    }

    public GenericResponse<IQueryable<GroupChatMessageEntity>?> ReadGroupChatMessages(Guid id, int pageSize, int pageNumber)
    {
        IQueryable<GroupChatMessageEntity> q = _dbContext.Set<GroupChatMessageEntity>()
            .Where(x => x.GroupChatId == id && x.DeletedAt == null)
            .Include(x => x.Media)
            .Include(x => x.Products)!.ThenInclude(x => x.Media)
            .Include(x => x.Products)!.ThenInclude(x => x.User).ThenInclude(x => x.Media)
            .Include(x => x.Parent)!.ThenInclude(x => x.Media)
            .Include(x => x.Parent)!.ThenInclude(x => x.Products).ThenInclude(x => x.Media)
            .Include(x => x.Parent).ThenInclude(x => x.User).ThenInclude(x => x.Media)
            .Include(x => x.User)!.ThenInclude(x => x.Media)
            .Include(x => x.ForwardedMessage)!.ThenInclude(x => x.Media)
            .Include(x => x.ForwardedMessage).ThenInclude(x => x.Products)!.ThenInclude(x => x.Media)
            .Include(x => x.ForwardedMessage)!.ThenInclude(x => x.Parent)!.ThenInclude(x => x.Media)
            .Include(x => x.ForwardedMessage).ThenInclude(x => x.User).ThenInclude(x => x.Media)
            .OrderBy(o => o.CreatedAt)
            .AsNoTracking();
        
        int totalCount = q.Count();
        q = q.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var tempGroupChatsMessage = q.ToList();
        var messageSeen = _dbContext.Set<SeenUsers>().Where(w => w.Fk_GroupChat == id);
        foreach (var item in tempGroupChatsMessage)
        {
            var usersMessage = new List<UserEntity>();
            foreach (var seenTbl in messageSeen)
            {
                var lastMessageThatUserSeened = q.FirstOrDefault(f => f.Id == seenTbl.Fk_GroupChatMessage);
                var user = _dbContext.Set<UserEntity>().Where(w => w.Id == seenTbl.Fk_UserId).Include(x => x.Media).FirstOrDefault();
                if (user is not null && (lastMessageThatUserSeened?.CreatedAt > item.CreatedAt || lastMessageThatUserSeened?.Id == item.Id))
                    usersMessage.Add(user);
            }
            item.MessageSeenBy = usersMessage;
        }

        return new GenericResponse<IQueryable<GroupChatMessageEntity>?>(tempGroupChatsMessage.AsQueryable()) {
            TotalCount = totalCount,
            PageCount = totalCount % pageSize == 0 ? totalCount / pageSize : totalCount / pageSize + 1,
            PageSize = pageSize
        };
    }

    public async Task<GenericResponse<IEnumerable<ChatReadDto>?>> ReadByUserId(string id)
    {
        string? userId = _userId;
        UserEntity? user = await _dbContext.Set<UserEntity>()
            .Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return new GenericResponse<IEnumerable<ChatReadDto>?>(null, UtilitiesStatusCodes.BadRequest);
        List<ChatEntity> conversation = await _dbContext.Set<ChatEntity>()
            .Where(c => c.ToUserId == userId && c.FromUserId == id)
            .Include(x => x.Media)
            .Include(x => x.Parent)
            .Include(x => x.Products)!.ThenInclude(x => x.Media)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

        foreach (ChatEntity? item in conversation.Where(item => item.ReadMessage == false))
        {
            item.ReadMessage = true;
            await _dbContext.SaveChangesAsync();
        }

        IEnumerable<ChatEntity> conversationToUser = await _dbContext.Set<ChatEntity>()
            .Where(x => x.FromUserId == userId && x.ToUserId == id)
            .Include(x => x.Media)
            .Include(x => x.Parent).ThenInclude(x => x.Media)
            .Include(x => x.Products).ThenInclude(x => x.Media)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();

        conversation.AddRange(conversationToUser);
        List<ChatReadDto> conversations = conversation.Select(x => new ChatReadDto
        {
            Id = x.Id,
            DateTime = x.CreatedAt,
            MessageText = x.MessageText,
            Send = userId == x.FromUserId,
            User = user,
            UserId = x.FromUserId == userId ? x.ToUserId : x.FromUserId,
            Media = x.Media,
            Products = x.Products,
            Parent = new ChatReadDto
            {
                Id = x.Parent?.Id,
                DateTime = x.Parent?.CreatedAt,
                MessageText = x.Parent?.MessageText,
                Media = x.Parent?.Media
                //UserId = conversation.Parent.ToUserId == conversation.Parent.User //Todo this and other one when need
            }
        }).OrderByDescending(x => x.DateTime).ToList();

        return new GenericResponse<IEnumerable<ChatReadDto>?>(conversations);
    }

    public async Task<GenericResponse<IEnumerable<ChatReadDto>?>> Read()
    {
        string userId = _userId!;
        List<string> toUserId = await _dbContext.Set<ChatEntity>()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.FromUserId == userId)
            .Include(x => x.Products)!.ThenInclude(x => x.Media)
            .Include(x => x.Parent)
            .Include(x => x.Media).Select(x => x.ToUserId).ToListAsync();
        List<string> fromUserId = await _dbContext.Set<ChatEntity>()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.ToUserId == userId)
            .Include(x => x.Parent)
            .Include(x => x.Products)!.ThenInclude(x => x.Media)
            .Include(x => x.Media).Select(x => x.FromUserId).ToListAsync();
        toUserId.AddRange(fromUserId);
        List<ChatReadDto> conversations = new();
        IEnumerable<string> userIds = toUserId.Distinct();

        foreach (string? item in userIds)
        {
            UserEntity? user = await _dbContext.Set<UserEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == item);
            ChatEntity? conversation = await _dbContext.Set<ChatEntity>()
                .Where(c => c.FromUserId == item && c.ToUserId == userId || c.FromUserId == userId && c.ToUserId == item).OrderByDescending(c => c.CreatedAt)
                .Include(y => y.Media)
                .Include(x => x.Parent)!.ThenInclude(x => x.Media)
                .Include(x => x.Products)!.ThenInclude(x => x.Media)
                .Take(1).FirstOrDefaultAsync();
            int? countUnReadMessage = _dbContext.Set<ChatEntity>().Where(c => c.FromUserId == item && c.ToUserId == userId).Count(x => x.ReadMessage == false);

            conversations.Add(new ChatReadDto
            {
                Id = conversation!.Id,
                DateTime = conversation.CreatedAt,
                MessageText = conversation.MessageText,
                Send = conversation.ToUserId == item,
                UserId = conversation.ToUserId == item ? conversation.ToUserId : conversation.FromUserId,
                UnReadMessages = countUnReadMessage,
                User = user,
                Media = conversation.Media,
                Products = conversation.Products,
                Parent = new ChatReadDto
                {
                    Id = conversation.Parent?.Id,
                    DateTime = conversation.Parent?.CreatedAt,
                    MessageText = conversation.Parent?.MessageText,
                    Media = conversation.Parent?.Media
                    //UserId = conversation.Parent.ToUserId == conversation.Parent.User //Todo this and other one when need
                }
            });
        }

        return new GenericResponse<IEnumerable<ChatReadDto>?>(conversations.OrderByDescending(x => x.DateTime));
    }

    private async Task<GenericResponse<GroupChatEntity?>> CreateGroupChatLogic(GroupChatCreateUpdateDto dto) {
        if (dto.UserIds.Count() > 2 && dto.Type == ChatType.Private)
            return new GenericResponse<GroupChatEntity?>(null, UtilitiesStatusCodes.MoreThan2UserIsInPrivateChat);
        List<UserEntity> users = new();
        if (dto.UserIds.IsNotNull())
            foreach (string id in dto.UserIds!)
            {
                var isBlocked = Utils.IsBlockedUser(_dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == id), _dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == _userId));
                if (!isBlocked.Item1) users.Add((await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
            }

        List<ProductEntity> products = new();
        if (dto.Products.IsNotNull())
            foreach (Guid id in dto.Products!)
                products.Add((await _dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);

        GroupChatEntity entity = new()
        {
            Department = dto.Department,
            Priority = dto.Priority,
            Title = dto.Title,
            Type = dto.Type,
            Value = dto.Value,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ChatStatus = dto.ChatStatus,
            Description = dto.Description,
            CreatorUserId = _userId!,
            Users = users,
            Products = products
        };
        if (dto.Id != null) entity.Id = (Guid)dto.Id;

        if (dto.Categories.IsNotNull())
        {
            List<CategoryEntity> listCategory = new();
            foreach (Guid item in dto.Categories!)
            {
                CategoryEntity? ce = await _dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
                if (ce != null) listCategory.Add(ce);
            }
            entity.Categories = listCategory;
        }

        EntityEntry<GroupChatEntity> e = await _dbContext.Set<GroupChatEntity>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return new GenericResponse<GroupChatEntity?>(e.Entity);
    }

    public async Task<GenericResponse> AddReactionToMessage(Reaction reaction, Guid messageId)
    {
        string userId = _userId!;
        UserEntity? user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound, "User Donest Logged In");

        ChatEntity? chat = await _dbContext.Set<ChatEntity>().Where(w => w.Id == messageId).FirstOrDefaultAsync();
        if (chat is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Chat Not Found");

        ReactionEntity? oldReaction = await _dbContext.Set<ReactionEntity>().Where(w => w.UserId == userId && w.ChatsId == chat.Id).FirstOrDefaultAsync();
        if (oldReaction is null)
        {
            ReactionEntity react = new ReactionEntity
            {
                ChatsId = chat.Id,
                Reaction = reaction,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };
            await _dbContext.Set<ReactionEntity>().AddAsync(react);
        }
        else if (oldReaction.Reaction != reaction)
        {
            oldReaction.Reaction = reaction;
            _dbContext.Set<ReactionEntity>().Update(oldReaction);
        }
        else
        {
            _dbContext.Set<ReactionEntity>().Remove(oldReaction);
        }
        await _dbContext.SaveChangesAsync();
        return new GenericResponse(UtilitiesStatusCodes.Success, "Ok");
    }

    public async Task<GenericResponse> SeenGroupChatMessage(Guid messageId)
    {
        var groupMessageChat = await _dbContext.Set<GroupChatMessageEntity>().Where(e => e.Id == messageId).FirstOrDefaultAsync();
        if (groupMessageChat is null)
            return new GenericResponse(UtilitiesStatusCodes.NotFound, "GroupChatMessage Not Found!");

        var groupChat = await _dbContext.Set<GroupChatEntity>().Where(w => w.Id == groupMessageChat.GroupChatId).FirstOrDefaultAsync();
        if (groupChat is null)
            return new GenericResponse(UtilitiesStatusCodes.NotFound, "GroupChate Not Found!");

        var seenUsers = await _dbContext.Set<SeenUsers>().Where(w => w.Fk_GroupChat == groupChat.Id && w.Fk_UserId == _userId).FirstOrDefaultAsync();

        if (seenUsers is null)
            await _dbContext.Set<SeenUsers>().AddAsync(new SeenUsers
            {
                CreatedAt = DateTime.Now,
                Fk_GroupChat = groupChat.Id,
                Fk_UserId = _userId,
                Fk_GroupChatMessage = messageId
            });
        else
        {
            var lastMessageSeen = await _dbContext.Set<GroupChatMessageEntity>().FirstOrDefaultAsync(f => f.Id == seenUsers.Fk_GroupChatMessage);
            if (lastMessageSeen?.CreatedAt > groupMessageChat.CreatedAt || seenUsers.Fk_GroupChatMessage == messageId) return new GenericResponse();

            seenUsers.Fk_GroupChatMessage = messageId;
            _dbContext.Update(seenUsers);
        }

        await _dbContext.SaveChangesAsync();
        return new GenericResponse();
    }

    public async Task<GenericResponse> ExitFromGroup(Guid id)
    {
        var groupChat = await _dbContext.Set<GroupChatEntity>().Include(i => i.Users).FirstOrDefaultAsync(f => f.Id == id);
        if (groupChat is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Group Chat not Founded");

        if (!groupChat.Users.Any(a => a.Id == _userId)) return new GenericResponse(UtilitiesStatusCodes.UserNotFound, "User Not Founded in GroupChat");

        var user = await _dbContext.Set<UserEntity>().Where(w => w.Id == _userId).FirstOrDefaultAsync();

        var tempGroup = groupChat;
        var tempUsers = tempGroup.Users.ToList();
        var result = tempUsers.Remove(user);

        if (result)
        {
            groupChat.Users = tempUsers;

            _dbContext.Update(groupChat);
            _dbContext.SaveChanges();
            return new GenericResponse(UtilitiesStatusCodes.Success, groupChat.Users.ToString());
        }
        else return new GenericResponse(UtilitiesStatusCodes.BadRequest);
    }

    public async Task<GenericResponse> Mute(Guid id)
    {
        var user = await _dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
        if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

        if (user.MutedChats.IsNullOrEmpty()) user.MutedChats += id.ToString();
        else user.MutedChats = user.MutedChats + "," + id.ToString();

        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync();

        return new GenericResponse();
    }
}