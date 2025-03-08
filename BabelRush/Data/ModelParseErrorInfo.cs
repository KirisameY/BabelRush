namespace BabelRush.Data;

public record struct ModelParseErrorInfo(int ErrorCount, string[] Messages)
{
    public static ModelParseErrorInfo Empty => new(0, []);
}