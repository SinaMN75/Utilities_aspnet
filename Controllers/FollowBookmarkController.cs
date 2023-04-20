namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowBookmarkController : BaseApiController {
	private readonly IFollowBookmarkRepository _repository;

	public FollowBookmarkController(IFollowBookmarkRepository repository) => _repository = repository;

	[HttpPost("ReadFollowers/{userId}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserReadDto>>>> ReadFollowers(string userId) => Result(await _repository.GetFollowers(userId));

	[HttpPost("ReadFollowings/{userId}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse<IEnumerable<UserReadDto>>>> ReadFollowings(string userId) => Result(await _repository.GetFollowing(userId));

	[HttpPost("ToggleFolllow")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> ToggleFollow(FollowCreateDto dto) => Result(await _repository.ToggleFollow(dto));

	[HttpPost("RemoveFollowing")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> RemoveFollowing(FollowCreateDto dto) => Result(await _repository.RemoveFollowings(dto));

	[HttpPost("ToggleBookmark")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> ToggleBookmark(BookmarkCreateDto dto) => Result(await _repository.ToggleBookmark(dto));

	[HttpPut("UpdateBookmark")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) => Result(await _repository.UpdateBookmark(bookmarkId, dto));

	[HttpPost("ReadBookmarks")]
	public ActionResult<GenericResponse<IEnumerable<BookmarkEntity>?>> ReadBookmarks(string? userId) => Result(_repository.ReadBookmarks(userId));
}