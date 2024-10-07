using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registers;

namespace BabelRush.Actions;

public record ActionStepData(string Id, string ActionDelegate) : IData<ActionStep, ActionStepData>
{
    public ActionStep ToModel()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        return new(actionDelegate);
    }

    public static ActionStepData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var actionDelegate = (string)entry["action_delegate"];
        return new(id, actionDelegate);
    }
}