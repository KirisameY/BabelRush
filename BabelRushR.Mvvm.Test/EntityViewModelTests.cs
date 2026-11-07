using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class EntityViewModelTests
{
    [Fact]
    public void Exposes_The_Entity_State()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.Health.HP = 7;
        entity.Position = 3.5;

        var viewModel = new EntityViewModel(entity);

        Assert.Equal(7, viewModel.HealthBar.HP);
        Assert.Equal(10, viewModel.HealthBar.MaxHP);
        Assert.Equal(0.7, viewModel.HealthBar.HPRatio, 10);
        Assert.True(viewModel.IsAlive);
        Assert.Equal(3.5, viewModel.Position);
    }

    [Fact]
    public void HP_Change_Notifies_IsAlive()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.Health.HP = 5;
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.HP = 3;

        Assert.True(viewModel.IsAlive);
        Assert.Contains(nameof(EntityViewModel.IsAlive), notified);
    }

    [Fact]
    public void IsAlive_Change_Notifies_IsAlive()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.Health.HP = 1;
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.HP = 0;

        Assert.False(viewModel.IsAlive);
        Assert.Contains(nameof(EntityViewModel.IsAlive), notified);
    }

    [Fact]
    public void Position_Change_Notifies_Position()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Position = 1.5;

        Assert.Equal(1.5, viewModel.Position);
        Assert.Contains(nameof(EntityViewModel.Position), notified);
    }
}
