using System.ComponentModel;

using BabelRushR.Core.EntityAction;

namespace BabelRushR.Core.Card;

public interface ICard : INotifyPropertyChanged // use empty string for all property changed
{
    int Cost { get; }
    IReadOnlyList<IEntityAction> Actions { get; }
}