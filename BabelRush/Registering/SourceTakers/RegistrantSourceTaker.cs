using System;

using BabelRush.Data;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.SourceTakers;

public class RegistrantSourceTaker<TSource, TModel, TTarget> : CommonSourceTaker<TSource, TModel, TTarget>, IRegistrant<TTarget>
    where TModel : IModel<TSource, TTarget>
{
    private IRegTarget<TTarget>? _registrant;

    public void AcceptTarget(IRegTarget<TTarget> target)
    {
        if (_registrant is null) _registrant = target;
        else throw new InvalidOperationException("RegistrantSourceTaker already has a target.");
    }

    protected override bool RegisterItem(string id, TTarget item)
    {
        if (_registrant is null) throw new InvalidOperationException("RegistrantSourceTaker does not have a target.");
        return _registrant.AddItem(id, item);
    }
}