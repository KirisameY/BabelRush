using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Logging;
using KirisameLib.Core.Register;

using Table = System.Collections.Generic.IDictionary<string, object>;

namespace BabelRush.Registering;

public abstract class ResRegTool(string subPath) : AssetRegTool("res/" + subPath)
{
    public string SubPath { get; } = subPath;
    
    public abstract Task RegisterSet(IEnumerable<ResSource> source);
    public abstract Task RegisterLocalizedSet(string local, IEnumerable<ResSource> source);

    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(ResRegTool)}.{subPath}");
}

public sealed class ResRegTool<TRes, TBox>(
    string subPath, CommonRegister<TRes> defaultRegister, LocalizedRegister<TRes> localizedRegister)
    : ResRegTool(subPath)
    where TBox : IResBox<TRes, TBox>
{
    private CommonRegister<TRes> DefaultRegister { get; } = defaultRegister;
    private LocalizedRegister<TRes> LocalizedRegister { get; } = localizedRegister;

    public override async Task RegisterSet(IEnumerable<ResSource> source)
    {
        await Task.Yield();
        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start parsing");
        var boxes = ParseSet<ResSource, TBox, TRes>(source);

        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start registering");
        foreach (var box in boxes)
        {
            DefaultRegister.RegisterItem(box.Id, box.GetAsset());
        }
        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Finish registering");
    }

    public override async Task RegisterLocalizedSet(string local, IEnumerable<ResSource> source)
    {
        await Task.Yield();
        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start parsing for local: {local}");
        var boxes = ParseSet<ResSource, TBox, TRes>(source);

        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start registering for local: {local}");
        foreach (var box in boxes)
        {
            LocalizedRegister.RegisterLocalizedItem(local, box.Id, box.GetAsset());
        }
        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Finish registering for local: {local}");
    }
}