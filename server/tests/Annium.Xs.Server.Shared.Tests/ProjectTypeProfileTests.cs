using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Domain.Models.Profiles;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="ProjectTypeProfile"/> — the <see cref="Annium.Core.Mapper"/> profile that teaches
/// the mapper how to turn a raw <see cref="string"/> into a <see cref="ProjectType"/>. The profile maps via
/// <see cref="ProjectType.Register"/> rather than <see cref="ProjectType.Get"/>, so mapping an as-yet-unknown
/// name auto-registers it instead of throwing; that distinction is the contract pinned here.
/// </summary>
/// <remarks>
/// Names are suffixed with a fresh guid on every use, since <c>ProjectType._types</c> is a single static
/// list shared by every test class in the process.
/// </remarks>
public class ProjectTypeProfileTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTypeProfileTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ProjectTypeProfileTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<ProjectTypeProfile>());
    }

    /// <summary>
    /// Mapping a name the registry has never seen must auto-register it, not throw. This is what
    /// distinguishes the profile's use of <see cref="ProjectType.Register"/> from <see cref="ProjectType.Get"/>.
    /// </summary>
    [Fact]
    public void Map_NameNeverRegistered_RegistersItAndReturnsMatchingProjectType()
    {
        // arrange
        var mapper = Get<IMapper>();
        var name = UniqueName();

        // act
        var projectType = mapper.Map<ProjectType>(name);

        // assert
        projectType.IsNotNull();
        projectType.ToString().Is(name);

        // the mapping had the side effect of registering the name, so a plain Get now succeeds
        ReferenceEquals(ProjectType.Get(name), projectType).IsTrue();
    }

    /// <summary>
    /// Mapping an already-registered name must yield the existing instance rather than a second one,
    /// so that project types stay reference-comparable across mapped and directly-registered values.
    /// </summary>
    [Fact]
    public void Map_NameAlreadyRegistered_ReturnsTheExistingInstance()
    {
        // arrange
        var mapper = Get<IMapper>();
        var name = UniqueName();
        var registered = ProjectType.Register(name);

        // act
        var mapped = mapper.Map<ProjectType>(name);

        // assert
        ReferenceEquals(registered, mapped).IsTrue();
    }

    private static string UniqueName() => $"xs-shared-tests-project-type-profile-{Guid.NewGuid():N}";
}
