using System.ComponentModel;

using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.GamePlay;

public interface IGamePlay : INotifyPropertyChanged
{
    IScene Scene { get; }
    IPlayerState PlayerState { get; }

    void Update(double delta);
}