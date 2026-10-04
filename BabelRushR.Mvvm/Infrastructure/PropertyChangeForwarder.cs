using System.Collections.Immutable;

namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     把源对象发出的属性变化翻译成本 VM 里需要一并通知的属性名。
/// </summary>
/// <param name="dependencies">
///     源属性名 → 本 VM 中受其影响、需要一并通知的属性名。
/// </param>
/// <remarks>
///     源只发它自己那层属性的变化，派生量与计算属性都要靠这张表接出来。
///     表里没有的源属性会被忽略；空属性名表示"全部失效"，无从查表，按表中出现过的全部目标属性通知。
/// </remarks>
public class PropertyChangeForwarder(IReadOnlyDictionary<string, ImmutableArray<string>> dependencies)
{
    private readonly ImmutableArray<string> _allProperties =
        [..dependencies.Values.SelectMany(names => names).Distinct()];

    /// <summary>
    ///     把源属性名翻译成需要通知的 VM 属性名，逐个交给 <paramref name="notify"/>。
    /// </summary>
    public void Forward(string? propertyName, Action<string?> notify)
    {
        ImmutableArray<string>? notifies = string.IsNullOrEmpty(propertyName) ? _allProperties :
            dependencies.TryGetValue(propertyName, out var affected) ? affected : null;

        if (notifies is null) return;
        foreach (var name in notifies) notify.Invoke(name);
    }
}