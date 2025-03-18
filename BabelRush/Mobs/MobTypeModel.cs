using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Mobs.Actions;

using Tomlyn.Syntax;

namespace BabelRush.Mobs;

[ModelSet("Mob")]
internal partial class MobTypeModel : IDataModel<MobType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    public string? AnimationSet { get; set; } = null;
    [NecessaryProperty]
    public partial bool BlocksMovement { get; set; }
    [NecessaryProperty]
    public partial int Health { get; set; }
    public MobActionStrategyModel? ActionStrategy { get; set; }

    public MobType Convert()
    {
        AnimationSet ??= Id;
        return new(Id, AnimationSet, BlocksMovement, Health, ActionStrategy?.Convert() ?? MobActionStrategy.Default);
    }

    public static IReadOnlyCollection<IModel<MobType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, MobType>(source, out errorMessages);
}