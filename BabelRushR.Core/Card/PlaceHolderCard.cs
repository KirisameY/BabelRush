using System.ComponentModel;

using BabelRushR.Core.EntityAction;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Numeric.CommonModifiers;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;
using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card;

public class PlaceHolderCard : ICard
{
    public RegKey DisplayKey => field ??= RegKey.From("core", "404");
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> Cost => field ??=
        INumeric.CreateReadonly(0, TypeA.Of<CommonNumericModifyOrder>())
                .WithModifier(new CommonLimitModifier(min: 0, 0));
    public IReadOnlyNotifiableList<IEntityAction> Actions => field ??= new NotifiableList<IEntityAction>().AsReadOnlyNotifiableList();
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
}