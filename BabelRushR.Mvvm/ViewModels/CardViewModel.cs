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
            [nameof(ICard.Cost)] = ([], [nameof(Cost)]),
        }.ToFrozenDictionary()
    );

    public int Cost => card.Cost.Value;

    public IReadOnlyNotifiableList<EntityActionViewModel> Actions => field ??= CollectionWrapper.List(
        card.Actions, action => new EntityActionViewModel(action), Track
    );
}