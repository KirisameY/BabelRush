using System.ComponentModel;

using BabelRushR.Core.Numeric;

using KirisameY.Numeric;

namespace BabelRushR.Core.Entity.Health;

public interface IEntityHealth : INotifyPropertyChanged
{
    IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxHP { get; }
    IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxShield { get; }

    int HP { get; set; }
    int Shield { get; set; }
    int Block { get; set; }


    public event EventHandler<UpdatedEventArgs> Updated;

    public class UpdatedEventArgs(EntityHealthPart part, int oldValue, int newValue) : EventArgs
    {
        public EntityHealthPart Part => part;
        public int OldValue => oldValue;
        public int NewValue => newValue;
    }
}

public static class EntityHealthExtensions
{
    extension<T>(T health) where T : IEntityHealth
    {
        public T WithUpdatedHandler(EventHandler<IEntityHealth.UpdatedEventArgs> handler)
        {
            health.Updated += handler;
            return health;
        }
    }
}