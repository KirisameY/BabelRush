using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     ViewModel 基类：在 <see cref="ObservableObject"/> 之上补上订阅的集中管理。
/// </summary>
/// <remarks>
///     VM 会订阅 Core 的 <see cref="System.ComponentModel.INotifyPropertyChanged"/> 与集合通知，
///     这些订阅必须成对撤销，否则会泄漏并让已释放的 VM 继续响应。<br/>
///     约定：本类的派生类型建立的<b>每一个</b>订阅都要交给 <see cref="Track"/>。<br/>
///     <see cref="Dispose"/> 之后本 VM 进入「已释放」状态且不可逆：此时再交给
///     <see cref="Track"/> 的订阅会被立刻释放，不会挂上来；<see cref="Dispose"/> 本身幂等。
/// </remarks>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    private bool _disposed;

    /// <summary>
    ///     本 VM 是否已被释放。派生类若在惰性属性里<b>创建</b>对象，可据此避免释放后再造一个。
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    ///     登记一个子 IDisposable，其生命周期与本 VM 绑定。
    /// </summary>
    /// <remarks>
    ///     若本 VM 已释放，传入的订阅会被<b>立刻释放</b>而不是登记，随即原样返回：调用方照常拿到
    ///     引用，但它已经是死的。这样「释放后一切失效」对滞后注册的订阅也成立，不会漏。
    /// </remarks>
    protected T Track<T>(T subscription) where T : IDisposable
    {
        if (_disposed)
        {
            subscription.Dispose();
            return subscription;
        }

        _subscriptions.Add(subscription);
        return subscription;
    }

    /// <summary>
    ///     移除一个被 Track 的对象。
    /// </summary>
    protected void Untrack(IDisposable subscription) => _subscriptions.Remove(subscription);

    public void Dispose()
    {
        // 重入与重复调用都在这里止住：DisposeCore 里若又调回 Dispose，不该再走一遍。
        if (Interlocked.Exchange(ref _disposed, true)) return;

        // 先置位再摘订阅：撤销过程中若有代码 Track，届时已走「立刻释放」那条路，
        // 不会往正在清空的列表里塞东西；列表也先换走，避免遍历时被 Untrack 改动。
        var subscriptions = _subscriptions.ToArray();
        _subscriptions.Clear();
        foreach (var subscription in subscriptions) subscription.Dispose();

        DisposeCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     释放本 VM 自有资源的钩子，在全部订阅撤销之后调用。
    /// </summary>
    protected virtual void DisposeCore() { }
}

public abstract class ViewModelBase<TSelf> : ViewModelBase, IDisposable
    where TSelf : ViewModelBase<TSelf>
{
    protected ViewModelBase(INotifyPropertyChanged model, PropertyChangeReactor<TSelf> reactions)
    {
        Track(model.Subscribe((sender, args) => reactions.Forward(args.PropertyName, (TSelf)this, OnPropertyChanged)));
    }
}