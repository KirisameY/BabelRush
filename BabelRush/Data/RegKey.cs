using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using KirisameLib.Logging;

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
            if (t.nameSpace.Contains(':') || t.key.Contains(':'))
            {
                Logger.Log(LogLevel.Error, "Parsing String", $"Invalid id: {t.nameSpace}:{t.key}");
                return Default;
            }
            return new RegKey(t.nameSpace, t.key);
        });
    }

    public static RegKey From(string id)
    {
        if (id.Split(':') is not [var nameSpace, var key])
        {
            Logger.Log(LogLevel.Error, "Parsing String", $"Invalid id: {id}");
            return Default;
        }
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


    // Default
    public static readonly RegKey Default = From("", "");


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(RegKey));
}

public static class RegKeyExtensions
{
    public static RegKey WithDefaultNameSpace(this string id, string defaultNameSpace) => id.Split(':') switch
    {
        [var nameSpace, var key] => RegKey.From(nameSpace,        key),
        [var key]                => RegKey.From(defaultNameSpace, key),
        _                        => RegKey.From(id), // will log a error and return default
    };
}