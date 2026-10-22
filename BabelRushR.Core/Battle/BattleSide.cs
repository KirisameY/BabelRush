namespace BabelRushR.Core.Battle;

/// <summary>
///     战斗中的阵营。
/// </summary>
public enum BattleSide
{
    /// <summary>不属于任何阵营（默认值，也用于无效阵营）。</summary>
    None = 0,

    /// <summary>我方：玩家、召唤物与友军。</summary>
    Ally,

    /// <summary>敌方。</summary>
    Enemy,
}

public static class BattleSideEnumExtensions
{
    extension(BattleSide side)
    {
        /// <summary>对阵的另一方；<see cref="BattleSide.None"/> 仍为 <see cref="BattleSide.None"/>。</summary>
        public BattleSide Opponent => side switch
        {
            BattleSide.Ally  => BattleSide.Enemy,
            BattleSide.Enemy => BattleSide.Ally,
            _                => BattleSide.None
        };
    }
}
