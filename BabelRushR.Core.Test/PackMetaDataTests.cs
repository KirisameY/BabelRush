using BabelRushR.Core.Common.Serializing;
using BabelRushR.Core.Contents;

using Tomlyn;

namespace BabelRushR.Core.Test;

[Collection("Contents")]
public class PackMetaDataTests
{
    // 同 DataRegistrantTests：SerializingOptions 是进程级一次性状态，必须复位并和其他 Contents 用例串行。
    public PackMetaDataTests() => SerializingOptionsTestAccessor.ResetAndInitialize();

    [Fact]
    public void Metadata_Deserializes_All_Fields_With_Snake_Case_Names()
    {
        const string toml = """
                            pack_id = "core"
                            pack_name = "Core Pack"
                            version = "2.3.4"
                            load_before = ["ui", "audio"]
                            load_after = ["base"]

                            [[dependencies]]
                            pack_id = "base"
                            version = "1.2.0"

                            [[dependencies]]
                            pack_id = "i18n"
                            version = "0.9.0"
                            """;

        var meta = TomlSerializer.Deserialize<PackMetaData>(toml, SerializingOptions.Default);

        Assert.NotNull(meta);
        Assert.Equal("core", meta!.PackId);
        Assert.Equal("Core Pack", meta.PackName);
        Assert.Equal(new Version(2, 3, 4), meta.Version);
        Assert.Equal(["ui", "audio"], meta.LoadBefore);
        Assert.Equal(["base"], meta.LoadAfter);
        Assert.Equal(2, meta.Dependencies.Count);
        Assert.Equal("base", meta.Dependencies[0].PackId);
        Assert.Equal(new Version(1, 2, 0), meta.Dependencies[0].Version);
        Assert.Equal("i18n", meta.Dependencies[1].PackId);
    }

    [Fact]
    public void Optional_Collections_Default_To_Empty()
    {
        const string toml = """
                            pack_id = "minimal"
                            pack_name = "Minimal"
                            version = "1.0.0"
                            """;

        var meta = TomlSerializer.Deserialize<PackMetaData>(toml, SerializingOptions.Default)!;

        Assert.Empty(meta.Dependencies);
        Assert.Empty(meta.LoadBefore);
        Assert.Empty(meta.LoadAfter);
    }
}
