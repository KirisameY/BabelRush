using System.ComponentModel;

using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Level;

public interface ILevel : INotifyPropertyChanged
{
    LevelMap Map { get; }

    LevelNode CurrentFork { get; }

    LevelNode CurrentRoom { get; }

    IReadOnlyNotifiableList<LevelNode> PlacedNodes { get; }

    IReadOnlyNotifiableList<LevelNode> PassedNodes { get; }

    bool CanSelect(LevelNode node); // todo: 我想維護一個可選節點的 Set 會更好

    bool TrySelectNext(LevelNode node);

    protected internal void Attach(IGamePlay gamePlay);

    protected internal void Unattach();
}
