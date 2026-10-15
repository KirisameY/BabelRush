using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;

using BabelRushR.Core.Card;
using BabelRushR.Core.Entity;
using BabelRushR.Core.EntityAction;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Mvvm.ViewModels;

public class CardViewModel(ICard card) : ViewModelBase<CardViewModel>(card, Reactions)
{
    private static readonly PropertyChangeReactor<CardViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<CardViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(ICard.Cost)]    = ([], [nameof(Cost)]),
            [nameof(ICard.Actions)] = ([ActionsRebind], []),
        }.ToFrozenDictionary()
    );

    private readonly ICard _card = card;

    public int Cost => _card.Cost.Value;

    private IReadOnlyNotifiableList<IEntityAction> _sourceActions = card.Actions;
    private NotifiableList<EntityActionViewModel> ActionsList => field ??= List();
    public IReadOnlyNotifiableList<EntityActionViewModel> Actions => field ??= ActionsList.AsReadOnlyNotifiableList();

    private NotifiableList<EntityActionViewModel> List()
    {
        var result = _card.Actions.Select(action => new EntityActionViewModel(action)).ToNotifiableList();
        _card.Actions.ListUpdated += ActionsUpdate;
        return result;
    }


    private static void ActionsRebind(CardViewModel instance)
    {
        if (instance._sourceActions == instance._card.Actions) return;
        instance._sourceActions = instance._card.Actions;
        instance.ActionsList.Clear().ForEach(a => a.Dispose());
        instance.ActionsList.AddRange(instance._card.Actions.Select(action => new EntityActionViewModel(action)));
    }

    private void ActionsUpdate(object? sender, ListUpdateEventArgs<IEntityAction> args)
    {
        Action action = args switch
        {
            IListItemAddedEventArgs<IEntityAction> added => () =>
                ActionsList.MultiInsert(added.Indexes, added.AddedItems.Select(a => new EntityActionViewModel(a))),
            IListItemClearedEventArgs<IEntityAction> => () =>
                ActionsList.Clear().ForEach(a => a.Dispose()),
            IListItemRemovedEventArgs<IEntityAction> removed => () =>
                ActionsList.MultiRemove(removed.Indexes)
                           .ForEach(a => a.Dispose()),
            IListItemReplacedEventArgs<IEntityAction> replaced => () =>
                ActionsList.MultiReplace(replaced.Indexes, replaced.NewItems.Select(a => new EntityActionViewModel(a)))
                           .ForEach(a => a.Dispose()),
            IListItemMovedEventArgs<IEntityAction> moved => () =>
                ActionsList.MultiMove(moved.ItemMoves.Select(m => (m.OldIndex, m.NewIndex))),
            IListResetEventArgs<IEntityAction> reset => () =>
            {
                ActionsList.Clear().ForEach(a => a.Dispose());
                ActionsList.AddRange(reset.ListView.Select(a => new EntityActionViewModel(a)));
            },
            _ => () => { }
        };
        action.Invoke();
    }

    protected override void DisposeCore()
    {
        ActionsList.Clear().ForEach(a => a.Dispose());
    }
}