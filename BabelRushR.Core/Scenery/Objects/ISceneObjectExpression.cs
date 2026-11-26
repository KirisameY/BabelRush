using Tomlyn.Serialization;

namespace BabelRushR.Core.Scenery.Objects;

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
public interface ISceneObjectExpression
{
    ISceneObject CreateObject();
}