using BabelRushR.Core.Numeric;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class HealthBarViewModelTests
{
    [Fact]
    public void Exposes_The_Entity_Health()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.Health.HP = 7;
        entity.Health.Shield = 3;
        entity.Health.Block = 2;

        var viewModel = new HealthBarViewModel(entity.Health);

        Assert.Equal(7, viewModel.HP);
        Assert.Equal(10, viewModel.MaxHP);
        Assert.Equal(0.7, viewModel.HPRatio, 10);
        Assert.Equal(3, viewModel.Shield);
        Assert.Equal(0.3, viewModel.ShieldRatio, 10);
        Assert.Equal(2, viewModel.Block);
    }

    // commented since max hp cannot be zero
    // [Fact]
    // public void HPRatio_Is_Zero_When_MaxHP_Is_Zero()
    // {
    //     var entity = new TestEntity(maxHP: 0);
    //
    //     var viewModel = new HealthBarViewModel(entity.Health);
    //
    //     Assert.Equal(0, viewModel.MaxHP);
    //     Assert.Equal(0.0, viewModel.HPRatio);
    // }

    [Fact]
    public void HP_Change_Notifies_HP_And_HPRatio()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.Health.HP = 5;
        var viewModel = new HealthBarViewModel(entity.Health);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.HP = 3;

        Assert.Equal(3, viewModel.HP);
        Assert.Contains(nameof(HealthBarViewModel.HP), notified);
        Assert.Contains(nameof(HealthBarViewModel.HPRatio), notified);
    }

    [Fact]
    public void MaxHP_Change_Notifies_MaxHP_HPRatio_And_ShieldRatio()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new HealthBarViewModel(entity.Health);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.MaxHP.AddModifier(Modifier.Of(CommonNumericModifyOrder.BaseAdd, static (ref double v) => v += 10));

        Assert.Equal(20, viewModel.MaxHP);
        Assert.Equal(0.5, viewModel.HPRatio, 10);

        // 不比对顺序：上限变化会连带触发 HP 重钳制，Core 那边的通知次序不是本测试要锁定的东西。
        Assert.Contains(nameof(HealthBarViewModel.MaxHP), notified);
        Assert.Contains(nameof(HealthBarViewModel.HPRatio), notified);
        Assert.Contains(nameof(HealthBarViewModel.ShieldRatio), notified);
    }

    [Fact]
    public void Shield_Change_Notifies_Shield_And_ShieldRatio()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new HealthBarViewModel(entity.Health);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.Shield = 2;

        Assert.Equal(2, viewModel.Shield);
        Assert.Equal(0.2, viewModel.ShieldRatio, 10);
        Assert.Contains(nameof(HealthBarViewModel.Shield), notified);
        Assert.Contains(nameof(HealthBarViewModel.ShieldRatio), notified);
    }

    [Fact]
    public void Block_Change_Notifies_Block()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new HealthBarViewModel(entity.Health);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Health.Block = 3;

        Assert.Equal(3, viewModel.Block);
        Assert.Contains(nameof(HealthBarViewModel.Block), notified);
    }

    [Fact]
    public void Disposing_Stops_Reflecting_The_Health()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new HealthBarViewModel(entity.Health);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Dispose();
        entity.Health.HP = 3;

        Assert.Empty(notified);
    }
}
