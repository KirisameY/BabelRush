using BabelRushR.Core.Battle;

namespace BabelRushR.Core.Test;

public class BattleSideTests
{
    [Theory]
    [InlineData(BattleSide.Ally, BattleSide.Enemy)]
    [InlineData(BattleSide.Enemy, BattleSide.Ally)]
    [InlineData(BattleSide.None, BattleSide.None)]
    public void Opponent_Is_The_Other_Side(BattleSide side, BattleSide expected)
        => Assert.Equal(expected, side.Opponent);
}
