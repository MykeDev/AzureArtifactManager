using System.Text.Json.Serialization;

namespace AzureArtifactManager.Model
{
    public class Feed
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("project")]
        public Project Project{ get; set; }
    }
}
