using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;
using BabelRush.Gui.Cards;

using Godot;

using KirisameLib.Core.Extensions;

namespace BabelRush.Gui.Screens.Cards;

public partial class CardListScreen : Control
{
    //Sub Nodes
    [field: AllowNull, MaybeNull]
    private CardList CardList => field ??= GetNode<CardList>("HSplitContainer/CardList");

    [field: AllowNull, MaybeNull]
    private CardInfoText CardInfoText => field ??= GetNode<CardInfoText>("HSplitContainer/MarginContainer/CardInfo");


    //Fields
    private readonly Dictionary<Card, CardInterface> _cardDict = new();


    //Public Methods
    public void Add(Card card)
    {
        if (_cardDict.ContainsKey(card)) return;

        var ci = _cardDict[card] = CardInterface.GetInstance(card);
        CardList.Add(ci);
    }

    public void AddRange(IEnumerable<Card> cards) => cards.ForEach(Add);

    public void Remove(Card card)
    {
        if (!_cardDict.Remove(card, out var ci)) return;

        CardList.Remove(ci);
    }

    public void Clear()
    {
        _cardDict.Clear();
        CardList.Clear();
    }

    public void ReplaceWith(IEnumerable<Card> cards)
    {
        Clear();
        AddRange(cards);
    }


    //Scene Tree
    public override void _Ready()
    {
        CardList.CardSelected += OnListCardSelected;
    }


    //Event Handlers
    private void OnListCardSelected(CardInterface? ci)
    {
        if (ci is null) CardInfoText.Clear();
        else CardInfoText.SetCard(ci.Card);
    }
}