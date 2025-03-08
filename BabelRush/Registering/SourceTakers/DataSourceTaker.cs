using System;
using System.Collections.Immutable;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Registering.SourceTakers;

public class DataSourceTaker(ISourceTaker<DocumentSyntax> innerSourceTaker, params string[] waitFor) : ISourceTaker<DocumentSyntax>
{
    //Getter

    public static DataSourceTaker Get<TModel, TTarget>(RegisterItem<TTarget> register, params string[] waitFor)
        where TModel : IDataModel<TTarget> =>
        new(new CommonSourceTaker<DocumentSyntax, TModel, TTarget>(register), waitFor);


    //Properties
    public ImmutableArray<string> WaitFor { get; } = waitFor.ToImmutableArray();


    //Implement
    public (string id, Func<bool> register)[] Take(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        innerSourceTaker.Take(source, out errorMessages);
}