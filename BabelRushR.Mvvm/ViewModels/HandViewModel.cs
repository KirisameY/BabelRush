using BabelRushR.Core.Card;
using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     手牌的视图代理：把 <see cref="IPlayerState.HandPile"/> 里的 <see cref="ICard"/> 一一包装成稳定的
///     <see cref="CardViewModel"/>，并把源列表的变更原样翻译成 VM 列表的变更。
/// </summary>
/// <remarks>
///     <see cref="Cards"/> 与源列表保持<b>同索引、同顺序、一一对应</b>，因此移动/排序不需要重建任何 VM。<br/>
///     移除、替换、清空时先把变更通知给视图，再释放被摘下的 VM，避免视图拿到已经断订阅的对象。
/// </remarks>
public sealed class HandViewModel : ViewModelBase
{
    /// <summary>
    ///     与 <see cref="IPlayerState.HandPile"/> 同步的牌 VM 列表。
    /// </summary>
    public IReadOnlyNotifiableList<CardViewModel> Cards => field ??= _cards.AsReadOnlyNotifiableList();

    private readonly NotifiableList<CardViewModel> _cards = [];

    public HandViewModel(INotifiableList<ICard> handPile)
    {
        foreach (var card in handPile) _cards.Add(new CardViewModel(card));

        handPile.ListUpdated += OnHandPileUpdated;
        Track(new ActionDisposable(() => handPile.ListUpdated -= OnHandPileUpdated));
    }

    private void OnHandPileUpdated(object? sender, ListUpdateEventArgs<ICard> args)
    {
        switch (args)
        {
            // Cleared 派生自 Removed，必须先判
            case IListItemClearedEventArgs<ICard>:
                _cards.Clear().ForEach(card => card.Dispose());
                break;
            case IListItemRemovedEventArgs<ICard> removed:
                _cards.MultiRemove(removed.Indexes).ForEach(card => card.Dispose());
                break;
            case IListItemAddedEventArgs<ICard> added:
                _cards.MultiInsert(added.Indexes, added.AddedItems.Select(card => new CardViewModel(card)));
                break;
            case IListItemReplacedEventArgs<ICard> replaced:
                _cards.MultiReplace(replaced.Indexes, replaced.NewItems.Select(card => new CardViewModel(card)))
                      .ForEach(card => card.Dispose());
                break;
            case IListItemMovedEventArgs<ICard> moved:
                _cards.MultiMove(moved.ItemMoves.Select(move => (move.OldIndex, move.NewIndex)));
                break;
            case IListResetEventArgs<ICard> reset:
                _cards.Clear().ForEach(card => card.Dispose());
                _cards.AddRange(reset.ListView.Select(card => new CardViewModel(card)));
                break;
        }
    }

    protected override void DisposeCore() => _cards.Clear().ForEach(card => card.Dispose());
}
