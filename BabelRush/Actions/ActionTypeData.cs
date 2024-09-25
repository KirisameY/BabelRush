using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Registers;

using KirisameLib.Core.Extensions;

using Tomlyn.Model;

namespace BabelRush.Actions;

public record ActionTypeData(string Id, string TargetPattern, IEnumerable<string> ActionItems) : ITomlData<ActionTypeData>
{
    public ActionType ToActionType()
    {
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => MiscRegisters.ActionItems.GetItem(id));
        return new(Id, targetPattern, actionItems);
    }

    public static ActionTypeData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var targetPattern = (string)entry["target_pattern"];

        var actionItems =
            (entry.GetOrDefault("action_items") as TomlArray)?.Select(x => x!.ToString()!) ?? [];

        return new ActionTypeData(id, targetPattern, actionItems);
    }
}