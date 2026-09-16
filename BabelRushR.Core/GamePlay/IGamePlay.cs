using System.ComponentModel;

using BabelRushR.Core.Common;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;

namespace BabelRushR.Core.GamePlay;

public interface IGamePlay : INotifyPropertyChanged, IUpdatable
{
    /// <summary>供逻辑层内部通信，表现层同步请订阅 Notify 相关接口。</summary>
    IEventBus EventBus { get; }

    IScene Scene { get; }
    IPlayerState PlayerState { get; }

    /// <summary>开局以来的累计时间（秒），不含被 <c>Update</c> 忽略的非法 delta。</summary>
    double Time { get; }

    /// <summary>上一帧实际推进的时间（秒）。</summary>
    double DeltaTime { get; }
}
