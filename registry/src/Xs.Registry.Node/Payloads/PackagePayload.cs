using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;

namespace Xs.Registry.Node.Payloads
{
    public class PackagePayload
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [JsonIgnore]
        public string Version => DistributionTags?.ContainsKey("latest") ?? false ? DistributionTags["latest"] : "";

        [Required]
        [StringLength(1000, MinimumLength = 20)]
        public string Description { get; set; }

        [Required]
        [JsonProperty("dist-tags")]
        public Dictionary<string, string> DistributionTags { get; set; }

        [Required]
        public Dictionary<string, PackageVersionPayload> Versions { get; set; }

        [Required]
        [JsonProperty("_attachments")]
        public Dictionary<string, PackageAttachmentPayload> Attachments { get; set; }

        public Stream GetAttachment() =>
            new MemoryStream(Convert.FromBase64String(Attachments[$"{Name}-{Version}.tgz"].Data));

        [JsonIgnore]
        public DateTime Published { get; set; }

        public static explicit operator Models.Package(PackagePayload src)
        {
            var version = src.Versions[src.Version];

            return new Models.Package(
                Models.PackageName.Parse(src.Name),
                src.Version,
                src.Description,
                version.Main,
                version.Dependencies,
                version.DevDependencies,
                src.Published,
                version.Distribution.Shasum,
                version.Distribution.Integrity
            );
        }
    }
}