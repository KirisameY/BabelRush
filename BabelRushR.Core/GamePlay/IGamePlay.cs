using System.ComponentModel;

using BabelRushR.Core.Battle;
using BabelRushR.Core.Common;
using BabelRushR.Core.GamePlay.CardRelated;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;

namespace BabelRushR.Core.GamePlay;

public interface IGamePlay : INotifyPropertyChanged, IUpdatable
{
    /// <summary>供逻辑层内部通信，表现层同步请订阅 Notify 相关接口。</summary>
    [Immutable] IEventBus<GamePlayEvent> EventBus { get; }

    IScene Scene { get; }
    [Immutable] IPlayerState PlayerState { get; }

    [Immutable] ICardDirector CardDirector { get; }

    /// <summary>战斗状态：双方阵营与是否在战斗中。首次访问时创建，常驻本局。</summary>
    [Immutable] IBattle Battle { get; }

    /// <summary>开局以来经过的累计时间（秒）。</summary>
    double Time { get; }

    /// <summary>上一帧实际推进的时间（秒）。</summary>
    double DeltaTime { get; }
}