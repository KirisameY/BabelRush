using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class DataRegistrant : Registrant<IDictionary<string, object>>
{
    //Constructor
    private DataRegistrant(Registrant<IDictionary<string, object>> registrant, params string[] waitFor)
    {
        _registrant = registrant;
        WaitFor = waitFor.ToImmutableArray();
    }

    public static DataRegistrant Get<TModel, TTarget>(CommonRegister<TTarget> register, params string[] waitFor)
        where TModel : IModel<IDictionary<string, object>, TTarget> =>
        new(new CommonRegistrant<IDictionary<string, object>, TModel, TTarget>(register), waitFor);


    //Properties
    public ImmutableArray<string> WaitFor { get; }


    //Implement
    private readonly Registrant<IDictionary<string, object>> _registrant;

    public override (string id, Func<bool> register)[] Parse(IDictionary<string, object> source, out ModelParseErrorInfo errorMessages) =>
        _registrant.Parse(source, out errorMessages);
}