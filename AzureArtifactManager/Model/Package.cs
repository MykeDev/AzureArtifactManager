using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace AzureArtifactManager.Model
{
    public class Package
    {
        public Package()
        {
            AvailableVersions = new List<PackageVersion>();
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        public PackageVersion LatestVersion => AvailableVersions.FirstOrDefault(x => x.IsLatest);

        [JsonPropertyName("versions")]
        public IEnumerable<PackageVersion> AvailableVersions { get; set; }
    }
}
