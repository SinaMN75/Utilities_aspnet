namespace Utilities_aspnet.Entities.Instagram;

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class InstagramUserPost {
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public string Status { get; set; }

	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public Data? Data { get; set; }
}

public partial class Data {
	[JsonProperty("next_page", NullValueHandling = NullValueHandling.Ignore)]
	public bool? NextPage { get; set; }

	[JsonProperty("end_cursor", NullValueHandling = NullValueHandling.Ignore)]
	public string EndCursor { get; set; }

	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<DataEdge>? Edges { get; set; }
}

public partial class DataEdge {
	[JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
	public PurpleNode? Node { get; set; }
}

public partial class PurpleNode {
	[JsonProperty("__typename", NullValueHandling = NullValueHandling.Ignore)]
	public string Typename { get; set; }

	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; set; }

	[JsonProperty("gating_info")]
	public object GatingInfo { get; set; }

	[JsonProperty("fact_check_overall_rating")]
	public object FactCheckOverallRating { get; set; }

	[JsonProperty("fact_check_information")]
	public object FactCheckInformation { get; set; }

	[JsonProperty("media_overlay_info")]
	public object MediaOverlayInfo { get; set; }

	[JsonProperty("sensitivity_friction_info")]
	public object SensitivityFrictionInfo { get; set; }

	[JsonProperty("sharing_friction_info", NullValueHandling = NullValueHandling.Ignore)]
	public SharingFrictionInfo SharingFrictionInfo { get; set; }

