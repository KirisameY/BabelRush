using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using BabelRush.Registering.FileLoading;

using KirisameLib.Event;
using KirisameLib.Extensions;
using KirisameLib.FileSys;
using KirisameLib.Logging;

namespace BabelRush.Registering;

internal static class RegisterManager
{
    static RegisterManager()
    {
        //todo: 重写以支持加载更多程序集
        typeof(RegisterManager)
           .Assembly.GetTypes()
           .Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(RegisterContainerAttribute)))
           .ForEach(t => RuntimeHelpers.RunClassConstructor(t.TypeHandle));
    }

    public static void LoadCommonAssets()
    {
        Game.LoadEventBus.Publish(new CommonRegisterStartEvent());
        var loader = new CommonFileLoader();
        LoadAssets(loader);
        //todo: 手动注册
        RegisterEventSource.TriggerCommonRegisterDone();
        Game.LoadEventBus.Publish(new CommonRegisterDoneEvent());
    }

    public static void LoadLocalAssets(string local)
    {
        Game.LoadEventBus.Publish(new LocalRegisterStartEvent(local));
        var loader = new LocalFileLoader(local);
        LoadAssets(loader);
        //todo: 手动注册
        RegisterEventSource.TriggerLocalRegisterDone();
        Game.LoadEventBus.Publish(new LocalRegisterDoneEvent(local));
    }

    private static void LoadAssets(FileLoader loader) //todo: 把获取资源包目录的逻辑提取到外面
    {
    #if TOOLS
        var root = new DirectoryInfo("./assets").ToReadableDirectory();
        if (!root.Exists)
        {
            Logger.Log(LogLevel.Error, "LoadingAssets", "Assets directory not found");
            return;
        }
    #else
        var zipInfo = new FileInfo("./assets.zip");
        if (!zipInfo.Exists)
        {
            Logger.Log(LogLevel.Error, "LoadingAssets", "Assets zip file not found");
            return;
        }
        using var zip = ZipFile.OpenRead(zipInfo.FullName);
        var root = zip.ToReadableDirectory(zipInfo.FullName);
    #endif

        Stack<(IReadableDirectory dir, IEnumerator<IReadableDirectory> children)> dirStack = new();
        // ReSharper disable once GenericEnumeratorNotDisposed
        dirStack.Push((root, root.Directories.GetEnumerator()));
        using MemoryStream buffer = new();
        while (dirStack.TryPeek(out var info))
        {
            var (dir, children) = info;
            if (children.MoveNext())
            {
                loader.EnterDirectory(children.Current.Name);
                // ReSharper disable once GenericEnumeratorNotDisposed
                dirStack.Push((children.Current, children.Current.Directories.GetEnumerator()));
                continue;
            }

            foreach (var file in dir.Files)
            {
                buffer.SetLength(0);
                buffer.Position = 0;
                using var fileStream = file.OpenRead();
                fileStream.CopyTo(buffer);
                loader.LoadFile(file.Name, buffer.ToArray());
            }

            loader.ExitDirectory();
            children.Dispose();
            dirStack.Pop();
        }
    }


    // Logging
    private static Logger Logger => Game.LogBus.GetLogger("Root");
}