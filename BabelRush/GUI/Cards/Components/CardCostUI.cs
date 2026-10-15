using Godot;

namespace BabelRushR.Game.GUI.Cards.Components;

public partial class CardCostUI : Control
{
    #region Scene & Lifetime

    private CardCostUI() { }

    // private static PackedScene Scene => field ??= GD.Load<PackedScene>("res://GUI/Cards/Components/CardCostUI.tscn");
    //
    // public static CardCostUI New() => Scene.Instantiate<CardCostUI>();

    #endregion

    #region Nodes & Properties

    private Label Label => field ??= GetNode<Label>("Label");

    public int Value
    {
        get;
        set
        {
            if (field == value) return;
            field      = value;
            Label.Text = value.ToString();
        }
    }

    #endregion
}