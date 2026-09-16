namespace BabelRushR.Core.Common;

/// <summary>
/// 每帧推进的对象。由所属系统按固定顺序驱动。
/// </summary>
public interface IUpdatable
{
    void Update(double delta);
}
