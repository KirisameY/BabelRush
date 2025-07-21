using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BabelRush.Data;
using BabelRush.Level.Scenery;
using BabelRush.Mobs;
using BabelRush.Registers;

namespace BabelRush.Level.Rooms;

public abstract partial record RoomObject
{
    [GeneratedRegex(@"^\s*(?<type>\S*)\s*\(\s*(?<params>.*)\s*\)\s*$", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex ObjectPattern { get; }

    public abstract IEnumerable<SceneObject> CreateObject();


    /// <exception cref="ArgumentException"></exception>
    public static RoomObject FromString(string from)
    {
        var match = ObjectPattern.Match(from);
        if (!match.Success) throw new ArgumentException($"Invalid room object format: {from}");
        var type = match.Groups["type"].Value.ToLower();
        var @params = match.Groups["params"].Value.Split(',', StringSplitOptions.TrimEntries);
        return (type, @params) switch
        {
            ("mob", [var id, var alignment]) => new Mob(id, Enum.Parse<Alignment>(alignment)),
            ("marker", [var id])             => new Marker(id),
            _                                => throw new ArgumentException($"Invalid room object format: {from}")
        };
    }

    public static bool CheckString(string from) => ObjectPattern.IsMatch(from);


    #region Implements

    public sealed record Mob(RegKey Id, Alignment Alignment) : RoomObject
    {
        public override IEnumerable<SceneObject> CreateObject() => [MobRegisters.Mobs[Id].GetInstance(Alignment)];
    }

    public sealed record Marker(string Mark) : RoomObject
    {
        public override IEnumerable<SceneObject> CreateObject() => [];
    }

    #endregion
}