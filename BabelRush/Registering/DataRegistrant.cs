using System;
using System.Collections.Immutable;

using BabelRush.Data;

using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public class DataRegistrant(IRegistrant<DocumentSyntax> innerRegistrant, params string[] waitFor) : IRegistrant<DocumentSyntax>
{
    //Getter

    public static DataRegistrant Get<TModel, TTarget>(CommonRegister<TTarget> register, params string[] waitFor)
        where TModel : IDataModel<TTarget> =>
        new(new CommonRegistrant<DocumentSyntax, TModel, TTarget>(register), waitFor);


    //Properties
    public ImmutableArray<string> WaitFor { get; } = waitFor.ToImmutableArray();


    //Implement
    public (string id, Func<bool> register)[] Parse(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        innerRegistrant.Parse(source, out errorMessages);
}