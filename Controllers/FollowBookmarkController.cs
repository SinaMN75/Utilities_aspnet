namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowBookmarkController : BaseApiController {
	private readonly IFollowBookmarkRepository _repository;

	public FollowBookmarkController(IFollowBookmarkRepository repository) => _repository = repository;

	[HttpPost("ReadFollowers/{userId}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public ActionResult<GenericResponse<IEnumerable<UserEntity>>> ReadFollowers(string userId) => Result(_repository.GetFollowers(userId));

	[HttpPost("ReadFollowings/{userId}")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public ActionResult<GenericResponse<IQueryable<UserEntity>>> ReadFollowings(string userId) => Result(_repository.GetFollowing(userId));

	[HttpPost("ToggleFolllow")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> ToggleFollow(FollowCreateDto dto) => Result(await _repository.ToggleFollow(dto));

	[HttpPost("RemoveFollowing")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<GenericResponse>> RemoveFollowing(FollowCreateDto dto)
		=> Result(await _repository.RemoveFollowings(User.Identity?.Name!, dto));

	[HttpPost("ToggleBookmark")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> ToggleBookmark(BookmarkCreateDto dto) => Result(await _repository.ToggleBookmark(dto));

	[HttpPut("UpdateBookmark")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<IActionResult> UpdateBookmark(Guid bookmarkId, BookmarkCreateDto dto) => Result(await _repository.UpdateBookmark(bookmarkId, dto));

	[HttpPost("ReadBookmarks")]
	public ActionResult<GenericResponse<IEnumerable<BookmarkEntity>?>> ReadBookmarks(string? userId) => Result(_repository.ReadBookmarks(userId));

	[HttpGet("ReadBookmarksByFolderName")]
	public ActionResult<GenericResponse<IEnumerable<BookmarkEntity>?>> ReadBookmarksByFolderName(string userId, string? folderName = null)
		=> Result(_repository.ReadBookmarksByFolderName(folderName, userId));
}