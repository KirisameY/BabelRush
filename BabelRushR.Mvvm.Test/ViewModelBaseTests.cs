using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.Test;

public class ViewModelBaseTests
{
    /// <summary>
    ///     只记录「自己被释放了几次」，用来区分「释放了」和「没释放」以及「释放了不止一次」。
    /// </summary>
    private sealed class FakeDisposable : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose() => DisposeCount++;
    }

    /// <summary>
    ///     把 protected 的 <c>Track</c> / <c>Untrack</c> / <c>IsDisposed</c> 开个口子，
    ///     并记下 <c>DisposeCore</c> 被调用的次数。
    /// </summary>
    private sealed class ProbeViewModel : ViewModelBase
    {
        public int CoreDisposeCount { get; private set; }

        public bool Disposed => IsDisposed;

        public T Add<T>(T subscription) where T : IDisposable => Track(subscription);

        public void Drop(IDisposable subscription) => Untrack(subscription);

        protected override void DisposeCore() => CoreDisposeCount++;
    }

    [Fact]
    public void IsDisposed_Is_False_Until_Dispose()
    {
        var viewModel = new ProbeViewModel();

        Assert.False(viewModel.Disposed);

        viewModel.Dispose();

        Assert.True(viewModel.Disposed);
    }

    [Fact]
    public void Tracked_Subscription_Is_Disposed_With_The_ViewModel()
    {
        var viewModel = new ProbeViewModel();
        var subscription = viewModel.Add(new FakeDisposable());

        viewModel.Dispose();

        Assert.Equal(1, subscription.DisposeCount);
    }

    [Fact]
    public void Untracked_Subscription_Is_Left_Alone()
    {
        var viewModel = new ProbeViewModel();
        var subscription = new FakeDisposable();
        viewModel.Add(subscription);
        viewModel.Drop(subscription);

        viewModel.Dispose();

        Assert.Equal(0, subscription.DisposeCount);
    }

    [Fact]
    public void Track_After_Dispose_Releases_The_Subscription_Immediately()
    {
        var viewModel = new ProbeViewModel();
        viewModel.Dispose();

        var late = new FakeDisposable();
        var returned = viewModel.Add(late);

        // 立刻释放，且不留在表里——否则下次 Dispose 会再释放一次。
        Assert.Equal(1, late.DisposeCount);
        Assert.Same(late, returned);

        viewModel.Dispose();
        Assert.Equal(1, late.DisposeCount);
    }

    [Fact]
    public void Dispose_Is_Idempotent_And_Runs_DisposeCore_Once()
    {
        var viewModel = new ProbeViewModel();
        var subscription = viewModel.Add(new FakeDisposable());

        viewModel.Dispose();
        viewModel.Dispose();

        Assert.Equal(1, subscription.DisposeCount);
        Assert.Equal(1, viewModel.CoreDisposeCount);
    }

    [Fact]
    public void Reentrant_Dispose_From_A_Subscription_Does_Not_Loop()
    {
        var viewModel = new ProbeViewModel();
        // 订阅在释放过程中反过来调 vm.Dispose()：重入必须在门口就被挡住。
        viewModel.Add(new ActionDisposable(() => viewModel.Dispose()));

        viewModel.Dispose();

        Assert.Equal(1, viewModel.CoreDisposeCount);
    }

    [Fact]
    public void Subscriptions_Tracked_During_Dispose_Are_Released_Not_Leaked()
    {
        var viewModel = new ProbeViewModel();
        var late = new FakeDisposable();
        // 撤销订阅时又注册一个：此时已置位，应立刻释放，而不是塞进正在清空的列表。
        viewModel.Add(new ActionDisposable(() => viewModel.Add(late)));

        viewModel.Dispose();

        Assert.Equal(1, late.DisposeCount);
    }
}
