using System;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public interface ISourceTaker<TSource>
{
    (string id, Func<bool> register)[] Take(TSource source, out ModelParseErrorInfo errorMessages);
}