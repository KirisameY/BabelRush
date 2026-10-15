using System.ComponentModel;

namespace BabelRushR.Core.EntityAction;

public interface IEntityAction: INotifyPropertyChanged
{
    void Act(EntityActionContext ctx);
}