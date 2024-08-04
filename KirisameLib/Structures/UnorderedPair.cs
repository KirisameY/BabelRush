namespace KirisameLib.Structures;

public readonly struct UnorderedPair<T>(T element1, T element2)
    where T : notnull
{
    private T Element1 { get; } = element1;
    private T Element2 { get; } = element2;
    
    public override string ToString() => $"({Element1}, {Element2})";
    
    public static explicit operator UnorderedPair<T>((T, T) pair) => new(pair.Item1, pair.Item2);
    public static implicit operator (T, T)(UnorderedPair<T> pair) => (pair.Element1, pair.Element2);
    
    public static bool operator ==(UnorderedPair<T> a, UnorderedPair<T> b) => a.Equals(b);
    public static bool operator !=(UnorderedPair<T> a, UnorderedPair<T> b) => !a.Equals(b);

    public bool Equals(UnorderedPair<T> other) => Element1.Equals(other.Element1) && Element2.Equals(other.Element2)
                                               || Element1.Equals(other.Element2) && Element2.Equals(other.Element1);

    public override bool Equals(object? obj)
    {
        return obj is UnorderedPair<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Element1.GetHashCode() ^ Element2.GetHashCode();
    }
}