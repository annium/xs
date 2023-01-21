using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using NodaTime;
using Server.Abstractions.Packages;
using Server.Node.Models;

namespace Server.Node.Payloads;

public class PackagePayload : IPayload
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    public PackageName PackageName => PackageName.Parse(Name);

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

    public Stream Stream =>
        new MemoryStream(Convert.FromBase64String(Attachments[$"{Name}-{Version}.tgz"].Data));

    [JsonIgnore]
    public Instant Published { get; set; }
}