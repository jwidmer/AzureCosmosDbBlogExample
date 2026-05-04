namespace BlogFunctionApp.Models;

/// <summary>
/// Minimal typed binding for Cosmos DB change-feed documents. Matches the
/// fields the change-feed functions read from the original
/// Microsoft.Azure.Documents.Document API. Uses camelCase to align with the
/// CosmosDbBlogConnectionString serializer options (CamelCase property naming).
/// </summary>
public sealed class BlogDocument
{
    public string id { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
    public string postId { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public string action { get; set; } = string.Empty;
    public DateTime dateCreated { get; set; }

    // Capture any additional fields without losing them on round-trip.
    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? AdditionalData { get; set; }
}
