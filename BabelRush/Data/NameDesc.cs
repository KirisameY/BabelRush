namespace BabelRush.Data;

public readonly struct NameDesc(string name, string desc, string quote)
{
    public string Name { get; } = name;
    public string Desc { get; } = desc;
    public string Quote { get; } = quote;


    public static implicit operator NameDesc((string Name, string Desc) tuple) => new(tuple.Name, tuple.Desc, "");
    public static implicit operator NameDesc((string Name, string Desc, string Quote) tuple) => new(tuple.Name, tuple.Desc, tuple.Quote);
    public void Deconstruct(out string name, out string desc) => (name, desc) = (Name, Desc);
    public void Deconstruct(out string name, out string desc, out string quote) => (name, desc, quote) = (Name, Desc, Quote);
}