namespace Utilities_aspnet.Repositories;

public interface IChatRepository {
	Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto);
	Task<GenericResponse<IQueryable<GroupChatEntity>>> FilterGroupChats(GroupChatFilterDto dto);
	Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto);
	Task<GenericResponse> DeleteGroupChat(Guid id);
	Task<GenericResponse<IEnumerable<GroupChatEntity>?>> ReadMyGroupChats();
	Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
	Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto);
	Task<GenericResponse> DeleteGroupChatMessage(Guid id);
	Task<GenericResponse<GroupChatEntity>> ReadGroupChatById(Guid id);
	Task<GenericResponse<IEnumerable<GroupChatMessageEntity>?>> FilterGroupChatMessages(FilterGroupChatMessagesDto dto);
	Task<GenericResponse> SeenGroupChatMessage(Guid messageId);
	Task<GenericResponse> ExitFromGroup(Guid id);
	Task<GenericResponse> Mute(Guid id);
}

public class ChatRepository(
	DbContext dbContext,
	IHttpContextAccessor httpContextAccessor,
	IConfiguration config,
	IMediaRepository mediaRepository,
	INotificationRepository notificationRepository
) : IChatRepository {
	private readonly string? _userId = httpContextAccessor.HttpContext!.User.Identity!.Name;

	public async Task<GenericResponse<GroupChatEntity?>> CreateGroupChat(GroupChatCreateUpdateDto dto) {
		if (dto.Type != ChatType.Private) return await CreateGroupChatLogic(dto);
		string firstUserId = dto.UserIds!.ToList()[0];
		string secondUserId = dto.UserIds!.ToList()[1];

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

	public async Task<GenericResponse<GroupChatEntity?>> UpdateGroupChat(GroupChatCreateUpdateDto dto) {
		GroupChatEntity e = (await dbContext.Set<GroupChatEntity>()
			.Include(x => x.Users)
			.Include(x => x.Products)
			.FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		if (dto.UserIds.IsNotNull()) {
			List<UserEntity> users = [];
			e.Users = users;
		}

		if (dto.Products.IsNotNull()) {
			List<ProductEntity> products = [];
			foreach (Guid id in dto.Products!) products.Add((await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			e.Products = products;
		}

		if (dto.Categories.IsNotNull()) {
			List<CategoryEntity> list = [];
			foreach (Guid id in dto.Categories!) list.Add((await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == id))!);
			e.Categories = list;
		}

		e.Title = dto.Title ?? e.Title;
		e.Type = dto.Type ?? e.Type;
		e.Tags = dto.Tags ?? e.Tags;
		
		e.UpdatedAt = DateTime.UtcNow;

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
		GroupChatEntity e = (await dbContext.Set<GroupChatEntity>()
			.Include(x => x.GroupChatMessage)!.ThenInclude(x => x.Media)
			.Include(x => x.Media)
			.Include(x => x.Notifications)
			.FirstOrDefaultAsync(x => x.Id == id))!;
		await mediaRepository.DeleteMedia(e.Media);
		foreach (GroupChatMessageEntity i in e.GroupChatMessage ?? []) await mediaRepository.DeleteMedia(i.Media);
		foreach (NotificationEntity? i in e.Notifications ?? []) await notificationRepository.Delete(i!.Id);
		dbContext.Set<GroupChatMessageEntity>().RemoveRange(e.GroupChatMessage!);
		dbContext.Set<GroupChatEntity>().Remove(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<GroupChatMessageEntity?>> CreateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		List<ProductEntity?> products = [];
		foreach (Guid id in dto.Products ?? [])
			products.Add(await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id));

		AppSettings appSettings = new();
		config.GetSection("AppSettings").Bind(appSettings);

		GroupChatMessageEntity entity = new() {
			Message = dto.Message,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			GroupChatId = dto.GroupChatId,
			ParentId = dto.ParentId,
			UserId = _userId,
			Products = products,
			ForwardedMessageId = dto.ForwardedMessageId
		};

		EntityEntry<GroupChatMessageEntity> e = await dbContext.Set<GroupChatMessageEntity>().AddAsync(entity);

		try {
			GroupChatEntity gce = (await dbContext.Set<GroupChatEntity>().AsNoTracking()
				.Include(x => x.Users)
				.FirstOrDefaultAsync(x => x.Id == dto.GroupChatId))!;

			foreach (UserEntity i in gce.Users ?? []) {
				await notificationRepository.Create(new NotificationCreateUpdateDto {
					Title = "پیام جدید",
					UserId = i.Id,
					CreatorUserId = _userId,
					Message = dto.Message,
					GroupChatId = dto.GroupChatId,
					Tags = [TagNotification.ReceivedChat],
				});
			}
		}
		catch (Exception) { }

		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatMessageEntity?>(e.Entity);
	}

	public async Task<GenericResponse<GroupChatMessageEntity?>> UpdateGroupChatMessage(GroupChatMessageCreateUpdateDto dto) {
		GroupChatMessageEntity e = (await dbContext.Set<GroupChatMessageEntity>()
			.FirstOrDefaultAsync(x => x.Id == dto.Id))!;

		e.Message = dto.Message ?? e.Message;
		e.UpdatedAt = DateTime.UtcNow;

		EntityEntry<GroupChatMessageEntity> entity = dbContext.Set<GroupChatMessageEntity>().Update(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatMessageEntity?>(entity.Entity);
	}

	public async Task<GenericResponse> DeleteGroupChatMessage(Guid id) {
		GroupChatMessageEntity e = (await dbContext.Set<GroupChatMessageEntity>()
			.Include(x => x.Media)
			.Include(x => x.SeenUsers)
			.FirstOrDefaultAsync(x => x.Id == id))!;

		await mediaRepository.DeleteMedia(e.Media);

		if (e.SeenUsers is not null) dbContext.RemoveRange(e.SeenUsers!);
		dbContext.Remove(e);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
	}

	public async Task<GenericResponse<IEnumerable<GroupChatEntity>?>> ReadMyGroupChats() {
		await DeleteEmptyGroups();
		List<GroupChatEntity> e = await dbContext.Set<GroupChatEntity>()
			.Where(x => x.Users!.Any(y => y.Id == _userId))
			.Include(x => x.Users)!.ThenInclude(x => x.Media)
			.Include(x => x.Media)
			.Include(x => x.GroupChatMessage!.OrderByDescending(y => y.CreatedAt).Take(1)).ThenInclude(x => x.Media)
			.ToListAsync();

		foreach (GroupChatEntity groupChatEntity in e.Where(groupChatEntity => groupChatEntity.Type == ChatType.Private))
			if (groupChatEntity.Users!.First().Id == _userId) {
				UserEntity u = (await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.Last().Id))!;
				groupChatEntity.Title = u.AppUserName;
			}
			else {
				UserEntity u = (await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == groupChatEntity.Users!.First().Id))!;
				groupChatEntity.Title = u.AppUserName;
			}

		List<GroupChatEntity> myGroupChats = [];

		foreach (GroupChatEntity? item in e) {
			int countOfMessage;
			SeenUsers? seenUsers = dbContext.Set<SeenUsers>().FirstOrDefault(w => w.FkGroupChat == item.Id && w.FkUserId == _userId);
			IQueryable<GroupChatMessageEntity> groupChatMessages = dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == item.Id);
			if (seenUsers is null) {
				countOfMessage = groupChatMessages.Count();
			}
			else {
				GroupChatMessageEntity lastSeenMessage = (await groupChatMessages.Where(w => w.Id == seenUsers.FkGroupChatMessage!.Value).FirstOrDefaultAsync())!;
				countOfMessage = await groupChatMessages.Where(w => w.CreatedAt > lastSeenMessage.CreatedAt).CountAsync();
			}

			item.CountOfUnreadMessages = countOfMessage;
			myGroupChats.Add(item);
		}

		return new GenericResponse<IEnumerable<GroupChatEntity>?>(myGroupChats);
	}

	public async Task<GenericResponse<IQueryable<GroupChatEntity>>> FilterGroupChats(GroupChatFilterDto dto) {
		await DeleteEmptyGroups();
		IQueryable<GroupChatEntity> q = dbContext.Set<GroupChatEntity>().AsNoTracking();

		if (dto.Ids.IsNotNullOrEmpty()) q = q.Where(x => dto.Ids!.Contains(x.Id));
		if (dto.UsersIds.IsNotNullOrEmpty()) q = q.Where(x => x.Users!.Any(y => dto.UsersIds!.Contains(y.Id)));
		if (dto.ProductsIds.IsNotNullOrEmpty()) q = q.Where(x => x.Products!.Any(y => dto.ProductsIds!.Contains(y.Id)));
		if (dto.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == dto.Title);
		if (dto.Description.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Description == dto.Description);
		if (dto.Type.HasValue) q = q.Where(x => x.Type == dto.Type);
		if (dto.Value.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Value == dto.Value);
		if (dto.Department.IsNotNullOrEmpty()) q = q.Where(x => x.JsonDetail.Department == dto.Department);
		if (dto.ChatStatus.HasValue) q = q.Where(x => x.JsonDetail.ChatStatus == dto.ChatStatus);
		if (dto.Priority.HasValue) q = q.Where(x => x.JsonDetail.Priority == dto.Priority);
		if (dto.StartDate.HasValue) q = q.Where(x => x.CreatedAt >= dto.StartDate);
		if (dto.EndDate.HasValue) q = q.Where(x => x.CreatedAt <= dto.EndDate);

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

		if (e == null) return new GenericResponse<GroupChatEntity>(e!);
		int countOfMessage;
		SeenUsers? seenUsers = dbContext.Set<SeenUsers>().FirstOrDefault(w => w.FkGroupChat == e.Id && w.FkUserId == _userId);
		IQueryable<GroupChatMessageEntity> groupChatMessages = dbContext.Set<GroupChatMessageEntity>().Where(w => w.GroupChatId == e.Id);
		if (seenUsers is null) {
			countOfMessage = groupChatMessages.Count();
		}
		else {
			GroupChatMessageEntity lastSeenMessage = groupChatMessages.FirstOrDefault(w => w.Id == seenUsers.FkGroupChatMessage)!;
			countOfMessage = groupChatMessages.Count(w => w.CreatedAt > lastSeenMessage.CreatedAt);
		}

		e.CountOfUnreadMessages = countOfMessage;

		return new GenericResponse<GroupChatEntity>(e);
	}

	public async Task<GenericResponse<IEnumerable<GroupChatMessageEntity>?>> FilterGroupChatMessages(FilterGroupChatMessagesDto dto) {
		IQueryable<GroupChatMessageEntity> q = dbContext.Set<GroupChatMessageEntity>()
			.Where(x => x.GroupChatId == dto.GroupChatId)
			.Where(x => (x.Message ?? "").Contains(dto.Message ?? ""))
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
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize);

		// List<GroupChatMessageEntity> tempGroupChatsMessage = await q.ToListAsync();
		// IQueryable<SeenUsers> messageSeen = dbContext.Set<SeenUsers>().Where(w => w.FkGroupChat == dto.GroupChatId);
		// foreach (GroupChatMessageEntity? item in tempGroupChatsMessage) {
		// 	List<UserEntity> usersMessage = [];
		// 	foreach (SeenUsers? seenTbl in messageSeen) {
		// 		GroupChatMessageEntity? lastMessageThatUserSaw = await q.FirstOrDefaultAsync(f => f.Id == seenTbl.FkGroupChatMessage);
		// 		UserEntity? user = await dbContext.Set<UserEntity>().Where(w => w.Id == seenTbl.FkUserId).Include(x => x.Media).FirstOrDefaultAsync();
		// 		if (user is not null && (lastMessageThatUserSaw?.CreatedAt > item.CreatedAt || lastMessageThatUserSaw?.Id == item.Id))
		// 			usersMessage.Add(user);
		// 	}
		//
		// 	item.MessageSeenBy = usersMessage;
		// }

		return new GenericResponse<IEnumerable<GroupChatMessageEntity>?>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}

	public async Task<GenericResponse> SeenGroupChatMessage(Guid messageId) {
		GroupChatMessageEntity message = (await dbContext.Set<GroupChatMessageEntity>().Where(e => e.Id == messageId).FirstOrDefaultAsync())!;
		GroupChatEntity groupChat = (await dbContext.Set<GroupChatEntity>().Where(w => w.Id == message.GroupChatId).FirstOrDefaultAsync())!;
		SeenUsers? seenUsers = await dbContext.Set<SeenUsers>().Where(w => w.FkGroupChat == groupChat.Id && w.FkUserId == _userId).FirstOrDefaultAsync();

		if (seenUsers is null) {
			await dbContext.Set<SeenUsers>().AddAsync(new SeenUsers {
				CreatedAt = DateTime.UtcNow,
				FkGroupChat = groupChat.Id,
				FkUserId = _userId,
				FkGroupChatMessage = messageId
			});
		}
		else {
			GroupChatMessageEntity? lastMessageSeen =
				await dbContext.Set<GroupChatMessageEntity>().FirstOrDefaultAsync(f => f.Id == seenUsers.FkGroupChatMessage);
			if (lastMessageSeen?.CreatedAt > message.CreatedAt || seenUsers.FkGroupChatMessage == messageId) return new GenericResponse();

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

		if (!result) return new GenericResponse(UtilitiesStatusCodes.BadRequest);
		groupChat.Users = tempUsers;

		dbContext.Update(groupChat);
		await dbContext.SaveChangesAsync();
		return new GenericResponse();
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
		List<UserEntity> users = [];

		foreach (string i in dto.UserIds ?? [])
			users.Add((await dbContext.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == i))!);

		List<ProductEntity> products = [];
		if (dto.Products.IsNotNullOrEmpty())
			foreach (Guid id in dto.Products!)
				products.Add((await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id))!);

		GroupChatEntity entity = new() {
			Title = dto.Title,
			Type = dto.Type,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			CreatorUserId = _userId!,
			Users = users,
			Tags = dto.Tags ?? [],
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
			List<CategoryEntity> listCategory = [];
			foreach (Guid item in dto.Categories!) {
				CategoryEntity? ce = await dbContext.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (ce != null) listCategory.Add(ce);
			}

			entity.Categories = listCategory;
		}

		if (entity.Type == ChatType.PublicChannel) entity.JsonDetail.Boosted = DateTime.UtcNow;

		EntityEntry<GroupChatEntity> e = await dbContext.Set<GroupChatEntity>().AddAsync(entity);
		await dbContext.SaveChangesAsync();
		return new GenericResponse<GroupChatEntity?>(e.Entity);
	}

	private async Task DeleteEmptyGroups() {
		IQueryable<GroupChatEntity> list = dbContext.Set<GroupChatEntity>()
			.Where(x => x.Type != ChatType.Private)
			.Include(x => x.Users)
			.Include(x => x.GroupChatMessage);
		foreach (GroupChatEntity groupChatEntity in list)
			if ((groupChatEntity.Users.IsNullOrEmpty() || groupChatEntity.GroupChatMessage.IsNullOrEmpty()) && groupChatEntity.Type == ChatType.Private)
				dbContext.Remove(groupChatEntity);
		await dbContext.SaveChangesAsync();
	}
}