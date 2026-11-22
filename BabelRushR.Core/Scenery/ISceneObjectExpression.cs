using Tomlyn.Serialization;

namespace BabelRushR.Core.Scenery;

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
public interface ISceneObjectExpression
{
    ISceneObject CreateObject();
}