using BabelRushR.Core.Card.Operations;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     卡牌区域UI相关的视图代理：目前只持有手牌容器，后续 AP、抽牌堆、弃牌堆都从这里扩展。
/// </summary>
public sealed class CardDeckViewModel(ICardDeck cardDeck, ICardDirector cardDirector) : ViewModelBase
{
    public ICardDeck SourceCardDeck => cardDeck;
    public ICardDirector SourceCardDirector => cardDirector;

    /// <summary>
    ///     手牌区。首次访问时创建，生命周期跟随本 VM。
    /// </summary>
    public IReadOnlyNotifiableList<CardViewModel> HandPile => field ??= CollectionWrapper.List(
        cardDeck.HandPile, card => new CardViewModel(card), Track
    );

    public void TryPlayCard(CardViewModel card) => cardDirector.TryPlayCard(card.SourceCard);
}