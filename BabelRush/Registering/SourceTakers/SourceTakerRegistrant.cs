using System;

using BabelRush.Data;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.SourceTakers;

public abstract class SourceTakerRegistrant<TSource>
{
    public abstract ISourceTaker<TSource> CreateSourceTaker(string nameSpace, bool overridePrevious);
}

public sealed class SourceTakerRegistrant<TSource, TModel, TTarget> : SourceTakerRegistrant<TSource>, IRegistrant<RegKey, TTarget>
    where TModel : IModel<TSource, TTarget>
{
    private IRegTarget<RegKey, TTarget>? _regTarget;

    public void AcceptTarget(IRegTarget<RegKey, TTarget> target)
    {
        if (_regTarget is null) _regTarget = target;
        else throw new InvalidOperationException("RegistrantSourceTaker already has a target.");
    }

    public override ISourceTaker<TSource> CreateSourceTaker(string nameSpace, bool overridePrevious)
    {
        if (_regTarget is null) throw new InvalidOperationException("RegistrantSourceTaker does not have a target.");
        return new RegistrantSourceTaker(_regTarget, nameSpace, overridePrevious);
    }

    private class RegistrantSourceTaker(IRegTarget<RegKey, TTarget> target, string nameSpace, bool overridePrevious) : CommonSourceTaker<TSource, TModel, TTarget>
    {
        protected override void RegisterItem(IModel<TTarget> model, out string[] errors)
        {
            errors = [];
            var (id, item) = model.Convert(nameSpace);
            if (overridePrevious) target.AddOrOverwriteItem(id, item);
            else if (!target.AddItem(id, item)) errors = [$"Failed to register item {id}, Possibly there's already a registered item with a duplicate ID."];
        }
    }
}