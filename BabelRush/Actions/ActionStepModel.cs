using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registers;

namespace BabelRush.Actions;

public record ActionStepModel(string Id, string ActionDelegate) : IDataModel<ActionStep>
{
    public ActionStep Convert()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        return new(actionDelegate);
    }

    public static ActionStepModel FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var actionDelegate = (string)entry["action_delegate"];
        return new(id, actionDelegate);
    }

    public static IModel<ActionStep> FromSource(IDictionary<string, object> source) => FromEntry(source);
}