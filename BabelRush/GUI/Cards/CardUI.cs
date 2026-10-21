using System.Collections.Generic;

using BabelRushR.Game.Assets;
using BabelRushR.Game.GUI.Cards.Components;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;
using KirisameY.Registration.Data;

using static BabelRushR.Game.Binding.DataBinding;

namespace BabelRushR.Game.GUI.Cards;

public partial class CardUI : Button
{
    #region Scene & Lifetime

    private CardUI() { }
    private static PackedScene Scene => field ??= GD.Load<PackedScene>("res://GUI/Cards/CardUI.tscn");
    public static CardUI New() => Scene.Instantiate<CardUI>();

    public override void _EnterTree() => Rebind();
    public override void _ExitTree() => Unbind();

    #endregion


    #region Nodes & Properties

    private Label Title => field ??= GetNode<Label>("Title");
    private TextureRect Image => field ??= GetNode<TextureRect>("Image");
    private CardCostUI Cost => field ??= GetNode<CardCostUI>("Cost");
    private CardActionsUI Actions => field ??= GetNode<CardActionsUI>("Actions");

    private RegKey DisplayKey
    {
        get;
        set
        {
            if (field == value) return;
            field         = value;
            Title.Text    = value;
            Image.Texture = AssetLoader.Load<Texture2D>("graphic/card", value, "png") ?? new PlaceholderTexture2D();
        }
    } = RegKey.Default;

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
            ),
            Binder.BindPropertyOneWay(
                viewModel, vm => vm.DisplayKey, this, ui => ui.DisplayKey
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