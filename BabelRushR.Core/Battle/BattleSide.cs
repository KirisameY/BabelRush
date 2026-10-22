namespace BabelRushR.Core.Battle;

/// <summary>
///     战斗中的一方。
/// </summary>
/// <remarks>
///     阵营不记在实体上：同一个实体可以在不同时间属于不同阵营，归属由
///     <see cref="IBattle.TryJoinBattle"/> 时指定。
/// </remarks>
public enum BattleSide
{
    /// <summary>我方：玩家、召唤物与友军。</summary>
    Ally,

    /// <summary>敌方。</summary>
    Enemy,
}
