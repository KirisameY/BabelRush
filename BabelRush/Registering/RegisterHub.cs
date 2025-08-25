using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registering.I18n;
using BabelRush.Registering.Registers;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering;

public static class RegisterHub
{
    private static readonly Dictionary<string, (IRegister<RegKey>, bool nullable)> Registers = new();

    private static Dictionary<string, (IRegister<RegKey> register, bool nullable)>.AlternateLookup<ReadOnlySpan<char>>? _alter;
    private static Dictionary<string, (IRegister<RegKey> register, bool nullable)>.AlternateLookup<ReadOnlySpan<char>> Alter =>
        _alter ??= Registers.GetAlternateLookup<ReadOnlySpan<char>>();


    public static bool AddRegister(string path, IRegister<RegKey> register, bool nullable) => Registers.TryAdd(path, (register, nullable));


    public static IRegister<RegKey>? GetRegister(string path)
    {
        {
            var indexes = path.Index().Where(c => c.Item == '/').Reverse();
            foreach (var (index, _) in indexes)
            {
                var span = path.AsSpan()[..index];
                if (Alter.TryGetValue(span, out var tuple))
                {
                    var (result, _) = tuple;
                    var suffix = path[(index + 1)..];
                    if (suffix == "") return result;
                    return SubRegister.Get(result, suffix);
                }
            }
            return null;
        }
    }

    public static IRegister<RegKey, T>? GetRegister<T>(string path) where T : notnull => GetRegister<T>(path, false);

    public static IRegister<RegKey, T>? GetRegisterNullable<T>(string path) => GetRegister<T>(path, true);

    private static IRegister<RegKey, T>? GetRegister<T>(string path, bool nullable)
    {
        var indexes = path.Index().Where(c => c.Item == '/').Reverse();
        foreach (var (index, _) in indexes)
        {
            var span = path.AsSpan()[..index];
            if (Alter.TryGetValue(span, out var tuple))
            {
                if (tuple.nullable && !nullable) return null;
                if (tuple.register is not IRegister<RegKey, T> result) return null;

                var suffix = path[(index + 1)..];
                if (suffix == "") return result;
                return SubRegister.Get(result, suffix);
            }
        }
        return null;
    }


    // Extensions
    public static I18nRegister<T> AddToRegisterHub<T>(this I18nRegister<T> register, string path, bool nullable)
    {
        AddRegister(path, register, nullable);
        return register;
    }

    public static I18nRegister<T> AddToRegisterHub<T>(this I18nRegister<T> register, string path) where T : notnull
    {
        AddRegister(path, register, false);
        return register;
    }

    public static I18nRegister<T> AddToRegisterHubNullable<T>(this I18nRegister<T> register, string path)
    {
        AddRegister(path, register, true);
        return register;
    }

    public static IEnumerableRegister<RegKey, T> AddToRegisterHub<T>(this IEnumerableRegister<RegKey, T> register, string path, bool nullable)
    {
        AddRegister(path, register, nullable);
        return register;
    }

    public static IEnumerableRegister<RegKey, T> AddToRegisterHub<T>(this IEnumerableRegister<RegKey, T> register, string path) where T : notnull
    {
        AddRegister(path, register, false);
        return register;
    }

    public static IEnumerableRegister<RegKey, T> AddToRegisterHubNullable<T>(this IEnumerableRegister<RegKey, T> register, string path)
    {
        AddRegister(path, register, true);
        return register;
    }

    public static IRegister<RegKey, T> AddToRegisterHub<T>(this IRegister<RegKey, T> register, string path, bool nullable)
    {
        AddRegister(path, register, nullable);
        return register;
    }

    public static IRegister<RegKey, T> AddToRegisterHub<T>(this IRegister<RegKey, T> register, string path) where T : notnull
    {
        AddRegister(path, register, false);
        return register;
    }

    public static IRegister<RegKey, T> AddToRegisterHubNullable<T>(this IRegister<RegKey, T> register, string path)
    {
        AddRegister(path, register, true);
        return register;
    }
}