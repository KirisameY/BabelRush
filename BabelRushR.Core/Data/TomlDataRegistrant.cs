using BabelRushR.Core.Common;
using BabelRushR.Core.Common.Serializing;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

using Tomlyn;

using Zio;

using ZLogger;

namespace BabelRushR.Core.Data;

public abstract class TomlDataRegistrant<T>(UPath path, Func<T, RegKey> idGetter) : DataRegistrant<T>
{
    protected sealed override void Load(string packNameSpace, IFileSystem packFs, IRegisterTarget<T> target)
    {
        if (packFs.TryGetFileSystemEntry(path) is not DirectoryEntry directory) return;
        var logger = Logging.Logger<TomlDataRegistrant<T>>();
        var opt = GetOptions(packNameSpace);
        var data = directory.EnumerateFiles("*.toml", SearchOption.AllDirectories).SelectMany(tomlFile =>
        {
            try
            {
                using var tomlStream = tomlFile.Open(FileMode.Open, FileAccess.Read);
                return LoadFromStream(tomlStream, opt);
            }
            catch (TomlException e)
            {
                logger.ZLogError(
                    $"Exception when deserializing toml data in file {tomlFile.Path} of pack {packNameSpace}: {e.Message}"
                );
                return [];
            }
        });
        foreach (var entry in data)
        {
            target.AddOrOverwriteItem(idGetter.Invoke(entry!), entry!);
        }
    }

    protected abstract IEnumerable<T> LoadFromStream(Stream tomlStream, TomlSerializerOptions options);
    protected virtual TomlSerializerOptions GetOptions(string packNameSpace) => SerializingOptions.WithDefaultNamespace(packNameSpace);
}

public sealed class CommonTomlDataRegistrant<T>(UPath path, Func<T, RegKey> idGetter) : TomlDataRegistrant<T>(path, idGetter)
{
    protected override IEnumerable<T> LoadFromStream(Stream tomlStream, TomlSerializerOptions options)
    {
        return [TomlSerializer.Deserialize<T>(tomlStream, options)!];
    }
}

public sealed class ListedTomlDataRegistrant<T>(UPath path, string rootKey, Func<T, RegKey> idGetter) : TomlDataRegistrant<T>(path, idGetter)
{
    protected override IEnumerable<T> LoadFromStream(Stream tomlStream, TomlSerializerOptions options)
    {
        return TomlSerializer.Deserialize<IReadOnlyList<T>>(tomlStream, options)!;
    }

    protected override TomlSerializerOptions GetOptions(string packNameSpace)
    {
        return base.GetOptions(packNameSpace)with
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            RootValueKeyName = rootKey,
        };
    }
}