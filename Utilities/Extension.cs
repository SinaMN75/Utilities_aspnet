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

	public static string GetLast(this string source, int tailLength) {
		return tailLength >= source.Length ? source : source.Substring(source.Length - tailLength);
	}
}