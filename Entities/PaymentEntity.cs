using System.Globalization;
using Newtonsoft.Json.Converters;

namespace Utilities_aspnet.Entities;

public partial class ZibalRequestReadDto {
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string? Message { get; set; }
	
	[JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
	public long? Result { get; set; }

	[JsonProperty("trackId", NullValueHandling = NullValueHandling.Ignore)]
	public long? TrackId { get; set; }
}

public partial class ZibalRequestReadDto {
	public static ZibalRequestReadDto? FromJson(string json) => JsonConvert.DeserializeObject<ZibalRequestReadDto>(json, ZibalRequestReadDtoConverter.Settings);
}

public static class ZibalRequestReadDtoSerialize {
	public static string ToJson(this ZibalRequestReadDto self) => JsonConvert.SerializeObject(self, ZibalRequestReadDtoConverter.Settings);
}

internal static class ZibalRequestReadDtoConverter {
	public static readonly JsonSerializerSettings? Settings = new() {
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } },
	};
}