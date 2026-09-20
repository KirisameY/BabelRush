namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     把一个 <see cref="Action"/> 包成 <see cref="IDisposable"/>，用于表示"撤销某次订阅"。
/// </summary>
/// <remarks>
///     重复 <see cref="Dispose"/> 是安全的：底层委托至多被执行一次。
/// </remarks>
internal sealed class ActionDisposable(Action dispose) : IDisposable
{
    private Action? _dispose = dispose;

    public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
}
