using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Data;

using KirisameLib.Core.Logging;
using KirisameLib.Core.Register;

using Table = System.Collections.Generic.IDictionary<string, object>;

namespace BabelRush.Registering;

public abstract class DataRegTool(string subPath) : AssetRegTool("data/" + subPath)
{
    protected string SubPath { get; } = subPath;
    protected static ConcurrentDictionary<string, TaskCompletionSource> RegisteringTasks { get; } = [];

    public override Task RegisterLocalizedSet(string local, object source)
    {
        throw new InvalidOperationException("Cannot register localized data");
    }


    #region Getters

    public static DataRegTool Get<TModel, TData>(string subPath, CommonRegister<TModel> register, params string[] waitFor)
        where TData : IData<TModel, TData> => new SpecificDataRegTool<TModel, TData>(subPath, register, waitFor);

    #endregion


    #region Specific Class

    private sealed class SpecificDataRegTool<TModel, TData>
        (string subPath, CommonRegister<TModel> register, IEnumerable<string> waitFor)
        : DataRegTool(subPath)
        where TData : IData<TModel, TData>
    {
        private CommonRegister<TModel> Register { get; } = register;
        private ImmutableArray<string> WaitForRegisters { get; } = [..waitFor];


        public override async Task RegisterSet(object source)
        {
            await Task.Yield();
            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start parsing");
            var items = ParseSet<Table, TData>(TData.FromEntry, source);

            await Task.WhenAll(WaitForRegisters.Select(s => RegisteringTasks.GetOrAdd(s, _ => new()).Task));
            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start registering");
            foreach (var item in items)
            {
                Register.RegisterItem(item.Id, item.ToModel());
            }

            RegisteringTasks.GetOrAdd(SubPath, _ => new()).SetResult();
            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Finish registering");
        }
    }

    #endregion


    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(DataRegTool)}.{subPath}");
}