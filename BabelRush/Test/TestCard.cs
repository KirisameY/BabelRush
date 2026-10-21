using System.Collections.Immutable;
using System.Linq;

using BabelRushR.Core.Card;
using BabelRushR.Core.Card.Types;
using BabelRushR.Core.EntityAction.Types;
using BabelRushR.Core.Numeric.CommonModifiers;
using BabelRushR.Game.GUI.Cards;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.Registration.Data;

namespace BabelRushR.Game.Test;

/// <summary>
///     开发期的卡牌测试场景：中间一张 <see cref="CardUI"/>，右侧一个热改 Cost 的滑块，
///     一组设定 <see cref="ICardType"/> 的 Cost 与 Action 数量的控件，以及一个按当前参数
///     重建类型与卡、并换掉 CardUI 现有绑定的按钮。
/// </summary>
/// <remarks>
///     卡牌初始绑定的「默认值」就取自场景里两个 <see cref="SpinBox"/> 的初值，与重建用的是同一份参数。<br/>
///     Cost 是「类型自带的基础值 + 挂上去的修正器」，所以滑块动的是修正器、不是数值本身；
///     Action 数量的变化则直接体现为 CardUI 里那列动作行的增减。
/// </remarks>
public partial class TestCard : Control
{
    #region Nodes

    private CardUI Card => field ??= GetNode<CardUI>("CardUI");

    private HSlider CostModifierSlider   => field ??= GetNode<HSlider>("Controls/CostModifierRow/CostModifierSlider");
    private Label   CostModifierValue    => field ??= GetNode<Label>("Controls/CostModifierRow/CostModifierValue");
    private SpinBox TypeCostBox          => field ??= GetNode<SpinBox>("Controls/TypeCostRow/TypeCostBox");
    private SpinBox ActionCountBox       => field ??= GetNode<SpinBox>("Controls/ActionCountRow/ActionCountBox");
    private Button  RebuildButton        => field ??= GetNode<Button>("Controls/RebuildButton");

    #endregion


    private CardViewModel? _viewModel;
    private ICard? _card;

    /// <summary>
    ///     挂在当前卡 Cost 上的修正器；滑块第一次动的时候才建出来。
    /// </summary>
    private MutableCommonBaseAdditionModifier? _costModifier;

    public override void _Ready()
    {
        CostModifierSlider.ValueChanged += _ => ApplyCostModifier();
        RebuildButton.Pressed            += Rebuild;

        Rebuild();
    }

    public override void _ExitTree() => _viewModel?.Dispose();


    #region Rebuild

    /// <summary>
    ///     按当前控件参数重建类型与卡，并把 CardUI 的绑定换到新卡上。
    /// </summary>
    private void Rebuild()
    {
        var card = CreateCardType((int)TypeCostBox.Value, (int)ActionCountBox.Value).CreateCard();

        var old = _viewModel;
        _card          = card;
        _viewModel     = new CardViewModel(card);
        _costModifier  = null; // 新卡换了一枚全新的 Numeric，修正器要重新挂上去。
        Card.ViewModel = _viewModel;
        old?.Dispose();

        ApplyCostModifier();
    }

    private static ICardType CreateCardType(int cost, int actionCount) => new RecordCardType(
        TestCardTypeId,
        cost,
        ImmutableArray.CreateRange(
            Enumerable.Range(1, actionCount).Select(value => (TestActionType, value))
        )
    );

    private static readonly RegKey TestCardTypeId = RegKey.From("babelrush", "test");

    /// <summary>
    ///     Action 的占位类型：目标选择与打牌都还没做，现在只关心数量，效果留空。
    /// </summary>
    private static readonly IEntityActionType TestActionType = new ValuedDelegateEntityActionType((_, _) => { });

    #endregion


    #region Cost Modifier

    /// <summary>
    ///     把滑块的值写到当前卡的 Cost 修正器上。
    /// </summary>
    /// <remarks>
    ///     第一次（以及每次换卡之后）是「添加」，之后都是「修改」同一个修正器。
    /// </remarks>
    private void ApplyCostModifier()
    {
        if (_card is not { } card) return;

        if (_costModifier is null)
        {
            _costModifier = new MutableCommonBaseAdditionModifier(CostModifierSlider.Value);
            card.Cost.AddModifier(_costModifier);
        }
        else
        {
            _costModifier.Value = CostModifierSlider.Value;
        }

        CostModifierValue.Text = _costModifier.Value.ToString("0");
    }

    #endregion
}
