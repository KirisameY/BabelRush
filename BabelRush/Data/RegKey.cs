using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BabelRush.Data;

public sealed class RegKey
{
    #region Factory

    private RegKey(string nameSpace, string key)
    {
        NameSpace = nameSpace;
        Key = key;
    }

    // ReSharper disable once InconsistentNaming
    private static readonly ConcurrentDictionary<(string nameSpace, string key), RegKey> _cache = new();

    public static RegKey From(string nameSpace, string key)
    {
        return _cache.GetOrAdd((nameSpace, key), static t =>
        {
            if (t.nameSpace.Contains(':') || t.key.Contains(':')) throw new ArgumentException($"Invalid id: {t.nameSpace}:{t.key}");
            return new RegKey(t.nameSpace, t.key);
        });
    }

    public static RegKey From(string id)
    {
        if (id.Split(':') is not [var nameSpace, var key]) throw new ArgumentException($"Invalid id: {id}");
        return From(nameSpace, key);
    }

    #endregion


    #region Fields

    public string NameSpace { get; }
    public string Key { get; }

    [field: AllowNull, MaybeNull]
    public string FullName => field ??= $"{NameSpace}:{Key}";

    #endregion


    #region Equality

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    #endregion


    #region Convert

    public override string ToString() => FullName;

    public void Deconstruct(out string nameSpace, out string key) => (nameSpace, key) = (NameSpace, Key);

    public static implicit operator string(RegKey regKey) => regKey.FullName;

    public static implicit operator RegKey(string regKey) => From(regKey);

    public static implicit operator RegKey((string nameSpace, string key) regKey) => From(regKey.nameSpace, regKey.key);

    #endregion
}

public static class RegKeyExtensions
{
    public static RegKey WithDefaultNameSpace(this string id, string defaultNameSpace) => id.Split(':') switch
    {
        [var nameSpace, var key] => RegKey.From(nameSpace,        key),
        [var key]                => RegKey.From(defaultNameSpace, key),
        _                        => throw new AggregateException($"Invalid id: {id}"),
    };
}