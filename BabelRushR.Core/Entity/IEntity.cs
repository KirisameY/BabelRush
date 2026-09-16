using BabelRushR.Core.Common;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Entity;

public interface IEntity : ISceneObject, IUpdatable
{
    int HP { get; set; }
    int MaxHP { get; }

    /// <summary>
    /// 由 <see cref="HP"/> 推导，不是独立维护的状态。
    /// </summary>
    bool IsAlive { get; }
}
