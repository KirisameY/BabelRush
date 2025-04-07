namespace BabelRush.Data;

public readonly record struct ModelParseErrorInfo(int ErrorCount, string[] Messages)
{
    public static ModelParseErrorInfo Empty => new(0, []);
}