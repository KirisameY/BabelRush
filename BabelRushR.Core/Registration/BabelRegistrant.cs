using KirisameY.Registration.Registering;

namespace BabelRushR.Core.Registration;

public abstract class BabelRegistrant
{
    protected internal abstract void Work();
}

public abstract class BabelRegistrant<T> : BabelRegistrant, IRegistrant<T>
{
    public abstract void AcceptTarget(IRegisterTarget<T> target);
}