using System;
using System.Threading.Tasks;

using BabelRush.Data;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Logging;
using KirisameLib.Core.Register;

using Table = System.Collections.Generic.IDictionary<string, object>;

namespace BabelRush.Registering;

public abstract class ResRegTool(string subPath) : AssetRegTool("res/" + subPath)
{
    protected static Func<ResData, (string Id, TRes Res)> ParseIdAndValue<TRes>(Func<FileAccess?, Table?, TRes> parse) =>
        data => (data.Id, parse(data.File, data.Data));


    #region Getters

    public static ResRegTool Get<TRes>(string subPath, Func<FileAccess?, Table?, TRes> parse,
                                       CommonRegister<TRes> defaultRegister, LocalizedRegister<TRes> localizedRegister) =>
        new SpecificResRegTool<TRes>(subPath, parse, defaultRegister, localizedRegister);

    #endregion


    #region Specific Class

    private sealed class SpecificResRegTool<TRes>(
        string subPath, Func<FileAccess?, Table?, TRes> parse,
        CommonRegister<TRes> defaultRegister, LocalizedRegister<TRes> localizedRegister)
        : ResRegTool(subPath)
    {
        private CommonRegister<TRes> DefaultRegister { get; } = defaultRegister;
        private LocalizedRegister<TRes> LocalizedRegister { get; } = localizedRegister;

        public override async Task RegisterSet(object source)
        {
            await Task.Yield();
            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start parsing");
            var items = ParseSet(ParseIdAndValue(parse), source);

            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Start registering");
            foreach (var (id, res) in items)
            {
                DefaultRegister.RegisterItem(id, res);
            }
            Logger.Log(LogLevel.Info, nameof(RegisterSet), "Finish registering");
        }

        public override async Task RegisterLocalizedSet(string local, object source)
        {
            await Task.Yield();
            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start parsing for local: {local}");
            var items = ParseSet(ParseIdAndValue(parse), source);

            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start registering for local: {local}");
            foreach (var (id, res) in items)
            {
                LocalizedRegister.RegisterLocalizedItem(local, id, res);
            }
            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Finish registering for local: {local}");
        }
    }

    #endregion


    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(ResRegTool)}.{subPath}");
}