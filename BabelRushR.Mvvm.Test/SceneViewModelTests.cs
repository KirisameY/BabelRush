using System.Collections.Specialized;

using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.ViewModels;

using KirisameY.EventBus.Bus;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Mvvm.Test;

public class SceneViewModelTests
{
    private static CommonScene NewScene() => new(new SimpleEventBus());

    [Fact]
    public void Picks_Up_Entities_Already_In_The_Scene()
    {
        var scene = NewScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        var viewModel = new SceneViewModel(scene);

        Assert.Same(viewModel.Find(entity), Assert.Single(viewModel.Entities));
    }

    [Fact]
    public void Adding_An_Entity_Adds_It_To_Entities()
    {
        var scene = NewScene();
        var viewModel = new SceneViewModel(scene);

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        Assert.Same(viewModel.Find(entity), Assert.Single(viewModel.Entities));
    }

    [Fact]
    public void Adding_An_Entity_Raises_A_Collection_Change()
    {
        var scene = NewScene();
        var viewModel = new SceneViewModel(scene);
        var changes = NotificationRecorder.CollectionChanges(viewModel.Entities);

        scene.AddEntity(new TestEntity(maxHP: 5));

        var change = Assert.Single(changes);
        Assert.Equal(NotifyCollectionChangedAction.Add, change.Action);
    }

    [Fact]
    public void Removing_An_Entity_Removes_It_From_Entities()
    {
        var scene = NewScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);
        var viewModel = new SceneViewModel(scene);

        scene.RemoveEntity(entity);

        Assert.Empty(viewModel.Entities);
        Assert.Null(viewModel.Find(entity));
    }

    [Fact]
    public void Find_Returns_Null_For_An_Unknown_Entity()
    {
        var viewModel = new SceneViewModel(NewScene());

        Assert.Null(viewModel.Find(new TestEntity(maxHP: 5)));
    }

    [Fact]
    public void Removing_An_Entity_Disposes_Its_ViewModel()
    {
        var scene = NewScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);
        var viewModel = new SceneViewModel(scene);
        var notified = NotificationRecorder.PropertyNames(viewModel.Find(entity)!);

        scene.RemoveEntity(entity);
        entity.HP = 1;

        Assert.Empty(notified);
    }

    [Fact]
    public void Replacing_An_Entity_Swaps_The_ViewModel()
    {
        var scene = NewScene();
        var oldEntity = new TestEntity(maxHP: 5);
        scene.AddEntity(oldEntity);
        var viewModel = new SceneViewModel(scene);
        var notified = NotificationRecorder.PropertyNames(viewModel.Find(oldEntity)!);

        var newEntity = new TestEntity(maxHP: 8);
        scene.Entities[0] = newEntity;

        Assert.Null(viewModel.Find(oldEntity));
        Assert.Same(viewModel.Find(newEntity), Assert.Single(viewModel.Entities));

        oldEntity.HP = 1;
        Assert.Empty(notified);
    }

    [Fact]
    public void Sorting_The_Scene_Does_Not_Disturb_Entities()
    {
        var scene = NewScene();
        var first = new TestEntity(maxHP: 5);
        var second = new TestEntity(maxHP: 5);
        scene.AddEntity(first);
        scene.AddEntity(second);
        var viewModel = new SceneViewModel(scene);
        var firstViewModel = viewModel.Find(first)!;
        var secondViewModel = viewModel.Find(second)!;

        // 映射表本身无序，整表重排对它是空操作。
        ((NotifiableList<IEntity>)scene.Entities).Sort(Comparer<IEntity>.Create(static (_, _) => 0));

        Assert.Contains(firstViewModel, viewModel.Entities);
        Assert.Contains(secondViewModel, viewModel.Entities);
        var count = viewModel.Entities.Count;
        Assert.Equal(2, count);
    }

    [Fact]
    public void Disposed_ViewModel_Stops_Tracking_The_Scene()
    {
        var scene = NewScene();
        var viewModel = new SceneViewModel(scene);

        viewModel.Dispose();
        scene.AddEntity(new TestEntity(maxHP: 5));

        Assert.Empty(viewModel.Entities);
    }
}
