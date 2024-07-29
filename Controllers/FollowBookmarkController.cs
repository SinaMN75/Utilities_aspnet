namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class FollowBookmarkController(IFollowBookmarkRepository repository, IUserRepository userRepository) : BaseApiController {
	[HttpPost("ReadFollowers/{userId}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> ReadFollowers(string userId) => Result(await userRepository.GetFollowers(userId));

	[HttpPost("ReadFollowings/{userId}")]
	[Authorize]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserEntity>>>> ReadFollowings(string userId) => Result(await userRepository.GetFollowing(userId));

	[HttpPost("ToggleFolllow")]
	[EnableRateLimiting("follow")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> ToggleFollow(FollowCreateDto dto) => Result(await userRepository.ToggleFollow(dto));

	[HttpPost("RemoveFollowing")]
	[Authorize]
	public async Task<ActionResult<GenericResponse>> RemoveFollowing(FollowCreateDto dto) => Result(await userRepository.RemoveFollowings(dto));

	[HttpPost("ToggleBookmark")]
	[Authorize]
	public async Task<IActionResult> ToggleBookmark(BookmarkCreateDto dto) => Result(await repository.ToggleBookmark(dto));

	[HttpPut("UpdateBookmark")]
	[Authorize]
	public async Task<IActionResult> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) => Result(await repository.UpdateBookmark(bookmarkId, dto));

	[HttpPost("ReadBookmarks")]
	public ActionResult<GenericResponse<IEnumerable<BookmarkEntity>?>> ReadBookmarks(string? userId) => Result(repository.ReadBookmarks(userId));
}