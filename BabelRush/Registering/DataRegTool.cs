using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

using KirisameLib.Core.Logging;
using KirisameLib.Core.Register;

using Table = System.Collections.Generic.IDictionary<string, object>;

namespace BabelRush.Registering;

public abstract class DataRegTool(string subPath) : AssetRegTool("data/" + subPath)
{
    protected string SubPath { get; } = subPath;
    protected static ConcurrentDictionary<string, TaskCompletionSource> RegisteringTasks { get; } = [];

    public override Task RegisterLocalizedSet(string local, IEnumerable source)
    {
        throw new InvalidOperationException("Cannot register localized data");
    }

    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(DataRegTool)}.{subPath}");
}

public sealed class DataRegTool<TData, TBox>
    (string subPath, CommonRegister<TData> register, params string[] waitFor)
    : DataRegTool(subPath)
    where TBox : IDataBox<TData, TBox>
{
    private CommonRegister<TData> Register { get; } = register;
    private ImmutableArray<string> WaitForRegisters { get; } = [..waitFor];


    public override async Task RegisterSet(IEnumerable source)
    {
        await Task.Yield();
        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start parsing");
        var boxes = ParseSet<Table, TBox, TData>(source);

        await Task.WhenAll(WaitForRegisters.Select(s => RegisteringTasks.GetOrAdd(s, _ => new()).Task));
        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start registering");
        foreach (var box in boxes)
        {
            Register.RegisterItem(box.Id, box.GetAsset());
        }

        RegisteringTasks.GetOrAdd(SubPath, _ => new()).SetResult();
        Logger.Log(LogLevel.Info, nameof(RegisterSet), "Finish registering");
    }
}