using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Gui.Cards;

using Godot;

using KirisameLib.Extensions;
using KirisameLib.Event;

using CardInterface = BabelRush.Gui.Cards.CardInterface;

namespace BabelRush.Gui.MainUI;

[EventHandlerContainer]
public partial class CardField : Control
{
    //Init
    public override void _Ready() { }


    #region Members

    private static CardPile Pile => Game.Play!.CardHub.CardField;
    private Dictionary<Card, CardInterface> CardDict { get; } = [];
    private IReadOnlyCollection<CardInterface> CardInterfaces => CardDict.Values;

    #endregion


    //Process
    public override void _Process(double delta)
    {
        MovePickedCard();
    }


    #region Card Operation

    private void AddCard(CardInterface ci)
    {
        CardDict.TryAdd(ci.Card, ci);
        AddChild(ci);
        ci.Selectable = false;

        InsertCard(ci);
        UpdateCardPosition();
    }

    private void AddCard(Card card)
    {
        var ci = CardInterface.GetInstance(card);
        ci.Position = new(Project.ViewportSize.X / 2, Project.ViewportSize.Y + 32); //new Vector2(200,86)
        AddCard(ci);
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private bool RemoveCard(Card card)
    {
        if (!CardDict.Remove(card)) return false;
        UpdateCardPosition();
        SortCards();
        return true;
    }

    private void RemoveCard(CardInterface ci) => RemoveCard(ci.Card);

    #endregion


    #region Card Select

    private CardInterface? _selected;
    private CardInterface? Selected
    {
        get => _selected;
        set
        {
            if (Picked is not null) return;
            var old = _selected;
            var @new = value;
            _selected = value;

            OnSelectChanged(old, @new);
            if (old is not null) Game.GameEventBus.Publish(new CardSelectedEvent(old.Card,   false));
            if (@new is not null) Game.GameEventBus.Publish(new CardSelectedEvent(@new.Card, true));
        }
    }

    private int SelectedCardIndex => Selected is not null ? CardInterfaces.PositionOfFirst(Selected) : -1;

    #endregion


    #region Card Drag & Use

    private CardInterface? _picked;
    private CardInterface? Picked
    {
        get => _picked;
        set
        {
            var oldOut = PickedCardOutField;
            var old = _picked;
            var @new = value;
            _picked = value;

            OnPickedChanged(old, oldOut, @new);
        }
    }

    private async void OnPickedChanged(CardInterface? old, bool oldOut, CardInterface? @new)
    {
        if (old is not null)
        {
            old.Selectable = false;
            Game.GameEventBus.Publish(new CardPickedEvent(old.Card, false));
            if (!oldOut || !await old.Card.Use(Game.Play!.BattleField.Player)) //偷懒了，先检查oldOut再进行TryUse，任何一个失败则执行InsertCard
                InsertCard(old);
        }

        if (@new is not null)
        {
            PickUpCard(@new);
            Game.GameEventBus.Publish(new CardPickedEvent(@new.Card, true));
        }
    }

    private Vector2 PickOffset { get; set; }

    private bool PickedCardOutField => Picked?.Position.Y < 0;

    private void MovePickedCard()
    {
        if (Picked is null) return;
        Picked.Position = GetLocalMousePosition() + PickOffset;
    }

    private void PickUpCard(CardInterface card)
    {
        PickOffset = -card.GetLocalMousePosition();
        card.XPosTween?.Kill();
        card.YPosTween?.Kill();
    }

    #endregion


    #region Event handlers

    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }

    [EventHandler]
    private void OnCardInterfaceSelected(CardInterfaceSelectedEvent e)
    {
        if (Game.Play is null) return;
        if (!CardInterfaces.Contains(e.CardInterface)) return;
        if (e.Selected)
            Selected = e.CardInterface;
        else if (Selected == e.CardInterface)
            Selected = null;
    }

    [EventHandler]
    private void OnCardInterfacePressed(CardInterfacePressedEvent e)
    {
        if (Game.Play is null) return;
        if (!CardInterfaces.Contains(e.CardInterface)) return;
        if (e.Pressed)
            Picked = e.CardInterface;
        else if (Picked == e.CardInterface)
            Picked = null;
    }

    [EventHandler]
    private void OnCardPileInserted(CardInsertedToPileEvent e)
    {
        if (e.CardPile != Pile) return;
        AddCard(e.Card);
    }

    [EventHandler]
    private void OnCardPileRemoved(CardRemovedFromPileEvent e)
    {
        if (e.CardPile != Pile) return;
        RemoveCard(e.Card);
    }

    #endregion
}