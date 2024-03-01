namespace Utilities_aspnet.Utilities;

public static class Extension {
	public static string DeleteAdditionsInsteadNumber(this string str) {
		str = str.Replace("@", "");
		str = str.Replace("#", "");
		str = str.Replace("+", "");
		str = str.Replace("*", "");
		str = str.Replace(" ", "");
		return str;
	}
	
	public static bool Contains<T>(this IEnumerable<T> l1, IEnumerable<T> l2) => l1.Intersect(l2).Any();
	
	public static string GetLast(this string source, int tailLength) => tailLength >= source.Length ? source : source[^tailLength..];
}