using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Registers;

using KirisameLib.Core.Extensions;

using Tomlyn.Model;

namespace BabelRush.Actions;

public record ActionTypeData(string Id, string TargetPattern, IEnumerable<string> ActionItems) : ITomlData<ActionType, ActionTypeData>
{
    public ActionType ToModel()
    {
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => MiscRegisters.ActionItems.GetItem(id));
        return new(Id, targetPattern, actionItems);
    }

    private static ActionTypeData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var targetPattern = (string)entry["target_pattern"];

        var actionItems =
            (entry.GetOrDefault("action_items") as TomlArray)?.Select(x => x!.ToString()!) ?? [];

        return new ActionTypeData(id, targetPattern, actionItems);
    }

    //Todo:这段代码似乎是重复的，能不能想办法封装一下呢——
    public static IEnumerable<ParseResult<ActionTypeData>> FromTomlTable(TomlTable table)
    {
        if (!table.TryGetValue("actions", out var actions)) yield break;
        if (actions is not TomlTableArray actionList) yield break;
        foreach (var actionEntry in actionList)
        {
            ParseResult<ActionTypeData> result;
            try
            {
                result = new(FromTomlEntry(actionEntry));
            }
            catch (Exception e)
            {
                result = new(e);
            }
            yield return result;
        }
    }
}