using BabelRushR.Core.Entity;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public static class SceneViewModelExtensions
{
    extension(SceneViewModel scene)
    {
        public EntityViewModel? Find(IEntity entity) => scene.Entities.FirstOrDefault(e => e.SourceEntity == entity);
    }
}