using System.ComponentModel;

using BabelRushR.Core.Common;
using BabelRushR.Core.EntityAction;
using BabelRushR.Core.Numeric;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;

namespace BabelRushR.Core.Card;

public class TypedCard(ICardType type) : ObservableObject, ICard
{
    public ICardType Type => type;

    public string DisplayKey => field ??= type.Id;
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> Cost => field ??=
        INumeric.CreateReadonly(type.Cost, TypeA.Of<CommonNumericModifyOrder>())
                .WithUpdateHandler(PropertyChangedHandler());

    public IReadOnlyNotifiableList<IEntityAction> Actions => field ??= _actions.AsReadOnlyNotifiableList();
    private readonly NotifiableList<IEntityAction> _actions = type.Actions
                                                                  .Select(t => t.Type.CreateAction(t.Value))
                                                                  .ToNotifiableList();
}