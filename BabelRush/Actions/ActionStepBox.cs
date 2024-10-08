using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.Parsing;
using BabelRush.Registers;

namespace BabelRush.Actions;

public record ActionStepBox(string Id, string ActionDelegate) : IDataBox<ActionStep, ActionStepBox>
{
    public ActionStep GetAsset()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        return new(actionDelegate);
    }

    public static ActionStepBox FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var actionDelegate = (string)entry["action_delegate"];
        return new(id, actionDelegate);
    }
}