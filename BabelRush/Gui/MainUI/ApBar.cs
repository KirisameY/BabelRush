using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.DisplayInfos;
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


    private static class Names
    {
        public static readonly StringName SetValue = "set_value";
        public static readonly StringName SetRate = "set_rate";
    }


    #region Update

    private bool _ready = false;

    private void UpdateAll()
    {
        UpdateAp();
        UpdateApRate();
        UpdateFont();
    }

    private void UpdateFont()
    {
        var fontInfo = LocalInfoRegisters.FontInfos.GetItem(FontInfoIds.Title);
        CardNameLabel.LabelSettings.Font     = fontInfo.Font;
        CardNameLabel.LabelSettings.FontSize = fontInfo.Size;
    }

    private void UpdateAp() => ApBall.Call(Names.SetValue,    Game.Play!.PlayerState.Ap);
    private void UpdateApRate() => ApBall.Call(Names.SetRate, Game.Play!.PlayerState.ApRegenerated);

    #endregion


    #region Override

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
        SubscribeInstanceHandler(Game.GameEventBus);
        if (_ready) UpdateAll();
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }

    #endregion


    #region Event Handlers

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

    #endregion
}