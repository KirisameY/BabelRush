using System;
using System.Collections.Immutable;

using BabelRush.Data;

using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class DataRegistrant(Registrant<byte[]> innerRegistrant, params string[] waitFor) : Registrant<byte[]>
{
    //Getter

    public static DataRegistrant Get<TModel, TTarget>(CommonRegister<TTarget> register, params string[] waitFor)
        where TModel : IDataModel<TTarget> =>
        new(new CommonRegistrant<byte[], TModel, TTarget>(register), waitFor);


    //Properties
    public ImmutableArray<string> WaitFor { get; } = waitFor.ToImmutableArray();


    //Implement

    public override (string id, Func<bool> register)[] Parse(byte[] source, out ModelParseErrorInfo errorMessages) =>
        innerRegistrant.Parse(source, out errorMessages);
}