using System.Collections.Generic;

using BabelRushR.Game.GUI.Cards.Components;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;

using static BabelRushR.Game.Binding.DataBinding;

namespace BabelRushR.Game.GUI.Cards;

public partial class CardUI : Control
{
    #region Scene & Lifetime

    private CardUI() { }
    private static PackedScene Scene => field ??= GD.Load<PackedScene>("res://GUI/Cards/CardUI.tscn");
    public static CardUI New() => Scene.Instantiate<CardUI>();

    #endregion


    #region Nodes & Properties

    private CardCostUI Cost => field ??= GetNode<CardCostUI>("CardCostUI");
    private CardActionsUI Actions => field ??= GetNode<CardActionsUI>("CardActionsUI");

    #endregion

    #region Binding

    private readonly List<IBindHandle> _binds = [];

    public CardViewModel? ViewModel
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (IsInsideTree()) Rebind();
            else Unbind();
        }
    }

    private void Rebind()
    {
        Unbind();
        if (ViewModel is not { } viewModel) return;

        _binds.AddRange(
            Binder.BindPropertyOneWay(
                viewModel, vm => vm.Cost, this, ui => ui.Cost.Value
            ),
            Binder.BindPropertyOneWay(
                viewModel, vm => vm.Actions, this, ui => ui.Actions.ViewModel
            )
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }

    #endregion
}