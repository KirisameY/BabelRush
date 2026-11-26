using System.ComponentModel;

namespace BabelRushR.Core.Scenery.Objects;

public interface ISceneObject : INotifyPropertyChanged
{
    double Position { get; set; }

    IScene? Scene { get; }

    void AttachTo(IScene scene);
    void UnattachFrom(IScene scene);
}