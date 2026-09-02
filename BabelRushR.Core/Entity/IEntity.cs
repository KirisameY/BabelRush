using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Entity;

public interface IEntity : ISceneObject
{
    int HP { get; set; }
    int MaxHP { get; }
}