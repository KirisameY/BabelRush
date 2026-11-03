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
///     一组设定 <see cref="ICardType"/> 的虚拟 id、Cost、Action 数量与 Action 数值的控件，以及一个按当前参数
///     重建类型与卡、并换掉 CardUI 现有绑定的按钮。
/// </summary>
/// <remarks>
///     卡牌初始绑定的「默认值」就取自场景里各输入控件的初值，与重建用的是同一份参数。<br/>
///     Cost 是「类型自带的基础值 + 挂上去的修正器」，所以滑块动的是修正器、不是数值本身；
///     卡牌与 Action 的 id 只用于显示，不查注册表，所以可以随便填。<br/>
///     Action 数量与「有无值」的变化直接体现为 CardUI 里那列动作行的数量，以及动作实例是
///     <c>IValuedEntityAction</c> 还是普通的 <c>IEntityAction</c>。
/// </remarks>
public partial class TestCard : Control
{
    #region Nodes

    private CardUI Card => field ??= GetNode<CardUI>("CardUI");

    private HSlider CostModifierSlider => field ??= GetNode<HSlider>("Controls/CostModifierRow/CostModifierSlider");
    private Label CostModifierValue => field ??= GetNode<Label>("Controls/CostModifierRow/CostModifierValue");
    private LineEdit CardIdInput => field ??= GetNode<LineEdit>("Controls/CardIdRow/CardIdInput");
    private SpinBox TypeCostBox => field ??= GetNode<SpinBox>("Controls/TypeCostRow/TypeCostBox");
    private LineEdit ActionIdInput => field ??= GetNode<LineEdit>("Controls/ActionIdRow/ActionIdInput");
    private SpinBox ActionCountBox => field ??= GetNode<SpinBox>("Controls/ActionCountRow/ActionCountBox");
    private CheckBox ActionHasValueCheck => field ??= GetNode<CheckBox>("Controls/ActionHasValueRow/ActionHasValueCheck");
    private SpinBox ActionValueBox => field ??= GetNode<SpinBox>("Controls/ActionValueRow/ActionValueBox");
    private Button RebuildButton => field ??= GetNode<Button>("Controls/RebuildButton");

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
        ActionHasValueCheck.Toggled     += OnActionHasValueToggled;
        RebuildButton.Pressed           += Rebuild;

        OnActionHasValueToggled(ActionHasValueCheck.ButtonPressed);
        Rebuild();
    }

    public override void _ExitTree() => _viewModel?.Dispose();


    #region Rebuild

    /// <summary>
    ///     按当前控件参数重建类型与卡，并把 CardUI 的绑定换到新卡上。
    /// </summary>
    private void Rebuild()
    {
        var card = CreateCardType(
            ParseRegKey(CardIdInput.Text),
            (int)TypeCostBox.Value,
            (int)ActionCountBox.Value,
            ParseRegKey(ActionIdInput.Text),
            ActionHasValueCheck.ButtonPressed,
            (int)ActionValueBox.Value
        ).CreateCard();

        var old = _viewModel;
        _card          = card;
        _viewModel     = new CardViewModel(card);
        _costModifier  = null; // 新卡换了一枚全新的 Numeric，修正器要重新挂上去。
        Card.ViewModel = _viewModel;
        old?.Dispose();

        ApplyCostModifier();
    }

    /// <summary>
    ///     构造一个测试用的卡牌类型：id 只作显示用途，不查注册表；所有 action 共用同一个 action 类型。
    /// </summary>
    private static ICardType CreateCardType(
        RegKey cardId, int cost, int actionCount, RegKey actionId, bool hasValue, int value)
    {
        // 有值 / 无值对应 Core 的两种 delegate 类型，CreateAction 时分别产出 IValuedEntityAction 和普通 IEntityAction。
        IEntityActionType actionType = hasValue
            ? new ValuedDelegateEntityActionType(actionId, (_, _) => { })
            : new DelegateEntityActionType(actionId, _ => { });

        return new RecordCardType(
            cardId,
            cost,
            [..Enumerable.Range(0, actionCount).Select(_ => (actionType, hasValue ? value : 0))]
        );
    }

    /// <summary>
    ///     测试用的虚拟 id：允许写 <c>namespace:key</c>，也允许只写 key（补 <c>core</c> 命名空间）。
    /// </summary>
    private static RegKey ParseRegKey(string text)
    {
        var trimmed = text.Trim();
        return (trimmed.Length == 0 ? null : RegKey.FromFull(trimmed)) ?? RegKey.From("core", trimmed);
    }

    /// <summary>没有数值的 action 类型不需要用户填数值，把输入框一并置灰。</summary>
    private void OnActionHasValueToggled(bool hasValue) => ActionValueBox.Editable = hasValue;

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