using System.Reflection;
using System.Runtime.CompilerServices;

using BabelRushR.Core.Contents;

using KirisameY.Registration.Registering;
using KirisameY.Relinq.Extensions;

using Zio;
using Zio.FileSystems;

namespace BabelRushR.Core.Data;

public static class DataRegistrationManager
{
    public static TRegistrant Attach<TRegistrant>(TRegistrant registrant)
        where TRegistrant : DataRegistrant
    {
        _dataRegistrants.Add(registrant);
        return registrant;
    }

    private static void InitializeRegisters()
    {
        var selfAsm = Assembly.GetAssembly(typeof(DataRegistrationManager))!;
        AppDomain.CurrentDomain
                 .GetAssemblies()
                 .Where(asm => asm.GetReferencedAssemblies().Contains(selfAsm.GetName()))
                 .Prepend(selfAsm)
                 .SelectMany(assembly => assembly.DefinedTypes)
                 .Where(type => type.GetCustomAttribute<DataRegisterInitializeAttribute>() is not null)
                 .ForEach(type => RuntimeHelpers.RunClassConstructor(type.TypeHandle));
    }

    private static readonly List<DataRegistrant> _dataRegistrants = [];

    private static readonly DoneEventSource _doneSource = new();
    public static IRegisterDoneEventSource DoneSource => _doneSource;

    private static event Action? RegisterDone;

    private class DoneEventSource : IRegisterDoneEventSource
    {
        event Action? IRegisterDoneEventSource.RegisterDone
        {
            add => RegisterDone += value;
            remove => RegisterDone -= value;
        }
    }

    [AutoContentsLoader("/data")]
    private class DataLoader : IContentsLoader<DataLoader>, IDisposable
    {
        public void Load(string packageName, FileSystemEntry entry)
        {
            if (entry is not DirectoryEntry directory) return;
            // 只是包内 /data 的视图：释放它不能把整包的文件系统一起释放，否则同一包里后续的加载器会拿到已释放的 FS。
            using var fs = new SubFileSystem(directory.FileSystem, directory.Path, false);
            _dataRegistrants.ForEach(reg => reg.Load(packageName, fs));
        }

        public static DataLoader Create()
        {
            InitializeRegisters();
            return new();
        }

        public void Dispose()
        {
            RegisterDone?.Invoke();
        }
    }
}