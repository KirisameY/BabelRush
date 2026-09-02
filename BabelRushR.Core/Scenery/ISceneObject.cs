using System.ComponentModel;

namespace BabelRushR.Core.Scenery;

public interface ISceneObject : INotifyPropertyChanged
{
    double Position { get; set; }
}