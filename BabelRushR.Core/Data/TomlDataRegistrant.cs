using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;
using KirisameY.Relinq.Extensions;

using Tomlyn;

using Zio;

namespace BabelRushR.Core.Data;

public class CommonTomlDataRegistrant<T>(UPath path, Func<T, RegKey> idGetter) : DataRegistrant<T>
{
    protected override void Load(string packNameSpace, IFileSystem packFs, IRegisterTarget<T> target)
    {
        if (packFs.TryGetFileSystemEntry(path) is not DirectoryEntry directory) return;
        var opt = GetOptions(packNameSpace);
        var data = directory.EnumerateFiles("*.toml", SearchOption.AllDirectories).Select(tomlFile =>
        {
            using var tomlStream = tomlFile.Open(FileMode.Open, FileAccess.Read);
            return TomlSerializer.Deserialize<T>(tomlStream, opt);
        });
        foreach (var entry in data)
        {
            // todo: 异常处理、日志
            target.AddItem(idGetter.Invoke(entry!), entry!);
        }
    }
}

public class ListedTomlDataRegistrant<T>(UPath path, string rootKey, Func<T, RegKey> idGetter) : DataRegistrant<T>
{
    protected override void Load(string packNameSpace, IFileSystem packFs, IRegisterTarget<T> target)
    {
        if (packFs.TryGetFileSystemEntry(path) is not DirectoryEntry directory) return;
        var opt = GetOptions(packNameSpace) with
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            RootValueKeyName = rootKey,
        };
        var data = directory.EnumerateFiles("*.toml").Select(tomlFile =>
        {
            using var tomlStream = tomlFile.Open(FileMode.Open, FileAccess.Read);
            return TomlSerializer.Deserialize<T[]>(tomlStream, opt)!;
        }).Flatten();
        foreach (var entry in data)
        {
            // todo: 异常处理、日志
            target.AddItem(idGetter.Invoke(entry!), entry!);
        }
    }
}