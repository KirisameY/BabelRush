using System.Collections.Immutable;

using JetBrains.Annotations;

using Tomlyn.Serialization;

namespace BabelRushR.Core.Contents;

[method: TomlConstructor, UsedImplicitly]
public record PackMetaData(
    [property: TomlRequired] string PackId,
    [property: TomlRequired] string PackName,
    [property: TomlRequired] Version Version
)
{
    public ImmutableList<DependencyEntry> Dependencies { get; init; } = [];
    public ImmutableList<string> LoadBefore { get; init; } = [];
    public ImmutableList<string> LoadAfter { get; init; } = [];


    [method: TomlConstructor, UsedImplicitly]
    public record DependencyEntry(
        [property: TomlRequired] string PackId,
        [property: TomlRequired] Version Version
    );
}