using CommunityToolkit.Mvvm.ComponentModel;

namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     ViewModel 基类：在 <see cref="ObservableObject"/> 之上补上订阅的集中管理。
/// </summary>
/// <remarks>
///     VM 会订阅 Core 的 <see cref="System.ComponentModel.INotifyPropertyChanged"/> 与集合通知，
///     这些订阅必须成对撤销，否则会泄漏并让已释放的 VM 继续响应。<br/>
///     约定：本类的派生类型建立的<b>每一个</b>订阅都要交给 <see cref="Track"/>。
/// </remarks>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    /// <summary>
    ///     登记一个订阅，其生命周期与本 VM 绑定。
    /// </summary>
    protected void Track(IDisposable subscription) => _subscriptions.Add(subscription);

    public void Dispose()
    {
        foreach (var subscription in _subscriptions) subscription.Dispose();
        _subscriptions.Clear();

        DisposeCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     释放本 VM 自有资源的钩子，在全部订阅撤销之后调用。
    /// </summary>
    protected virtual void DisposeCore() { }
}
