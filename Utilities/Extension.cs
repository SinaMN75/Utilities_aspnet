﻿namespace Utilities_aspnet.Utilities;

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
}

public static class StringExtension {
	
	public static bool IsNotNullOrEmpty(this string? s) => s is { Length: > 0 };

	public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);

	public static string? GetShebaNumber(this string s) {
		s = s.Replace(" ", "");
		s = s.Replace("IR", "");
		s = s.Replace("ir", "");
		return s.Length == 24 ? s : null;
	}
}

public static class ListExtensions {
	public static IQueryable<ProductEntity> Shuffle(this IEnumerable<ProductEntity> list) {
		List<ProductEntity> values = list.ToList();
		for (int i = values.Count - 1; i > 0; i--) {
			int k = new Random().Next(i + 1);
			(values[k], values[i]) = (values[i], values[k]);
		}

		return values.AsQueryable();
	}

	public static bool Contains<T>(this IEnumerable<T> l1, IEnumerable<T> l2) => l1.Intersect(l2).Any();
}