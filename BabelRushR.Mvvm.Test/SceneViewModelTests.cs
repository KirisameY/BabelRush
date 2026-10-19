using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class SceneViewModelTests
{
    // 场景不再自带事件总线，本类只关心 Entities 的映射，不挂到 GamePlay 上也够用。
    private static CommonScene NewScene() => new();

    [Fact]
    public void Exposes_The_Source_Scene()
    {
        var scene = NewScene();

        var viewModel = new SceneViewModel(scene);

        Assert.Same(scene, viewModel.SourceScene);
    }

    [Fact]
    public void Entities_Map_The_Scene_Entities_To_ViewModels()
    {
        var scene = NewScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        var viewModel = new SceneViewModel(scene);

        var entityViewModel = Assert.Single(viewModel.Entities);
        Assert.Same(entity, entityViewModel.SourceEntity);
    }

    [Fact]
    public void Entities_Are_Lazily_Created_And_Stable()
    {
        var viewModel = new SceneViewModel(NewScene());

        var entities = viewModel.Entities;

        Assert.Same(entities, viewModel.Entities);
    }
}
