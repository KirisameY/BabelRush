using BabelRushR.Core.Numeric;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class EntityViewModelTests
{
    /// <summary>HP 变化时 Core 发出的通知顺序：先 HP，再补一发 IsAlive。</summary>
    private static readonly string?[] HPChangeNotifications = ["HP", "HPRatio", "IsAlive"];

    [Fact]
    public void Exposes_The_Entity_State()
    {
        var entity = new TestEntity(maxHP: 10) { HP = 7, Position = 3.5 };

        var viewModel = new EntityViewModel(entity);

        Assert.Equal(7, viewModel.HP);
        Assert.Equal(10, viewModel.MaxHP);
        Assert.Equal(0.7, viewModel.HPRatio, 10);
        Assert.True(viewModel.IsAlive);
        Assert.Equal(3.5, viewModel.Position);
    }

    [Fact]
    public void HPRatio_Is_Zero_When_MaxHP_Is_Zero()
    {
        var entity = new TestEntity(maxHP: 0);

        var viewModel = new EntityViewModel(entity);

        Assert.Equal(0, viewModel.MaxHP);
        Assert.Equal(0.0, viewModel.HPRatio);
    }

    [Fact]
    public void HP_Change_Notifies_HP_HPRatio_And_IsAlive()
    {
        var entity = new TestEntity(maxHP: 10) { HP = 5 };
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.HP = 3;

        Assert.Equal(3, viewModel.HP);
        Assert.Equal(HPChangeNotifications, notified);
    }

    [Fact]
    public void IsAlive_Change_Notifies_IsAlive()
    {
        var entity = new TestEntity(maxHP: 10) { HP = 1 };
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.HP = 0;

        Assert.False(viewModel.IsAlive);
        Assert.Equal(HPChangeNotifications, notified);
    }

    [Fact]
    public void MaxHP_Change_Notifies_MaxHP_And_HPRatio()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.MaxHP.AddModifier(Modifier.Of(CommonNumericModifyOrder.BaseAdd, static (ref double v) => v += 10));

        Assert.Equal(20, viewModel.MaxHP);
        Assert.Equal(0.5, viewModel.HPRatio, 10);

        // 不比对顺序：上限变化会连带触发 HP 重钳制，Core 那边的通知次序不是本测试要锁定的东西。
        Assert.Contains(nameof(EntityViewModel.MaxHP), notified);
        Assert.Contains(nameof(EntityViewModel.HPRatio), notified);
    }

    [Fact]
    public void Position_Change_Notifies_Position()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.Position = 1.5;

        Assert.Equal(1.5, viewModel.Position);
        Assert.Equal(new string?[] { "Position" }, notified);
    }

    [Fact]
    public void Unknown_Property_Change_Is_Ignored()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.NotifyUnknownPropertyChanged();

        Assert.Empty(notified);
    }

    [Fact]
    public void Empty_Property_Name_Notifies_Every_Mapped_Property()
    {
        var entity = new TestEntity(maxHP: 10);
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        entity.NotifyAllPropertiesChanged();

        // 逐个断言而不是比序列：映射表内部的枚举次序不属于对外契约。
        var count = notified.Count;
        Assert.Equal(5, count);
        Assert.Contains(nameof(EntityViewModel.HP), notified);
        Assert.Contains(nameof(EntityViewModel.MaxHP), notified);
        Assert.Contains(nameof(EntityViewModel.HPRatio), notified);
        Assert.Contains(nameof(EntityViewModel.IsAlive), notified);
        Assert.Contains(nameof(EntityViewModel.Position), notified);
    }

    [Fact]
    public void Disposed_ViewModel_Stops_Responding()
    {
        var entity = new TestEntity(maxHP: 10) { HP = 5 };
        var viewModel = new EntityViewModel(entity);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Dispose();
        entity.HP = 3;

        Assert.Empty(notified);
    }

    [Fact]
    public void Dispose_Is_Idempotent()
    {
        var viewModel = new EntityViewModel(new TestEntity(maxHP: 10));

        viewModel.Dispose();
        viewModel.Dispose();
    }
}
