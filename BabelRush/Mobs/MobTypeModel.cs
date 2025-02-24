using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Mobs.Actions;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Mobs;

[ModelSet("Mob")]
internal partial class MobTypeModel : IDataModel<MobType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial string AnimationSet { get; set; }
    [NecessaryProperty]
    public partial bool BlocksMovement { get; set; }
    [NecessaryProperty]
    public partial int Health { get; set; }

    public MobType Convert()
    {
        var animationSet = Registers.MobRegisters.MobAnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement, Health, MobActionStrategy.Default); //todo: 加入MobActionStrategy字段
    }

    public static IReadOnlyCollection<IModel<MobType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, MobType>(source, out errorMessages);
}