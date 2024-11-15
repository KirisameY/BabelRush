using System.Collections.Generic;

using BabelRush.Data;

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

    public MobType Convert()
    {
        var animationSet = Registers.MobRegisters.MobAnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement);
    }

    public static IReadOnlyCollection<IModel<MobType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        IDataModel<MobType>.ParseFromSource<ModelSet>(source, out errorMessages);
}