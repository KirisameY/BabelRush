using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using JetBrains.Annotations;

using Tomlyn.Syntax;

namespace BabelRush.Actions;

[ModelSet("Action")]
internal partial class ActionTypeModel : IDataModel<ActionType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    public string? Icon { get; set; } = null;
    [NecessaryProperty]
    public partial string TargetPattern { get; set; }
    [UsedImplicitly]
    public List<string> ActionItems { get; set; } = [];

    public ActionType Convert()
    {
        Icon ??= Id;
        var targetPattern = Actions.TargetPattern.FromString(TargetPattern);
        var actionItems = ActionItems.Select(id => ActionRegisters.ActionSteps.GetItem(id));
        return new(Id, Icon, targetPattern, actionItems);
    }

    public static IReadOnlyCollection<IModel<ActionType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, ActionType>(source, out errorMessages);
}