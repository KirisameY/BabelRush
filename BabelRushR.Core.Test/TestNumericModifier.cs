using KirisameY.Numeric;

namespace BabelRushR.Core.Test;

/// <summary>一次 double 空间上的改写。</summary>
internal delegate void NumericModify(ref double value);

/// <summary>
/// 把一段改写逻辑包成 Modifier。库只提供 <see cref="INumericModifier{TOrder}"/> 接口，
/// 测试里需要一个能随手造的东西。
/// </summary>
internal sealed class TestNumericModifier<TOrder>(TOrder order, NumericModify modify)
    : INumericModifier<TOrder> where TOrder : Enum
{
    private EventHandler? _updated;

    public TOrder Order => order;

    /// <summary>宿主 numeric 会订阅它，等它变化时重算。<br/>
    /// 测试用的 Modifier 自身的值不会变，所以这里从不 raise。</summary>
    public event EventHandler? Updated
    {
        add => _updated += value;
        remove => _updated -= value;
    }

    public void ModifyValue(ref double value) => modify(ref value);
}

internal static class Modifier
{
    public static TestNumericModifier<TOrder> Of<TOrder>(TOrder order, NumericModify modify)
        where TOrder : Enum => new(order, modify);
}
