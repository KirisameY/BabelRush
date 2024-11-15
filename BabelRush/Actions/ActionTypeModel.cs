using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using JetBrains.Annotations;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Actions;

[ModelSet("Action")]
internal partial class ActionTypeModel : IDataModel<ActionType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial string TargetPattern { get; set; }
    [UsedImplicitly]
    public List<string> ActionItems { get; set; } = [];

    public ActionType Convert()
    {
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => ActionRegisters.ActionSteps.GetItem(id));
        return new(Id, targetPattern, actionItems);
    }

    public static IReadOnlyCollection<IModel<ActionType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        IDataModel<ActionType>.ParseFromSource<ModelSet>(source, out errorMessages);
}