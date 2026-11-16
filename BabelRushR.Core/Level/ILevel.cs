using System.ComponentModel;

using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Level;

public interface ILevel : INotifyPropertyChanged
{
    IGamePlay? GamePlay { get; } // todo: 這真的應該是公開接口的一部分嗎？

    LevelMap Map { get; }

    LevelNode CurrentFork { get; }

    LevelNode CurrentRoom { get; }

    IReadOnlyNotifiableList<LevelNode> PlacedNodes { get; }

    IReadOnlyNotifiableList<LevelNode> PassedNodes { get; }

    bool CanSelect(LevelNode node); // todo: 我想維護一個可選節點的 Set 會更好

    bool TrySelectNext(LevelNode node);

    bool TryBegin(); // todo: 不要這麼做

    protected internal void Attach(IGamePlay gamePlay);

    protected internal void Unattach();
}
