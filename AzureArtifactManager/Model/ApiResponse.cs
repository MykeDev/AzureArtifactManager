using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AzureArtifactManager.Model
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("value")]
        public IEnumerable<T> Value { get; set; }
    }
}
