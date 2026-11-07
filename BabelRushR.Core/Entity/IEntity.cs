using BabelRushR.Core.Common;
using BabelRushR.Core.Entity.Health;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Scenery;

using KirisameY.Numeric;
using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity;

public interface IEntity : ISceneObject, IUpdatable
{
    RegKey DisplayKey { get; }

    [Immutable] IEntityHealth Health { get; }

    /// <summary>当前实体是否存活</summary>
    bool IsAlive { get; }
}