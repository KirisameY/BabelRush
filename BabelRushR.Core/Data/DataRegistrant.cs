using BabelRushR.Core.Common.Serializing;

using KirisameY.Registration.Registering;

using Tomlyn;

using Zio;

namespace BabelRushR.Core.Data;

public abstract class DataRegistrant
{
    protected internal abstract void Load(string packNameSpace, IFileSystem packFs);
}

public abstract class DataRegistrant<T> : DataRegistrant, IRegistrant<T>
{
    private IRegisterTarget<T>? _target;

    public void AcceptTarget(IRegisterTarget<T> target)
    {
        if (_target is not null) throw new InvalidOperationException("Registrant already have a target");
        _target = target;
    }

    protected internal sealed override void Load(string packNameSpace, IFileSystem packFs)
    {
        if (_target is null) throw new InvalidOperationException("Registrant has no target");
        Load(packNameSpace, packFs, _target);
    }

    protected abstract void Load(string packNameSpace, IFileSystem packFs, IRegisterTarget<T> target);
}