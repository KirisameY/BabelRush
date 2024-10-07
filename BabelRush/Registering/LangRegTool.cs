using System;
using System.Threading.Tasks;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Logging;

using Pair = System.Collections.Generic.KeyValuePair<string, object>;

namespace BabelRush.Registering;

public abstract class LangRegTool(string subPath) : AssetRegTool("lang/" + subPath)
{
    public override Task RegisterSet(object source)
    {
        throw new InvalidOperationException("Cannot register non-localized lang");
    }

    protected static Func<Pair, (string Id, TData Data)> ParseIdAndValue<TSource, TData>(Func<TSource, TData> parse) =>
        pair => (pair.Key, parse((TSource)pair.Value));


    #region Getters

    public static LangRegTool Get<TLang, TSource>(string subPath, Func<TSource, TLang> parse, LocalizedRegister<TLang> register)
        => new SpecificLangRegTool<TSource, TLang>(subPath, parse, register);

    public static LangRegTool Get<TLang>(string subPath, LocalizedRegister<TLang> register)
        => new SpecificLangRegTool<TLang, TLang>(subPath, l => l, register);

    #endregion


    #region Specific Class

    private class SpecificLangRegTool<TSource, TLang>(string subPath, Func<TSource, TLang> parse, LocalizedRegister<TLang> register)
        : LangRegTool(subPath)
    {
        private LocalizedRegister<TLang> Register { get; } = register;

        public override async Task RegisterLocalizedSet(string local, object source)
        {
            await Task.Yield();
            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start parsing for local: {local}");
            var items = ParseSet(ParseIdAndValue(parse), source);

            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start registering for local: {local}");
            foreach (var (id, item) in items)
            {
                Register.RegisterLocalizedItem(local, id, item);
            }
            Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Finish registering for local: {local}");
        }
    }

    #endregion


    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(LangRegTool)}.{subPath}");
}