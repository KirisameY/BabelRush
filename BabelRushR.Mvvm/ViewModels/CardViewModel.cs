using System.Collections.Frozen;
using System.Collections.Immutable;

using BabelRushR.Core.Card;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.Registration.Data;

namespace BabelRushR.Mvvm.ViewModels;

public class CardViewModel(ICard card) : ViewModelBase<CardViewModel>(card, Reactions)
{
    private static readonly PropertyChangeReactor<CardViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<CardViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(ICard.Cost)]       = ([], [nameof(Cost)]),
            [nameof(ICard.DisplayKey)] = ([], [nameof(DisplayKey)])
        }.ToFrozenDictionary()
    );


    public ICard SourceCard => card;

    public RegKey DisplayKey => card.DisplayKey;

    public int Cost => card.Cost.Value;

    public IReadOnlyNotifiableList<EntityActionViewModel> Actions => field ??= CollectionWrapper.List(
        card.Actions, action => new EntityActionViewModel(action), Track
    );
}