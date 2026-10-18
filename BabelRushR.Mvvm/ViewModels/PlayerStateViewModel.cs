using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     玩家状态的视图代理：目前只持有手牌容器，后续 AP、抽牌堆、弃牌堆都从这里扩展。
/// </summary>
public sealed class PlayerStateViewModel(IPlayerState playerState) : ViewModelBase
{
    public IPlayerState SourcePlayerState => playerState;

    /// <summary>
    ///     手牌区。首次访问时创建，生命周期跟随本 VM。
    /// </summary>
    public HandViewModel Hand => field ??= Track(new HandViewModel(playerState.HandPile));
}
