using BabelRushR.Core.Card;
using BabelRushR.Core.Card.Types;
using BabelRushR.Core.Numeric.CommonModifiers;

using KirisameY.Registration.Data;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     测试用造牌与改费用：避开注册表，直接构造 <see cref="RecordCardType"/>。
/// </summary>
internal static class TestCards
{
    public static ICard New(int cost = 1) => new RecordCardType(RegKey.Default, cost, []).CreateCard();

    /// <summary>给牌挂一个费用修正器，触发 <see cref="ICard"/> 的 Cost 属性通知。</summary>
    public static void RaiseCost(ICard card, double delta) =>
        card.Cost.AddModifier(new MutableCommonBaseAdditionModifier(delta));
}
