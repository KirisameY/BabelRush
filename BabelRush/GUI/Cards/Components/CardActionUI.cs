using System.Collections.Generic;

using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;

namespace BabelRushR.Game.GUI.Cards.Components;

public partial class CardActionUI : Control
{
    #region Scene & Lifetime

    private CardActionUI() { }

    private static PackedScene Scene => field ??= GD.Load<PackedScene>("res://GUI/Cards/Components/CardActionUI.tscn");
    public static CardActionUI New() => Scene.Instantiate<CardActionUI>();

    #endregion


    #region Binding

    private readonly List<IBindHandle> _binds = [];

    public EntityActionViewModel? ViewModel
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
            // todo: bind here
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    #endregion

    public override void _Ready()
    {
        var preview = FindChild("_preview");
        RemoveChild(preview);
        preview.QueueFree();
    }
}