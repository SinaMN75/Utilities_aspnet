namespace Utilities_aspnet.Utilities;

public static class Core {
	public static readonly JsonSerializerSettings JsonSettings = new() {
		Formatting = Formatting.Indented,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		NullValueHandling = NullValueHandling.Ignore
	};
}