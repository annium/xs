using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json.Serialization;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Node.Internal;
using Annium.Xs.Server.Shared.Domain.Models;
using NodaTime;

namespace Annium.Xs.Server.Node.Views.Requests;

public sealed record PackageRequest : IPackageRequest
{
    [JsonIgnore]
    public ProjectType ProjectType => Constants.ProjectType;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [JsonIgnore]
    public string Version => DistributionTags.TryGetValue("latest", out var version) ? version : string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 20)]
    public string Description { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("dist-tags")]
    public Dictionary<string, string> DistributionTags { get; init; } = new();

    [Required]
    public Dictionary<string, PackageVersionRequest> Versions { get; init; } = new();

    [Required]
    [JsonPropertyName("_attachments")]
    public Dictionary<string, PackageAttachmentRequest> Attachments { get; init; } = new();

    [JsonIgnore]
    public Stream Stream => new MemoryStream(Convert.FromBase64String(Attachments[$"{Name}-{Version}.tgz"].Data));

    [JsonIgnore]
    public Instant Published { get; set; }
}
