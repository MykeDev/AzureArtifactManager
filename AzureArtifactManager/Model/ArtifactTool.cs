using System.Text.Json.Serialization;

namespace AzureArtifactManager.Model
{
    public class ArtifactTool
    {
        [JsonPropertyName("name")]
        public string Name{ get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("rid")]
        public string Os { get; set; }
    }
}
