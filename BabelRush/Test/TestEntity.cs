using BabelRushR.Core.Entity;

namespace BabelRushR.Game.Test;

/// <summary>
///     测试用实体：沿 X 轴在 ±<see cref="Travel"/> 之间来回走。
/// </summary>
/// <remarks>
///     目的是让「每帧推进 → 位置通知 → 节点位置」这条链有肉眼可见的证据。
/// </remarks>
internal sealed class TestEntity(int maxHP) : EntityBase(maxHP)
{
    private const double Travel = 400.0;
    private const double Speed = 240.0;

    private double _direction = 1.0;

    protected override void DoUpdate(double delta)
    {
        var next = Position + _direction * Speed * delta;

        if (next > Travel)
        {
            next = 2 * Travel - next;
            _direction = -1.0;
        }
        else if (next < -Travel)
        {
            next = -2 * Travel - next;
            _direction = 1.0;
        }

        Position = next;
    }
}
