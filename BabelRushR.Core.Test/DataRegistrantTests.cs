using BabelRushR.Core.Common.Serializing;
using BabelRushR.Core.Data;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

using Tomlyn.Serialization;

using Zio;
using Zio.FileSystems;

namespace BabelRushR.Core.Test;

[Collection("Contents")]
public class DataRegistrantTests
{
    // 不经过 LoadPacks 就直接使用 SerializingOptions，必须复位进程级一次性状态，并和其他会复位它的用例串行。
    public DataRegistrantTests() => SerializingOptionsTestAccessor.ResetAndInitialize();

    [Fact]
    public void CommonRegistrant_Loads_Toml_Recursively_And_Applies_The_Pack_Namespace()
    {
        using var fs = new MemoryFileSystem();
        Write(fs, "/cards/strike.toml", "id = \"strike\"\nname = \"Strike\"");
        Write(fs, "/cards/sub/defend.toml", "id = \"other:defend\"\nname = \"Defend\"");
        Write(fs, "/cards/ignored.txt", "not toml");

        var target = new RecordingRegisterTarget<TestData>();
        var registrant = new CommonTomlDataRegistrant<TestData>("/cards", data => data.Id);
        registrant.AcceptTarget(target);

        registrant.Load("my-pack", fs);

        Assert.Equal(2, target.Items.Count);
        Assert.Contains(RegKey.From("my-pack", "strike"), target.Items.Keys);
        // 显式写了命名空间的值不会被包的默认命名空间覆盖。
        Assert.Contains(RegKey.From("other", "defend"), target.Items.Keys);
        Assert.Equal("Strike", target.Items[RegKey.From("my-pack", "strike")].Name);
    }

    [Fact]
    public void CommonRegistrant_Only_Scans_The_Configured_Directory()
    {
        using var fs = new MemoryFileSystem();
        Write(fs, "/other/a.toml", "id = \"a\"\nname = \"A\"");

        var target = new RecordingRegisterTarget<TestData>();
        var registrant = new CommonTomlDataRegistrant<TestData>("/cards", data => data.Id);
        registrant.AcceptTarget(target);

        registrant.Load("pack", fs);

        Assert.Empty(target.Items);
    }

    [Fact]
    public void ListedRegistrant_Loads_Multiple_Entries_And_Scans_Nested_Directories()
    {
        using var fs = new MemoryFileSystem();
        Write(fs, "/cards/cards.toml", """
                                       [[card]]
                                       id = "strike"
                                       name = "Strike"

                                       [[card]]
                                       id = "defend"
                                       name = "Defend"
                                       """);
        Write(fs, "/cards/nested/nested.toml", """
                                                [[card]]
                                                id = "nested"
                                                name = "Nested"
                                                """);

        var target = new RecordingRegisterTarget<TestData>();
        var registrant = new ListedTomlDataRegistrant<TestData>("/cards", "card", data => data.Id);
        registrant.AcceptTarget(target);

        registrant.Load("pack", fs);

        Assert.Equal(3, target.Items.Count);
        Assert.Contains(RegKey.From("pack", "strike"), target.Items.Keys);
        Assert.Contains(RegKey.From("pack", "defend"), target.Items.Keys);
        Assert.Contains(RegKey.From("pack", "nested"), target.Items.Keys);
    }

    [Fact]
    public void Registrant_Is_A_NoOp_When_Its_Directory_Does_Not_Exist()
    {
        using var fs = new MemoryFileSystem();

        var target = new RecordingRegisterTarget<TestData>();
        var registrant = new CommonTomlDataRegistrant<TestData>("/cards", data => data.Id);
        registrant.AcceptTarget(target);

        registrant.Load("pack", fs);

        Assert.Empty(target.Items);
    }

    [Fact]
    public void Registrant_Rejects_Loading_Without_A_Target()
    {
        using var fs = new MemoryFileSystem();
        var registrant = new CommonTomlDataRegistrant<TestData>("/cards", data => data.Id);

        var ex = Assert.Throws<InvalidOperationException>(() => registrant.Load("pack", fs));

        Assert.Contains("no target", ex.Message);
    }

    [Fact]
    public void RegKeyConverter_Reads_The_Default_Namespace_And_An_Explicit_One()
    {
        var options = SerializingOptions.WithDefaultNamespace("pack");

        var withDefault = Tomlyn.TomlSerializer.Deserialize<TestData>("id = \"strike\"\nname = \"S\"", options)!;
        var explicitNs = Tomlyn.TomlSerializer.Deserialize<TestData>("id = \"other:strike\"\nname = \"S\"", options)!;

        Assert.Equal(RegKey.From("pack", "strike"), withDefault.Id);
        Assert.Equal(RegKey.From("other", "strike"), explicitNs.Id);
    }

    [Fact]
    public void RegKeyConverter_Writes_Keys_Relative_To_The_Configured_Namespace()
    {
        var options = SerializingOptions.WithDefaultNamespace("pack");

        var local = Tomlyn.TomlSerializer.Serialize(new TestData(RegKey.From("pack", "strike"), "Strike"), options);
        var foreign = Tomlyn.TomlSerializer.Serialize(new TestData(RegKey.From("other", "strike"), "Strike"), options);

        // 与默认命名空间相同时只写 key；别的命名空间要写 full name。
        Assert.Contains("id = \"strike\"", local);
        Assert.DoesNotContain("id = \"pack:", local);
        Assert.Contains("id = \"other:strike\"", foreign);
    }

    private static void Write(IFileSystem fs, string path, string content)
    {
        var filePath = new UPath(path);
        var directory = filePath.GetDirectory();
        if (!directory.IsNull && directory != UPath.Root) fs.CreateDirectory(directory);
        fs.WriteAllText(filePath, content);
    }
}

[method: TomlConstructor]
public sealed record TestData([property: TomlRequired] RegKey Id, [property: TomlRequired] string Name);

internal sealed class RecordingRegisterTarget<T> : IRegisterTarget<T>
{
    public Dictionary<RegKey, T> Items { get; } = [];

    public bool IsAvailableToRegister => true;

    public bool AddItem(RegKey key, T item)
    {
        Items.Add(key, item);
        return true;
    }

    public void AddOrOverwriteItem(RegKey key, T item) => Items[key] = item;
}
