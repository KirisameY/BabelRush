using System;
using System.Collections.Immutable;

using BabelRush.Data;

using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class DataRegistrant : Registrant<byte[]>
{
    //Getter
    private DataRegistrant(Registrant<byte[]> registrant, params string[] waitFor)
    {
        _registrant = registrant;
        WaitFor = waitFor.ToImmutableArray();
    }

    public static DataRegistrant Get<TModel, TTarget>(CommonRegister<TTarget> register, params string[] waitFor)
        where TModel : IDataModel<TTarget> =>
        new(new CommonRegistrant<byte[], TModel, TTarget>(register), waitFor);


    //Properties
    public ImmutableArray<string> WaitFor { get; }


    //Implement
    private readonly Registrant<byte[]> _registrant;

    public override (string id, Func<bool> register)[] Parse(byte[] source, out ModelParseErrorInfo errorMessages) =>
        _registrant.Parse(source, out errorMessages);
}