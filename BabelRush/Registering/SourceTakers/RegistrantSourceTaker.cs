using System;

using BabelRush.Data;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

namespace BabelRush.Registering.SourceTakers;

public class RegistrantSourceTaker<TSource, TModel, TTarget> : CommonSourceTaker<TSource, TModel, TTarget>, IRegistrant<TTarget>
    where TModel : IModel<TSource, TTarget>
{
    private MoltenRegister<TTarget>? _registrant;

    public void AcceptMoltenRegister(MoltenRegister<TTarget> moltenRegister)
    {
        if (_registrant is null) _registrant = moltenRegister;
        else throw new InvalidOperationException("RegistrantSourceTaker already has a MoltenRegister.");
    }

    protected override bool RegisterItem(string id, TTarget item)
    {
        if (_registrant is null) throw new InvalidOperationException("RegistrantSourceTaker does not have a MoltenRegister.");
        return _registrant.AddItem(id, item);
    }
}