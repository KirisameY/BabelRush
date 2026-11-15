using System.ComponentModel;

using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Level;

public interface ILevel : INotifyPropertyChanged
{
    IGamePlay? GamePlay { get; }

    LevelMap Map { get; }

    LevelNode CurrentNode { get; }

    IReadOnlyNotifiableList<RoomPlacement> PlacedRooms { get; }

    IReadOnlyList<LevelNode> PassedNodes { get; }

    bool CanSelect(LevelNode node);

    bool TrySelectNext(LevelNode node);

    bool TryBegin();

    protected internal void Attach(IGamePlay gamePlay);

    protected internal void Unattach();
}
