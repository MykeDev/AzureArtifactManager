using System.Text.Json.Serialization;

namespace AzureArtifactManager.Model
{
    public class PackageVersion
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("isLatest")]
        public bool IsLatest { get; set; }

        [JsonPropertyName("packageDescription")]
        public string Description { get; set; }

    }
}
