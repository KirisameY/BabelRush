using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registers;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Actions;

[ModelSet("ActionStep")]
internal partial class ActionStepModel : IDataModel<ActionStep>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial string ActionDelegate { get; set; }
    
    public ActionStep Convert()
    {
        var actionDelegate = InCodeRegisters.ActionDelegates.GetItem(ActionDelegate);
        
        return new(actionDelegate);
    }

    public static IReadOnlyCollection<IModel<ActionStep>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, ActionStep>(source, out errorMessages);
}