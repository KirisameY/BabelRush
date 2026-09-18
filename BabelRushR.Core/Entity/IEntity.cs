using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Scenery;

using KirisameY.Numeric;

namespace BabelRushR.Core.Entity;

public interface IEntity : ISceneObject, IUpdatable
{
    /// <summary>当前生命值，应被钳制在 [0, <see cref="MaxHP"/>] 内。</summary>
    int HP { get; set; }

    /// <summary>生命上限。数值变化时应即时重新钳制 <see cref="HP"/>。</summary>
    IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxHP { get; }

    /// <summary>当前实体是否存活</summary>
    bool IsAlive { get; }
}
