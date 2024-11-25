using BabelRush.Cards;
using BabelRush.GamePlay;

using Godot;

using KirisameLib.Core.Events;

namespace BabelRush.Gui.MainUI;

[EventHandlerContainer]
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
        SubscribeInstanceHandler(Game.EventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
    }


    //Event Handlers

    [EventHandler]
    private void OnApChanged(ApChangedEvent e)
    {
        UpdateAp();
    }

    [EventHandler]
    private void OnCardSelected(CardSelectedEvent e)
    {
        CardNameLabel.Text = e.Selected ? e.Card.Type.NameDesc.Name : "";
    }
}