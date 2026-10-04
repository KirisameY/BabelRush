using System.Collections.Immutable;

namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     按源对象发出的属性名，把一次属性变化翻译成"更新本 VM"与"通知视图"两组动作。
/// </summary>
/// <typeparam name="TObj">
///     接收更新的 VM 类型。
/// </typeparam>
/// <param name="dependencies">
///     源属性名 → （该变化引发的更新，该变化需要一并通知的 VM 属性名）。
/// </param>
/// <remarks>
///     源只发它自己那层属性的变化，派生量、计算属性以及需要连带重建的对象都靠这张表接出来。<br/>
///     表里没有的源属性会被忽略；空属性名表示"全部失效"，无从查表，按表中出现过的全部条目处理。<br/>
///     不论走哪条路都<b>先跑完更新再发通知</b>，保证视图收到通知时读到的已经是更新后的状态。<br/>
///     两个列表都要写全，没有就写 <c>[]</c>：<see cref="ImmutableArray{T}"/> 的 <c>default</c> 一枚举就会抛。
/// </remarks>
public sealed class PropertyChangeReactor<TObj>(
    IReadOnlyDictionary<string, (ImmutableArray<Action<TObj>> Updates, ImmutableArray<string> Notifications)> dependencies)
{
    /// <summary>
    ///     "全部失效"时要跑的全部更新，按动作去重（同一个方法可能挂在多个源属性名下）。
    /// </summary>
    private readonly ImmutableArray<Action<TObj>> _allUpdates =
        [..dependencies.Values.SelectMany(reaction => reaction.Updates).Distinct()];

    /// <summary>
    ///     "全部失效"时要发的全部通知，按属性名去重。
    /// </summary>
    private readonly ImmutableArray<string> _allNotifications =
        [..dependencies.Values.SelectMany(reaction => reaction.Notifications).Distinct()];

    /// <summary>
    ///     执行该属性变化对应的更新，再把对应的通知交给 <paramref name="notify"/>。
    /// </summary>
    /// <param name="propertyName">源对象发出的属性名；<see langword="null"/> 或空串表示"全部失效"。</param>
    /// <param name="instance">接收更新的 VM。</param>
    /// <param name="notify">发出属性变化通知的回调，通常是 VM 的 <c>OnPropertyChanged</c>。</param>
    public void Forward(string? propertyName, TObj instance, Action<string?> notify)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            foreach (var update in _allUpdates) update.Invoke(instance);
            foreach (var notification in _allNotifications) notify.Invoke(notification);
            return;
        }

        if (!dependencies.TryGetValue(propertyName, out var reaction)) return;

        foreach (var update in reaction.Updates) update.Invoke(instance);
        foreach (var notification in reaction.Notifications) notify.Invoke(notification);
    }
}