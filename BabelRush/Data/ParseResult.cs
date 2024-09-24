using System;
using System.Diagnostics.CodeAnalysis;

namespace BabelRush.Data;

public readonly struct ParseResult<TResult>
    where TResult : notnull
{
    private readonly TResult? _result;
    private readonly Exception? _exception;

    public ParseResult(TResult result)
    {
        _result = result;
    }

    public ParseResult(Exception exception)
    {
        _exception = exception;
    }


    public bool Succeeded => _result is not null;
    public TResult Result => _result ?? throw Exception;
    public Exception Exception => _exception ?? (Succeeded ? new NoException() : new InvalidDataException());

    public bool TryGetResult([MaybeNullWhen(false)] out TResult result)
    {
        result = _result!;
        return Succeeded;
    }


    //Exceptions
    public class NoException() : Exception("Since there is nothing wrong, this exception is just a placeholder.");

    public class InvalidDataException() : Exception("Invalid ParseResult: both Result and Exception are null");
}