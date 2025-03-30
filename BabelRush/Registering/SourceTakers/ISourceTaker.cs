using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public interface ISourceTaker<TSource>
{
    void Take(TSource source, string path, out ModelParseErrorInfo errorMessages);
}