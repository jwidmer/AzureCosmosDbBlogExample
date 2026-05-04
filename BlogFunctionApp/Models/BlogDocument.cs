namespace BlogFunctionApp.Models;

/// <summary>
/// Typed binding for Cosmos DB change-feed documents. Captures the union of
/// fields used across post documents (Posts container) and user documents
/// (Users container). Property names use camelCase to match the JSON shape
/// produced by BlogWebApp's Newtonsoft-attributed models.
///
/// Note: the original sample bound Microsoft.Azure.Documents.Document, which
/// is a fully-dynamic JSON bag. The isolated-worker SDK does not provide that
/// type, so we use a typed POCO. To preserve the round-trip behavior the
/// change-feed function depends on (every field a post carries must survive
/// deserialization into BlogDocument and serialization back out into the Feed
/// and Users containers), every field BlogPost / BlogUser carries is declared
/// explicitly here.
/// </summary>
public sealed class BlogDocument
{
    // Fields common to every doc in Posts and Users containers.
    public string id { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public DateTime dateCreated { get; set; }

    // Post-specific fields. Property names match BlogPost's [JsonProperty]
    // names in BlogWebApp/Models/BlogPost.cs (camelCase by convention).
    public string postId { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
    public string userUsername { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public int commentCount { get; set; }
    public int likeCount { get; set; }

    // User-specific fields, plus the synthetic per-post placeholder the
    // post-to-Users-container hack writes (see FunctionPostsChangeFeed.Run).
    public string username { get; set; } = string.Empty;
    public string action { get; set; } = string.Empty;
}
