namespace BabelRush.Scenery;

public interface ISceneObject
{
    double RoomPosition { get; set; }
    double ScenePosition { get; set; }

    Scene Scene { get; set; }
    Room Room { get; }
}