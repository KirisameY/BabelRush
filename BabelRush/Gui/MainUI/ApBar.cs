using BabelRush.Cards;
using BabelRush.GamePlay;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Core.Events;

namespace BabelRush.Gui.MainUI;

public partial class ApBar : Control
{
    #region Sub nodes

    private Sprite2D? _apBall;
    private Sprite2D ApBall => _apBall ??= GetNode<Sprite2D>("ApBall");

    private Label? _cardNameLabel;
    private Label CardNameLabel => _cardNameLabel ??= GetNode<Label>("CardNameLabel");

    #endregion


    //Update
    private static readonly StringName StringNameSetValue = "SetValue";
    private static readonly StringName StringNameSetRate = "SetRate";


    private void UpdateAp() => ApBall.Call(StringNameSetValue,    Play.PlayerState.Ap);
    private void UpdateApRate() => ApBall.Call(StringNameSetRate, Play.PlayerState.ApRegenerated);


    //Override
    public override void _Ready()
    {
        UpdateAp();
        UpdateApRate();
    }

    public override void _Process(double delta)
    {
        UpdateApRate();
    }

    public override void _EnterTree()
    {
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public override void _ExitTree()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }


    //Event Handlers

    [EventHandler] [UsedImplicitly]
    private void OnApChanged(ApChangedEvent e)
    {
        UpdateAp();
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardSelected(CardSelectedEvent e)
    {
        CardNameLabel.Text = e.Selected ? e.Card.Type.NameDesc.Name : "";
    }
}