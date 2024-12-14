namespace Utilities_aspnet.Utilities;

public class GenericResponse<T> : GenericResponse {
	public GenericResponse(T result, UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "") {
		Result = result;
		Status = status;
		Message = message;
	}

	public T? Result { get; }
}

public class GenericResponse(UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "") {
	public UtilitiesStatusCodes Status { get; protected set; } = status;
	public int? PageSize { get; set; }
	public int? PageCount { get; set; }
	public int? TotalCount { get; set; }
	public string Message { get; set; } = message;
}

public static class BoolExtension {
	public static bool IsTrue(this bool? value) => value != null && value != false;
}

public static class EnumerableExtension {
	public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? list) => list != null && list.Any();

	public static bool IsNotNull<T>(this IEnumerable<T>? list) => list != null;

	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? list) => list == null || list.Any();

	public static async Task<GenericResponse<IQueryable<T>>> Paginate<T>(this IQueryable<T> q, BaseFilterDto dto) where T : BaseEntity {
		int totalCount = await q.CountAsync();
		q = q.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).AsNoTracking();

		return new GenericResponse<IQueryable<T>>(q) {
			TotalCount = totalCount,
			PageCount = totalCount % dto.PageSize == 0 ? totalCount / dto.PageSize : totalCount / dto.PageSize + 1,
			PageSize = dto.PageSize
		};
	}
}

public static class StringExtension {
	public static bool IsNotNullOrEmpty(this string? s) => s is { Length: > 0 };

	public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

	public static string EncodeJson<T>(this T obj) => JsonConvert.SerializeObject(obj, Core.JsonSettings);

	public static T DecodeJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json, Core.JsonSettings)!;
}

public static class ListExtensions {
	public static bool Contains<T>(this IEnumerable<T> l1, IEnumerable<T> l2) => l1.Intersect(l2).Any();
}