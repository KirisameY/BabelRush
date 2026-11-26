using BabelRushR.Core.Common;

namespace BabelRushR.Core.Scenery.Objects;

public class SceneObjectBase : ObservableObject, ISceneObject
{
    public double Position
    {
        get;
        set => SetProperty(ref field, value);
    }

    private IScene? _scene;
    public IScene? Scene => _scene;


    protected virtual void Attached(IScene to) { }

    public void AttachTo(IScene scene)
    {
        if (Interlocked.CompareExchange(ref _scene, scene, null) is not null)
            throw new InvalidOperationException("Object already attached to a scene!");
        Attached(scene);
    }

    protected virtual void Unattached(IScene from) { }

    public void UnattachFrom(IScene scene)
    {
        if (Interlocked.CompareExchange(ref _scene, null, scene) is var oldScene && oldScene != scene)
        {
            throw new InvalidOperationException(oldScene switch
            {
                null     => "Object has not attached to any scene yet!",
                not null => "Object has attached to another scene!"
            });
        }
        Unattached(scene);
    }
}