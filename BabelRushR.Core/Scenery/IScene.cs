using System.ComponentModel;

using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Scenery;

public interface IScene : INotifyPropertyChanged
{
    INotifiableList<IEntity> Entities { get; }
}