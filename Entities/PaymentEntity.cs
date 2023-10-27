using System.Globalization;
using Newtonsoft.Json.Converters;

namespace Utilities_aspnet.Entities;

internal static class Converter {
	public static readonly JsonSerializerSettings? Settings = new() {
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } },
	};
}

public class ZibalRequestCreateDto {
	[JsonProperty("merchant")]
	public string Merchant { get; set; }

	[JsonProperty("amount")]
	public long Amount { get; set; }

	[JsonProperty("callbackUrl")]
	public string CallbackUrl { get; set; }

	public static ZibalRequestCreateDto? FromJson(string json) => JsonConvert.DeserializeObject<ZibalRequestCreateDto>(json, Converter.Settings);
	public static string ToJson(ZibalRequestCreateDto self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

public class ZibalVerifyCreateDto {
	[JsonProperty("merchant")]
	public required string Merchant { get; set; }

	[JsonProperty("trackId")]
	public required long TrackId { get; set; }

	public static ZibalVerifyCreateDto? FromJson(string json) => JsonConvert.DeserializeObject<ZibalVerifyCreateDto>(json, Converter.Settings);
	public static string ToJson(ZibalVerifyCreateDto self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

public class ZibalRequestReadDto {
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string? Message { get; set; }

	[JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
	public long? Result { get; set; }

	[JsonProperty("trackId", NullValueHandling = NullValueHandling.Ignore)]
	public long? TrackId { get; set; }

	public static ZibalRequestReadDto? FromJson(string json) => JsonConvert.DeserializeObject<ZibalRequestReadDto>(json, Converter.Settings);

	public static string ToJson(ZibalRequestReadDto self) => JsonConvert.SerializeObject(self, Converter.Settings);
}