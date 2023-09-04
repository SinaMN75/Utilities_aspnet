namespace Utilities_aspnet.Entities.Instagram;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class InstagramUserId {
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public string Status { get; set; } = null!;

	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public string Data { get; set; } = null!;
}

public partial class InstagramUserId {
	public static InstagramUserId FromJson(string json) => JsonConvert.DeserializeObject<InstagramUserId>(json, InstagramUserIdConverter.Settings)!;
}

internal static class InstagramUserIdConverter {
	public static readonly JsonSerializerSettings Settings = new() {
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters = {
			new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
		},
	};
}