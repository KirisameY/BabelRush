namespace BabelRushR.Core.GamePlay;

/// <summary>标记一个可在每局游戏创建时自动装配的事件处理器。</summary>
/// <remarks>
///     必须同时标注 <see cref="GamePlayEventHandlerAttribute"/>，否则不会被自动发现。
/// </remarks>
public interface IGamePlayEventHandler
{
    /// <summary>向本局装配处理器。</summary>
    /// <returns>表示该处理器生命周期的 <see cref="IDisposable"/>；没有需要释放的资源时返回 <c>null</c>。</returns>
    static abstract IDisposable? Register(IGamePlay gamePlay);
}
