using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using NodaTime;
using Server.Abstractions.Domain;
using Server.Domain.Models;
using Server.Node.Models;

namespace Server.Node.Payloads;

public class PackagePayload : IPayload
{
    [JsonIgnore]
    public ProjectType ProjectType => Constants.ProjectType;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public PackageName PackageName => PackageName.Parse(Name);

    [JsonIgnore]
    public string Version => DistributionTags.TryGetValue("latest", out var version) ? version : string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [JsonProperty("dist-tags")]
    public Dictionary<string, string> DistributionTags { get; set; } = new();

    [Required]
    public Dictionary<string, PackageVersionPayload> Versions { get; set; } = new();

    [Required]
    [JsonProperty("_attachments")]
    public Dictionary<string, PackageAttachmentPayload> Attachments { get; set; } = new();

    [JsonIgnore]
    public Stream Stream =>
        new MemoryStream(Convert.FromBase64String(Attachments[$"{Name}-{Version}.tgz"].Data));

    [JsonIgnore]
    public Instant Published { get; set; }
}