using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Numeric.CommonModifiers;

using KirisameY.Numeric;

namespace BabelRushR.Core.Entity.Health;

public class CommonEntityHealth(int maxHP) : ObservableObject, IEntityHealth
{
    private readonly int _maxHP = maxHP; // avoid CS9124
    private MutableLimitModifier MaxShieldLimit => field ??= new MutableLimitModifier(min: 0, max: MaxHP.Value * 2);

    public IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxHP => field ??=
        INumeric.CreateReadonly<int, CommonNumericModifyOrder>(_maxHP)
                .WithModifier(new CommonLimitModifier(min: 1))
                .WithUpdateHandler(PropertyChangedHandler())
                .WithUpdateHandler((_, _) => HP = HP) // 更新时重新赋值HP触发钳制
                .WithUpdateHandler((_, _) => MaxShieldLimit.MaxValue = MaxHP.Value * 2);
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxShield => field ??=
        INumeric.CreateReadonly<int, CommonNumericModifyOrder>(MaxHP.Value)
                .WithModifier(MaxShieldLimit)
                .WithUpdateHandler(PropertyChangedHandler())
                .WithUpdateHandler((_, _) => Shield = Shield);

    public int HP
    {
        get;
        set
        {
            var old = field;
            if (SetProperty(ref field, Math.Clamp(value, 0, MaxHP.Value)))
                Updated?.Invoke(this, new(IEntityHealth.Part.HP, old, field));
        }
    } = maxHP;

    public int Shield
    {
        get;
        set
        {
            var old = field;
            if (SetProperty(ref field, Math.Clamp(value, 0, MaxShield.Value)))
                Updated?.Invoke(this, new(IEntityHealth.Part.Shield, old, field));
        }
    }

    public int Block
    {
        get;
        set
        {
            var old = field;
            if (SetProperty(ref field, Math.Max(value, 0)))
                Updated?.Invoke(this, new(IEntityHealth.Part.Block, old, field));
        }
    }

    public event EventHandler<IEntityHealth.UpdatedEventArgs>? Updated;
}