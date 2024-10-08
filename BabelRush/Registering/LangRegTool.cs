using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Logging;

using Pair = System.Collections.Generic.KeyValuePair<string, object>;

namespace BabelRush.Registering;

public abstract class LangRegTool(string subPath) : AssetRegTool("lang/" + subPath)
{
    public override Task RegisterSet(IEnumerable source)
    {
        throw new InvalidOperationException("Cannot register non-localized lang");
    }

    //Logging
    protected override Logger Logger { get; } = LogManager.GetLogger($"{nameof(LangRegTool)}.{subPath}");
}

public class LangRegTool<TLang, TBox>(string subPath, LocalizedRegister<TLang> register)
    : LangRegTool(subPath)
    where TBox : ILangBox<TLang, TBox>
{
    private LocalizedRegister<TLang> Register { get; } = register;

    public override async Task RegisterLocalizedSet(string local, IEnumerable source)
    {
        await Task.Yield();
        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start parsing for local: {local}");
        var boxes = ParseSet<Pair, TBox, TLang>(source);

        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Start registering for local: {local}");
        foreach (var box in boxes)
        {
            Register.RegisterLocalizedItem(local, box.Id, box.GetAsset());
        }
        Logger.Log(LogLevel.Info, nameof(RegisterLocalizedSet), $"Finish registering for local: {local}");
    }
}