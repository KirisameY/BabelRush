using System.ComponentModel;

using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Battle;

/// <summary>
///     战斗状态：维护双方阵营的参战单位，并由敌方阵营是否为空推导
///     <see cref="IsInBattle"/>。
/// </summary>
public interface IBattle : INotifyPropertyChanged
{
    /// <summary>当前是否在战斗中。</summary>
    bool IsInBattle { get; }

    /// <summary>我方参战单位，按加入顺序排列。</summary>
    /// <remarks>只读视图：成员变化必须走加入/离开 API，否则 <see cref="IsInBattle"/> 与事件会失同步。</remarks>
    IReadOnlyNotifiableList<IEntity> Allies { get; }

    /// <summary>敌方参战单位，按加入顺序排列。</summary>
    /// <remarks>只读视图：成员变化必须走加入/离开 API，否则 <see cref="IsInBattle"/> 与事件会失同步。</remarks>
    IReadOnlyNotifiableList<IEntity> Enemies { get; }

    /// <summary>
    /// 取单位所在的阵营；不在战斗中（未加入任一阵营）时为 <c>None</c>。
    /// </summary>
    BattleSide GetSideOf(IEntity unit);

    /// <summary>
    /// 取指定阵营的参战单位；<see cref="BattleSide.None"/> 返回 <c>null</c>。
    /// </summary>
    /// <returns>该阵营的实时列表，不要缓存。</returns>
    IReadOnlyList<IEntity>? GetSideEntities(BattleSide side);

    /// <summary>
    /// 把单位加入指定阵营。
    /// </summary>
    /// <returns>
    /// 若成功加入为 <c>true</c>；否则可能给出的阵营无效、单位已在任一阵营，或不在当前场景中，此时为 <c>false</c>，
    /// </returns>
    bool TryJoinBattle(IEntity unit, BattleSide side);

    /// <summary>
    /// 把单位移出战斗。单位本身仍留在场景里，只是不再属于任何阵营。
    /// </summary>
    /// <returns>若成功移出为 <c>true</c>；单位不在任一阵营时为 <c>false</c>。</returns>
    bool TryLeaveBattle(IEntity unit);
}
