using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registering.Parsing;
using BabelRush.Registers;

using KirisameLib.Core.Extensions;

namespace BabelRush.Actions;

public record ActionTypeBox(string Id, string TargetPattern, ImmutableArray<string> ActionItems) : IDataBox<ActionType, ActionTypeBox>
{
    public ActionType GetAsset()
    {
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => ActionRegisters.ActionSteps.GetItem(id));
        return new(Id, targetPattern, actionItems);
    }

    public static ActionTypeBox FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var targetPattern = (string)entry["target_pattern"];

        var actionItems =
            (entry.GetOrDefault("action_items") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];

        return new ActionTypeBox(id, targetPattern, [..actionItems]);
    }
}