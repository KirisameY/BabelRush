using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.Misc;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

using KirisameLib.Event;

namespace BabelRush.Gui.MainUI;

[EventHandlerContainer]
public partial class ApBar : Control
{
    #region Sub nodes

    [field: AllowNull, MaybeNull]
    private Sprite2D ApBall => field ??= GetNode<Sprite2D>("ApBall");

    [field: AllowNull, MaybeNull]
    private Label CardNameLabel => field ??= GetNode<Label>("CardNameLabel");

    #endregion


    //Update
    private static readonly StringName StringNameSetValue = "SetValue";
    private static readonly StringName StringNameSetRate = "SetRate";

    private bool _ready = false;

    private void UpdateAll()
    {
        UpdateAp();
        UpdateApRate();
        UpdateFont();
    }

    private void UpdateFont()
    {
        var fontInfo = LocalInfoRegisters.FontInfo.GetItem("title");
        CardNameLabel.LabelSettings.Font = fontInfo.Font;
        CardNameLabel.LabelSettings.FontSize = fontInfo.Size;
    }

    private void UpdateAp() => ApBall.Call(StringNameSetValue,    Play.PlayerState.Ap);
    private void UpdateApRate() => ApBall.Call(StringNameSetRate, Play.PlayerState.ApRegenerated);


    //Override
    public override void _Ready()
    {
        UpdateAll();
        _ready = true;
    }

    public override void _Process(double delta)
    {
        UpdateApRate();
    }

    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.EventBus);
        if (_ready) UpdateAll();
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

    [EventHandler]
    private void OnLocalChanged(LocalChangedEvent e)
    {
        UpdateFont();
    }
}