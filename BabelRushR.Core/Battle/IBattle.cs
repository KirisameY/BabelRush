using System.ComponentModel;

using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Battle;

/// <summary>
///     战斗状态：维护双方阵营的参战单位，并在首个敌人出现 / 最后一个敌人离开之间切换
///     <see cref="IsInBattle"/>。
/// </summary>
/// <remarks>
///     阵营由调用方在 <see cref="TryJoinBattle"/> 时指定，不写在实体上——同一实体可以在
///     不同战斗、不同时间属于不同阵营；换边请先 <see cref="TryLeaveBattle"/> 再加入。<br/>
///     事件一律在状态落定之后发布：处理器看到的阵营列表与 <see cref="IsInBattle"/> 都已是最终值。<br/>
///     阵营成员与战斗状态是两件事：我方成员可以在战斗之外加入/离开；只有敌方阵营的
///     空与非空才决定 <see cref="IsInBattle"/>。
/// </remarks>
public interface IBattle : INotifyPropertyChanged
{
    /// <summary>当前是否在战斗中。等价于 <see cref="Enemies"/> 非空。</summary>
    bool IsInBattle { get; }

    /// <summary>我方参战单位，按加入顺序排列。</summary>
    /// <remarks>只读视图：成员变化必须走加入/离开 API，否则 <see cref="IsInBattle"/> 与事件会失同步。</remarks>
    IReadOnlyNotifiableList<IEntity> Allies { get; }

    /// <summary>敌方参战单位，按加入顺序排列。</summary>
    /// <remarks>只读视图：成员变化必须走加入/离开 API，否则 <see cref="IsInBattle"/> 与事件会失同步。</remarks>
    IReadOnlyNotifiableList<IEntity> Enemies { get; }

    /// <summary>
    /// 取单位所在的阵营；不在战斗中（未加入任一阵营）时为 <c>null</c>。
    /// </summary>
    BattleSide? GetSideOf(IEntity unit);

    /// <summary>
    /// 取单位所在阵营的对方阵营单位；单位不属于任何阵营时返回空列表。
    /// </summary>
    /// <returns>对方阵营的实时列表，不要缓存。</returns>
    IReadOnlyList<IEntity> GetOpponentsOf(IEntity unit);

    /// <summary>
    /// 把单位加入指定阵营。
    /// </summary>
    /// <returns>
    /// 若加入为 <c>true</c>；单位已在任一阵营、或不在当前场景中时为 <c>false</c>，
    /// 且不产生任何状态变化与事件。
    /// </returns>
    bool TryJoinBattle(IEntity unit, BattleSide side);

    /// <summary>
    /// 把单位移出战斗。单位本身仍留在场景里，只是不再属于任何阵营。
    /// </summary>
    /// <returns>若成功移出为 <c>true</c>；单位不在任一阵营时为 <c>false</c>。</returns>
    bool TryLeaveBattle(IEntity unit);
}
