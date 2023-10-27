namespace Utilities_aspnet.Repositories;

public interface IChatRepository {
	Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto);
	Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto);
	Task<GenericResponse> DeleteGroupChat(Guid id);
	Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
	Task<GenericResponse<IQueryable<GroupChatEntity>?>> ReadMyGroupChats();
	Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
	Task<GenericResponse> DeleteGroupChatMessage(Guid id);
	GenericResponse<IQueryable<GroupChatEntity>> FilterGroupChats(GroupChatFilterDto dto);
	GenericResponse<IQueryable<GroupChatEntity>> FilterAllGroupChats(GroupChatFilterDto dto);
	Task<GenericResponse<GroupChatEntity>> ReadGroupChatById(Guid id);
	GenericResponse<IQueryable<GroupChatMessageEntity>?> ReadGroupChatMessages(Guid id, int pageSize, int pageNumber);
	Task<GenericResponse> SeenGroupChatMessage(Guid messageId);
	Task<GenericResponse> ExitFromGroup(Guid id);
	Task<GenericResponse> Mute(Guid id);
}

public class ChatRepository(DbContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration config, IPromotionRepository promotionRepository)
	: IChatRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto) {
		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);
		Tuple<bool, UtilitiesStatusCodes> overUsedCheck =
			Utils.IsUserOverused(dbContext, _userId ?? string.Empty, CallerType.CreateGroupChat, dto.Type, null, appSettings.UsageRules);
		if (overUsedCheck.Item1)
			return new GenericResponse<GroupChatEntity?>(null, overUsedCheck.Item2);

		if (dto.Type == ChatType.Private && dto.UserIds!.Count() == 2) {
			string firstUserId = dto.UserIds!.ToList()[0];
			string secondUserId = dto.UserIds!.ToList()[1];

			Tuple<bool, UtilitiesStatusCodes> blockedState = Utils.IsBlockedUser(dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == firstUserId),
				dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == secondUserId));
			if (blockedState.Item1)
				return new GenericResponse<GroupChatEntity?>(null, blockedState.Item2);

			GroupChatEntity? e = await dbContext.Set<GroupChatEntity>().AsNoTracking()
				.Include(x => x.Users)!.ThenInclude(x => x.Media)
				.Include(x => x.Products)!.ThenInclude(x => x.Media)
				.Include(x => x.Products)!.ThenInclude(x => x.Categories)
				.Include(x => x.Media)
				.FirstOrDefaultAsync(x => x.Users!.Count() == 2 &&
				                          x.Users!.Any(y => y.Id == firstUserId) &&
				                          x.Users!.Any(y => y.Id == secondUserId) &&
				                          x.Type == ChatType.Private
				);

			if (e == null) return await CreateGroupChatLogic(dto);
			return new GenericResponse<GroupChatEntity?>(e);
		}

		return await CreateGroupChatLogic(dto);
	}

	public async Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto) {
		GroupChatEntity e = (await dbContext.Set<GroupChatEntity>()
			.Include(x => x.Users)
			.Include(x => x.Products)
			.FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		if (dto.UserIds.IsNotNull()) {
			List<UserEntity> users = new();
			foreach (string id in dto.UserIds!) {
				Tuple<bool, UtilitiesStatusCodes> isBlocked = Utils.IsBlockedUser(dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == id),
					dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == _userId));
				if (!isBlocked.Item1) users.Add((await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			}

			e.Users = users;
		}

		if (dto.Products.IsNotNull()) {
			List<ProductEntity> products = new();
			foreach (Guid id in dto.Products!) products.Add((await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			e.Products = products;
		}

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> list = new();
			foreach (Guid id in dto.Categories!) list.Add((await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			e.Categories = list;
		}

		e.Title = dto.Title ?? e.Title;
		e.Type = dto.Type ?? e.Type;
		e.UpdatedAt = DateTime.Now;

		e.JsonDetail = new GroupChatJsonDetail {
			Department = dto.Department ?? e.JsonDetail.Department,
			Priority = dto.Priority ?? e.JsonDetail.Priority,
			Value = dto.Value ?? e.JsonDetail.Value,
			Description = dto.Description ?? e.JsonDetail.Description,
			ChatStatus = dto.ChatStatus ?? e.JsonDetail.ChatStatus
		};

		EntityEntry<GroupChatEntity> entity = dbContext.Set<GroupChatEntity>().Update(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatEntity?>(entity.Entity);
	}

	public async Task<GenericResponse> DeleteGroupChat(Guid id) {
		IQueryable<MediaEntity>? medias = dbContext.Set<MediaEntity>().Where(w => w.GroupChatId == id);
		if (medias is not null && medias.Any()) dbContext.RemoveRange(medias);
		await dbContext.SaveChangesAsync();
		await dbContext.Set<GroupChatEntity>().Include(w => w.GroupChatMessage).Where(x => x.Id == id).ExecuteDeleteAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		List<ProductEntity?> products = new();
		foreach (Guid id in dto.Products ?? new List<Guid>()) products.Add(await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id));

		GroupChatEntity? groupChat = await dbContext.Set<GroupChatEntity>().FirstOrDefaultAsync(f => f.Id == dto.GroupChatId);
		if (groupChat != null) {
			if (groupChat.Type == ChatType.Private) {
				if (groupChat.Users == null || !groupChat.Users.Any()) return new GenericResponse<GroupChatMessageEntity?>(null, UtilitiesStatusCodes.BadRequest);
				string? firstUserId = groupChat.Users.First().Id;
				string? secondUserId = groupChat.Users.Last().Id;
				Tuple<bool, UtilitiesStatusCodes> blockedState = Utils.IsBlockedUser(dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == firstUserId),
					dbContext.Set<UserEntity>().FirstOrDefault(w => w.Id == secondUserId));
				if (blockedState.Item1)
					return new GenericResponse<GroupChatMessageEntity?>(null, blockedState.Item2);
			}
		}

		GroupChatMessageEntity entity = new() {
			Message = dto.Message,
			Type = dto.Type,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			UseCase = dto.UseCase,
			GroupChatId = dto.GroupChatId,
			ParentId = dto.ParentId,
			UserId = _userId,
			Products = products,
			ForwardedMessageId = dto.ForwardedMessageId
		};

		EntityEntry<GroupChatMessageEntity> e = await dbContext.Set<GroupChatMessageEntity>().AddAsync(entity);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatMessageEntity?>(e.Entity);
	}

	public async Task<GenericResponse<IQueryable<GroupChatEntity>?>> ReadMyGroupChats() {
		List<GroupChatEntity> e = await dbContext.Set<GroupChatEntity>().AsNoTracking()
			.Where(x => x.Users!.Any(y => y.Id == _userId))
			.Include(x => x.Users)!.ThenInclude(x => x.Media)
			.Include(x => x.Media)
			.Include(x => x.GroupChatMessage!.OrderByDescending(y => y.CreatedAt).Take(1)).ThenInclude(x => x.Media)
			.ToListAsync();

		foreach (GroupChatEntity groupChatEntity in e.Where(groupChatEntity => groupChatEntity.Type == ChatType.Private))
			if (groupChatEntity.Users!.First().Id == _userId) {
				UserEntity u = (await dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.Last().Id))!;
				groupChatEntity.Title = u.AppUserName;
			}
			else {
				UserEntity u = (await dbContext.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.First().Id))!;
				groupChatEntity.Title = u.AppUserName;
			}

		List<GroupChatEntity> myGroupChats = new();

		foreach (GroupChatEntity? item in e) {
			int countOfMessage = 0;
			SeenUsers? seenUsers = dbContext.Set<SeenUsers>().FirstOrDefault(w => w.FkGroupChat == item.Id && w.FkUserId == _userId);
			IQueryable<GroupChatMessageEntity> groupchatMessages = dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == item.Id);
			if (seenUsers is null) {
				countOfMessage = groupchatMessages.Count();
			}
			else {
				GroupChatMessageEntity lastSeenMessage = (await groupchatMessages.Where(w => w.Id == seenUsers.FkGroupChatMessage.Value).FirstOrDefaultAsync())!;
				countOfMessage = await groupchatMessages.Where(w => w.CreatedAt > lastSeenMessage.CreatedAt).CountAsync();
			}

			item.CountOfUnreadMessages = countOfMessage;
			myGroupChats.Add(item);
		}

		return new GenericResponse<IQueryable<GroupChatEntity>?>(myGroupChats.AsQueryable());
	}

	public async Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		GroupChatMessageEntity e = (await dbContext.Set<GroupChatMessageEntity>()
			.FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		e.Message = dto.Message ?? e.Message;
		e.Type = dto.Type ?? e.Type;
		e.UpdatedAt = DateTime.Now;
		e.UseCase = dto.UseCase ?? e.UseCase;

		EntityEntry<GroupChatMessageEntity> entity = dbContext.Set<GroupChatMessageEntity>().Update(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatMessageEntity?>(entity.Entity);
	}

	public async Task<GenericResponse> DeleteGroupChatMessage(Guid id) {
		IQueryable<MediaEntity>? medias = dbContext.Set<MediaEntity>().Where(w => w.GroupChatMessageId == id);
		if (medias is not null && medias.Any()) dbContext.RemoveRange(medias);
		IQueryable<SeenUsers>? seenUsers = dbContext.Set<SeenUsers>().Where(w => w.FkGroupChatMessage == id);
		if (seenUsers is not null && seenUsers.Any()) dbContext.RemoveRange(seenUsers);
		await dbContext.Set<GroupChatMessageEntity>().Where(x => x.Id == id).ExecuteDeleteAsync();
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public GenericResponse<IQueryable<GroupChatEntity>> FilterGroupChats(GroupChatFilterDto dto) {
		IQueryable<GroupChatEntity> q = dbContext.Set<GroupChatEntity>()
			.Where(x => x.Users!.Any(y => y.Id == _userId));

		if (dto.UsersIds.IsNotNullOrEmpty()) q = q.Where(x => x.Users!.Any(y => y.Id == dto.UsersIds!.FirstOrDefault()));
		if (dto.ProductsIds.IsNotNullOrEmpty()) q = q.Where(x => x.Products!.Any(y => y.Id == dto.ProductsIds!.FirstOrDefault()));
		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == dto.Title);
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Description == dto.Description);
		if (dto.Type.HasValue) q = q.Where(x => x.Type == dto.Type);
		if (dto.Value.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Value == dto.Value);
		if (dto.Department.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Department == dto.Department);
		if (dto.ChatStatus.HasValue) q = q.Where(x => x.JsonDetail.ChatStatus == dto.ChatStatus);
		if (dto.Priority.HasValue) q = q.Where(x => x.JsonDetail.Priority == dto.Priority);

		if (dto.ShowProducts.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Media);
		if (dto.ShowUsers.IsTrue()) q = q.Include(x => x.Users)!.ThenInclude(x => x.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Categories);

		if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
		if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
		if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByCreaedDateDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		if (dto.Boosted) q = q.OrderByDescending(o => o.JsonDetail.Boosted);
		if (dto.ShowAhtorized) {
			List<OrderEntity> orders = dbContext.Set<OrderEntity>().Where(w => w.ProductOwnerId == _userId).ToList();
			q = q.Where(w => orders.Any(a => a.UserId == w.Users!.FirstOrDefault()!.Id));
		}

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<GroupChatEntity>>(q.AsNoTracking()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public GenericResponse<IQueryable<GroupChatEntity>> FilterAllGroupChats(GroupChatFilterDto dto) {
		IQueryable<GroupChatEntity> q = dbContext.Set<GroupChatEntity>();

		if (dto.UsersIds.IsNotNullOrEmpty()) q = q.Where(x => x.Users!.Any(y => y.Id == dto.UsersIds!.FirstOrDefault()));
		if (dto.ProductsIds.IsNotNullOrEmpty()) q = q.Where(x => x.Products!.Any(y => y.Id == dto.ProductsIds!.FirstOrDefault()));
		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == dto.Title);
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Description == dto.Description);
		if (dto.Type.HasValue) q = q.Where(x => x.Type == dto.Type);
		if (dto.Value.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Value == dto.Value);
		if (dto.Department.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Department == dto.Department);
		if (dto.ChatStatus.HasValue) q = q.Where(x => x.JsonDetail.ChatStatus == dto.ChatStatus);
		if (dto.Priority.HasValue) q = q.Where(x => x.JsonDetail.Priority == dto.Priority);

		if (dto.ShowProducts.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Media);
		if (dto.ShowUsers.IsTrue()) q = q.Include(x => x.Users)!.ThenInclude(x => x.Media);
		if (dto.ShowCategories.IsTrue()) q = q.Include(x => x.Products)!.ThenInclude(x => x.Categories);

		if (dto.OrderByAtoZ.IsTrue()) q = q.OrderBy(x => x.Title);
		if (dto.OrderByZtoA.IsTrue()) q = q.OrderByDescending(x => x.Title);
		if (dto.OrderByCreatedDate.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);
		if (dto.OrderByCreaedDateDecending.IsTrue()) q = q.OrderByDescending(x => x.CreatedAt);

		if (dto.Boosted) q = q.OrderByDescending(o => o.JsonDetail.Boosted);
		if (dto.ShowAhtorized) {
			List<OrderEntity> orders = dbContext.Set<OrderEntity>().Where(w => w.ProductOwnerId == _userId).ToList();
			q = q.Where(w => orders.Any(a => a.UserId == w.Users!.FirstOrDefault()!.Id));
		}

		int totalCount = q.Count();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		return new GenericResponse<IQueryable<GroupChatEntity>>(q.AsNoTracking()) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse<GroupChatEntity>> ReadGroupChatById(Guid id) {
		GroupChatEntity? e = await dbContext.Set<GroupChatEntity>()
			.Include(x => x.Users)!.ThenInclude(x => x.Media)
			.Include(x => x.Products)!.ThenInclude(x => x.Media)
			.Include(x => x.Products)!.ThenInclude(x => x.Categories)
			.Include(x => x.GroupChatMessage)
			.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);

		if (e != null) {
			int countOfMessage;
			SeenUsers? seenUsers = dbContext.Set<SeenUsers>().FirstOrDefault(w => w.FkGroupChat == e.Id && w.FkUserId == _userId);
			IQueryable<GroupChatMessageEntity> groupchatMessages = dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == e.Id);
			if (seenUsers is null) {
				countOfMessage = groupchatMessages.Count();
			}
			else {
				GroupChatMessageEntity lastSeenMessage = groupchatMessages.FirstOrDefault(w => w.Id == seenUsers.FkGroupChatMessage)!;
				countOfMessage = groupchatMessages.Count(w => w.CreatedAt > lastSeenMessage.CreatedAt);
			}

			e.CountOfUnreadMessages = countOfMessage;
			await promotionRepository.UserSeened(e.Id);
		}

		return new GenericResponse<GroupChatEntity>(e!);
	}

	public GenericResponse<IQueryable<GroupChatMessageEntity>?> ReadGroupChatMessages(Guid id, int pageSize, int pageNumber) {
		IQueryable<GroupChatMessageEntity> q = dbContext.Set<GroupChatMessageEntity>()
			.Where(x => x.GroupChatId == id)
			.Include(x => x.Media)
			.Include(x => x.Products)!.ThenInclude(x => x!.Media)
			.Include(x => x.Products)!.ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.Include(x => x.Parent).ThenInclude(x => x!.Media)
			.Include(x => x.Parent).ThenInclude(x => x!.Products)!.ThenInclude(x => x!.Media)
			.Include(x => x.Parent).ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.Include(x => x.User).ThenInclude(x => x!.Media)
			.Include(x => x.ForwardedMessage).ThenInclude(x => x!.Media)
			.Include(x => x.ForwardedMessage).ThenInclude(x => x!.Products)!.ThenInclude(x => x!.Media)
			.Include(x => x.ForwardedMessage).ThenInclude(x => x!.Parent).ThenInclude(x => x!.Media)
			.Include(x => x.ForwardedMessage).ThenInclude(x => x!.User).ThenInclude(x => x!.Media)
			.OrderBy(o => o.CreatedAt)
			.Reverse()
			.AsNoTracking();

		int totalCount = q.Count();
		q = q.Skip((pageNumber - 1) * pageSize).Take(pageSize);

		List<GroupChatMessageEntity> tempGroupChatsMessage = q.ToList();
		IQueryable<SeenUsers> messageSeen = dbContext.Set<SeenUsers>().Where(w => w.FkGroupChat == id);
		foreach (GroupChatMessageEntity? item in tempGroupChatsMessage) {
			List<UserEntity> usersMessage = new();
			foreach (SeenUsers? seenTbl in messageSeen) {
				GroupChatMessageEntity? lastMessageThatUserSeened = q.FirstOrDefault(f => f.Id == seenTbl.FkGroupChatMessage);
				UserEntity? user = dbContext.Set<UserEntity>().Where(w => w.Id == seenTbl.FkUserId).Include(x => x.Media).FirstOrDefault();
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

	public async Task<GenericResponse> SeenGroupChatMessage(Guid messageId) {
		GroupChatMessageEntity? groupMessageChat = await dbContext.Set<GroupChatMessageEntity>().Where(e => e.Id == messageId).FirstOrDefaultAsync();
		if (groupMessageChat is null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound, "GroupChatMessage Not Found!");

		GroupChatEntity? groupChat = await dbContext.Set<GroupChatEntity>().Where(w => w.Id == groupMessageChat.GroupChatId).FirstOrDefaultAsync();
		if (groupChat is null)
			return new GenericResponse(UtilitiesStatusCodes.NotFound, "GroupChate Not Found!");

		SeenUsers? seenUsers = await dbContext.Set<SeenUsers>().Where(w => w.FkGroupChat == groupChat.Id && w.FkUserId == _userId).FirstOrDefaultAsync();

		if (seenUsers is null) {
			await dbContext.Set<SeenUsers>().AddAsync(new SeenUsers {
				CreatedAt = DateTime.Now,
				FkGroupChat = groupChat.Id,
				FkUserId = _userId,
				FkGroupChatMessage = messageId
			});
		}
		else {
			GroupChatMessageEntity? lastMessageSeen =
				await dbContext.Set<GroupChatMessageEntity>().FirstOrDefaultAsync(f => f.Id == seenUsers.FkGroupChatMessage);
			if (lastMessageSeen?.CreatedAt > groupMessageChat.CreatedAt || seenUsers.FkGroupChatMessage == messageId) return new GenericResponse();

			seenUsers.FkGroupChatMessage = messageId;
			dbContext.Update(seenUsers);
		}

		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse> ExitFromGroup(Guid id) {
		GroupChatEntity? groupChat = await dbContext.Set<GroupChatEntity>().Include(i => i.Users).FirstOrDefaultAsync(f => f.Id == id);
		if (groupChat is null) return new GenericResponse(UtilitiesStatusCodes.NotFound, "Group Chat not Founded");

		if (groupChat.Users!.All(a => a.Id != _userId)) return new GenericResponse(UtilitiesStatusCodes.UserNotFound, "User Not Founded in GroupChat");

		UserEntity? user = await dbContext.Set<UserEntity>().Where(w => w.Id == _userId).FirstOrDefaultAsync();

		List<UserEntity> tempUsers = groupChat.Users!.ToList();
		bool result = tempUsers.Remove(user!);

		if (result) {
			groupChat.Users = tempUsers;

			dbContext.Update(groupChat);
			await dbContext.SaveChangesAsync();
			return new GenericResponse();
		}

		return new GenericResponse(UtilitiesStatusCodes.BadRequest);
	}

	public async Task<GenericResponse> Mute(Guid id) {
		UserEntity? user = await dbContext.Set<UserEntity>().FirstOrDefaultAsync(f => f.Id == _userId);
		if (user is null) return new GenericResponse(UtilitiesStatusCodes.UserNotFound);

		if (user.MutedChats.IsNullOrEmpty()) user.MutedChats += id.ToString();
		else user.MutedChats = user.MutedChats + "," + id;

		dbContext.Update(user);
		await dbContext.SaveChangesAsync();

		return new GenericResponse();
	}

	private async Task<GenericResponse<GroupChatEntity?>> CreateGroupChatLogic(GroupChatCreateUpdateDto dto) {
		if (dto.UserIds!.Count() > 2 && dto.Type == ChatType.Private)
			return new GenericResponse<GroupChatEntity?>(null, UtilitiesStatusCodes.MoreThan2UserIsInPrivateChat);
		List<UserEntity> users = new();
		if (dto.UserIds.IsNotNullOrEmpty())
			foreach (string id in dto.UserIds!) {
				Tuple<bool, UtilitiesStatusCodes> isBlocked = Utils.IsBlockedUser(dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == id),
					dbContext.Set<UserEntity>().FirstOrDefault(f => f.Id == _userId));
				if (!isBlocked.Item1) users.Add((await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			}

		List<ProductEntity> products = new();
		if (dto.Products.IsNotNullOrEmpty())
			foreach (Guid id in dto.Products!)
				products.Add((await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);

		GroupChatEntity entity = new() {
			Title = dto.Title,
			Type = dto.Type,
			CreatedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			CreatorUserId = _userId!,
			Users = users,
			Products = products,
			JsonDetail = new GroupChatJsonDetail {
				ChatStatus = dto.ChatStatus,
				Description = dto.Description,
				Value = dto.Value,
				Department = dto.Department,
				Priority = dto.Priority
			}
		};
		if (dto.Id != null) entity.Id = (Guid)dto.Id;

		if (dto.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> listCategory = new();
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? ce = await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (ce != null) listCategory.Add(ce);
			}

			entity.Categories = listCategory;
		}

		if (entity.Type == ChatType.PublicChannel) entity.JsonDetail.Boosted = DateTime.Now;

		EntityEntry<GroupChatEntity> e = await dbContext.Set<GroupChatEntity>().AddAsync(entity);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatEntity?>(e.Entity);
	}
}