	[JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
	public Dimensions Dimensions { get; set; }

	[JsonProperty("display_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? DisplayUrl { get; set; }

	[JsonProperty("display_resources", NullValueHandling = NullValueHandling.Ignore)]
	public List<Resource> DisplayResources { get; set; }

	[JsonProperty("is_video", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsVideo { get; set; }

	[JsonProperty("media_preview")]
	public string MediaPreview { get; set; }

	[JsonProperty("tracking_token", NullValueHandling = NullValueHandling.Ignore)]
	public string TrackingToken { get; set; }

	[JsonProperty("has_upcoming_event", NullValueHandling = NullValueHandling.Ignore)]
	public bool? HasUpcomingEvent { get; set; }

	[JsonProperty("edge_media_to_tagged_user", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaToTaggedUser EdgeMediaToTaggedUser { get; set; }

	[JsonProperty("accessibility_caption")]
	public object AccessibilityCaption { get; set; }

	[JsonProperty("edge_media_to_caption", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaToCaption EdgeMediaToCaption { get; set; }

	[JsonProperty("shortcode", NullValueHandling = NullValueHandling.Ignore)]
	public string Shortcode { get; set; }

	[JsonProperty("edge_media_to_comment", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaToComment EdgeMediaToComment { get; set; }

	[JsonProperty("edge_media_to_sponsor_user", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaToSponsorUser EdgeMediaToSponsorUser { get; set; }

	[JsonProperty("is_affiliate", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsAffiliate { get; set; }

	[JsonProperty("is_paid_partnership", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsPaidPartnership { get; set; }

	[JsonProperty("comments_disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? CommentsDisabled { get; set; }

	[JsonProperty("taken_at_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public long? TakenAtTimestamp { get; set; }

	[JsonProperty("edge_media_preview_like", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaPreviewLike EdgeMediaPreviewLike { get; set; }

	[JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
	public Owner Owner { get; set; }

	[JsonProperty("location")]
	public object Location { get; set; }

	[JsonProperty("nft_asset_info")]
	public object NftAssetInfo { get; set; }

	[JsonProperty("viewer_has_liked", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ViewerHasLiked { get; set; }

	[JsonProperty("viewer_has_saved", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ViewerHasSaved { get; set; }

	[JsonProperty("viewer_has_saved_to_collection", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ViewerHasSavedToCollection { get; set; }

	[JsonProperty("viewer_in_photo_of_you", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ViewerInPhotoOfYou { get; set; }

	[JsonProperty("viewer_can_reshare", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ViewerCanReshare { get; set; }

	[JsonProperty("thumbnail_src", NullValueHandling = NullValueHandling.Ignore)]
	public Uri ThumbnailSrc { get; set; }

	[JsonProperty("thumbnail_resources", NullValueHandling = NullValueHandling.Ignore)]
	public List<Resource> ThumbnailResources { get; set; }

	[JsonProperty("coauthor_producers", NullValueHandling = NullValueHandling.Ignore)]
	public List<object> CoauthorProducers { get; set; }

	[JsonProperty("pinned_for_users", NullValueHandling = NullValueHandling.Ignore)]
	public List<object> PinnedForUsers { get; set; }

	[JsonProperty("edge_sidecar_to_children", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeSidecarToChildren? EdgeSidecarToChildren { get; set; }

	[JsonProperty("dash_info", NullValueHandling = NullValueHandling.Ignore)]
	public DashInfo DashInfo { get; set; }

	[JsonProperty("has_audio", NullValueHandling = NullValueHandling.Ignore)]
	public bool? HasAudio { get; set; }

	[JsonProperty("video_url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri VideoUrl { get; set; }

	[JsonProperty("video_view_count", NullValueHandling = NullValueHandling.Ignore)]
	public long? VideoViewCount { get; set; }

	[JsonProperty("product_type", NullValueHandling = NullValueHandling.Ignore)]
	public string ProductType { get; set; }
}

public partial class DashInfo {
	[JsonProperty("is_dash_eligible", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsDashEligible { get; set; }

	[JsonProperty("video_dash_manifest", NullValueHandling = NullValueHandling.Ignore)]
	public string VideoDashManifest { get; set; }

	[JsonProperty("number_of_qualities", NullValueHandling = NullValueHandling.Ignore)]
	public long? NumberOfQualities { get; set; }
}

public partial class Dimensions {
	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public long? Height { get; set; }

	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public long? Width { get; set; }
}

public partial class Resource {
	[JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
	public Uri Src { get; set; }

	[JsonProperty("config_width", NullValueHandling = NullValueHandling.Ignore)]
	public long? ConfigWidth { get; set; }

	[JsonProperty("config_height", NullValueHandling = NullValueHandling.Ignore)]
	public long? ConfigHeight { get; set; }
}

public partial class EdgeMediaPreviewLike {
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public long? Count { get; set; }

	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<object> Edges { get; set; }
}

public partial class EdgeMediaToCaption {
	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<EdgeMediaToCaptionEdge> Edges { get; set; }
}

public partial class EdgeMediaToCaptionEdge {
	[JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
	public FluffyNode Node { get; set; }
}

public partial class FluffyNode {
	[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
	public string Text { get; set; }
}

public partial class EdgeMediaToComment {
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public long? Count { get; set; }

	[JsonProperty("page_info", NullValueHandling = NullValueHandling.Ignore)]
	public PageInfo PageInfo { get; set; }
}

public partial class PageInfo {
	[JsonProperty("has_next_page", NullValueHandling = NullValueHandling.Ignore)]
	public bool? HasNextPage { get; set; }

	[JsonProperty("end_cursor")]
	public string EndCursor { get; set; }
}

public partial class EdgeMediaToSponsorUser {
	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<object> Edges { get; set; }
}

public partial class EdgeMediaToTaggedUser {
	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<EdgeMediaToTaggedUserEdge> Edges { get; set; }
}

public partial class EdgeMediaToTaggedUserEdge {
	[JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
	public TentacledNode Node { get; set; }
}

public partial class TentacledNode {
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public User User { get; set; }

	[JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
	public double? X { get; set; }

	[JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
	public double? Y { get; set; }
}

public partial class User {
	[JsonProperty("full_name", NullValueHandling = NullValueHandling.Ignore)]
	public string FullName { get; set; }

	[JsonProperty("followed_by_viewer", NullValueHandling = NullValueHandling.Ignore)]
	public bool? FollowedByViewer { get; set; }

	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; set; }

	[JsonProperty("is_verified", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsVerified { get; set; }

	[JsonProperty("profile_pic_url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri ProfilePicUrl { get; set; }

	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; set; }
}

public partial class EdgeSidecarToChildren {
	[JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
	public List<EdgeSidecarToChildrenEdge>? Edges { get; set; }
}

public partial class EdgeSidecarToChildrenEdge {
	[JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
	public StickyNode? Node { get; set; }
}

public partial class StickyNode {
	[JsonProperty("__typename", NullValueHandling = NullValueHandling.Ignore)]
	public string Typename { get; set; }

	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; set; }

	[JsonProperty("gating_info")]
	public object GatingInfo { get; set; }

	[JsonProperty("fact_check_overall_rating")]
	public object FactCheckOverallRating { get; set; }

	[JsonProperty("fact_check_information")]
	public object FactCheckInformation { get; set; }

	[JsonProperty("media_overlay_info")]
	public object MediaOverlayInfo { get; set; }

	[JsonProperty("sensitivity_friction_info")]
	public object SensitivityFrictionInfo { get; set; }

	[JsonProperty("sharing_friction_info", NullValueHandling = NullValueHandling.Ignore)]
	public SharingFrictionInfo SharingFrictionInfo { get; set; }

	[JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
	public Dimensions Dimensions { get; set; }

	[JsonProperty("display_url", NullValueHandling = NullValueHandling.Ignore)]
	public string DisplayUrl { get; set; } = "";

	[JsonProperty("display_resources", NullValueHandling = NullValueHandling.Ignore)]
	public List<Resource> DisplayResources { get; set; }

	[JsonProperty("is_video", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsVideo { get; set; }

	[JsonProperty("media_preview", NullValueHandling = NullValueHandling.Ignore)]
	public string MediaPreview { get; set; }

	[JsonProperty("tracking_token", NullValueHandling = NullValueHandling.Ignore)]
	public string TrackingToken { get; set; }

	[JsonProperty("has_upcoming_event", NullValueHandling = NullValueHandling.Ignore)]
	public bool? HasUpcomingEvent { get; set; }

	[JsonProperty("edge_media_to_tagged_user", NullValueHandling = NullValueHandling.Ignore)]
	public EdgeMediaToTaggedUser EdgeMediaToTaggedUser { get; set; }

	[JsonProperty("accessibility_caption")]
	public object AccessibilityCaption { get; set; }

	[JsonProperty("dash_info", NullValueHandling = NullValueHandling.Ignore)]
	public DashInfo DashInfo { get; set; }

	[JsonProperty("has_audio", NullValueHandling = NullValueHandling.Ignore)]
	public bool? HasAudio { get; set; }

	[JsonProperty("video_url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri VideoUrl { get; set; }

	[JsonProperty("video_view_count", NullValueHandling = NullValueHandling.Ignore)]
	public long? VideoViewCount { get; set; }
}

public partial class SharingFrictionInfo {
	[JsonProperty("should_have_sharing_friction", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ShouldHaveSharingFriction { get; set; }

	[JsonProperty("bloks_app_url")]
	public object BloksAppUrl { get; set; }
}

public partial class Owner {
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; set; }

	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; set; }
}

public partial class InstagramUserPost {
	public static InstagramUserPost FromJson(string json) =>
		JsonConvert.DeserializeObject<InstagramUserPost>(json, InstagramUserPostConverter.Settings)!;
}

internal static class InstagramUserPostConverter {
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters = {
			new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
		},
	};
}