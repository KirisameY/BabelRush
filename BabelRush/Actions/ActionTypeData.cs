using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using KirisameLib.Core.Extensions;

namespace BabelRush.Actions;

public record ActionTypeData(string Id, string TargetPattern, ImmutableArray<string> ActionItems) : IData<ActionTypeData>
{
    public ActionType ToActionType()
    {
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => MiscRegisters.ActionItems.GetItem(id));
        return new(Id, targetPattern, actionItems);
    }

    public static ActionTypeData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var targetPattern = (string)entry["target_pattern"];

        var actionItems =
            (entry.GetOrDefault("action_items") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];

        return new ActionTypeData(id, targetPattern, [..actionItems]);
    }
}