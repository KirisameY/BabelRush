using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Extensions;

namespace BabelRush.Level.Stages.Template.GenerationRules;

[UsedImplicitly]
public class AfterRuleModel : IDictionary<string, object>
{
    #region Implement of dictionary

    private readonly IDictionary<string, object> _dictionaryImplementation = new Dictionary<string, object>();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _dictionaryImplementation.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dictionaryImplementation).GetEnumerator();
    }

    public void Add(KeyValuePair<string, object> item)
    {
        _dictionaryImplementation.Add(item);
    }

    public void Clear()
    {
        _dictionaryImplementation.Clear();
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return _dictionaryImplementation.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        _dictionaryImplementation.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
        return _dictionaryImplementation.Remove(item);
    }

    public int Count => _dictionaryImplementation.Count;
    public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

    public void Add(string key, object value)
    {
        _dictionaryImplementation.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _dictionaryImplementation.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _dictionaryImplementation.Remove(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        return _dictionaryImplementation.TryGetValue(key, out value);
    }

    public object this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }
    public ICollection<string> Keys => _dictionaryImplementation.Keys;
    public ICollection<object> Values => _dictionaryImplementation.Values;

    #endregion


    private static readonly Dictionary<string, (Func<AfterRuleModel, string[]> checker, Func<AfterRuleModel, string, AfterRule> parser)> Models = new()
    {
        ["replace"] = (model =>
        {
            if (!model.TryGetValue("room", out var outRoom) || outRoom is not string)
                return ["ReplaceRuleModel expected a string room id field."];
            return [];
        }, (model, nameSpace) =>
        {
            var roomId = (string)model["room"];
            var room = roomId.WithDefaultNameSpace(nameSpace);
            var rate = model.TryGetValue("rate", out var outRate) && outRate is double or long ? (double)outRate : 1;
            var ordinalFrom = (int)(model.GetOrDefault("ordinal_from") as long? ?? 0);
            var ordinalTo = (int)(model.GetOrDefault("ordinal_to") as long? ?? int.MaxValue);
            var parallelFrom = (int)(model.GetOrDefault("parallel_from") as long? ?? 0);
            var parallelTo = (int)(model.GetOrDefault("parallel_to") as long? ?? int.MaxValue);
            return new ReplaceRule(room, rate, ordinalFrom, ordinalTo, parallelFrom, parallelTo);
        }),

        ["combine"] = (model =>
        {
            if (!model.TryGetValue("ordinal", out var outOrdinal) || outOrdinal is not long)
                return ["ReplaceRuleModel expected a integer ordinal field."];
            if (!model.TryGetValue("new_pos", out var outNewPos) || outNewPos is not long)
                return ["ReplaceRuleModel expected a integer new_pos field."];
            if (!model.TryGetValue("new_display_pos", out var outNewDisplayPos) ||
                outNewDisplayPos is not IDictionary<string, object> newDisplayPos ||
                !newDisplayPos.TryGetValue("x", out var outX) || outX is not long or double ||
                !newDisplayPos.TryGetValue("y", out var outY) || outY is not long or double)
                return ["ReplaceRuleModel expected a vector2 new_display_pos(table with float x and y field) field."];
            return [];
        }, (model, nameSpace) =>
        {
            var ordinal = (int)model["ordinal"];
            var from = (int)(model.GetOrDefault("from") as long? ?? 0);
            var to = (int)(model.GetOrDefault("to") as long? ?? int.MaxValue);
            var newPos = (int)model["new_pos"];
            var newDisplayPos = new Vector2((float)((IDictionary<string, object>)model["new_display_pos"])["x"],
                                            (float)((IDictionary<string, object>)model["new_display_pos"])["y"]);
            var room = model.GetOrDefault("room") is string roomId ? roomId.WithDefaultNameSpace(nameSpace) : null;
            return new CombineRule(ordinal, from, to, newPos, newDisplayPos, room);
        }),
    };

    public AfterRule Convert(string nameSpace)
    {
        var type = (string)this["type"];
        return Models[type].parser.Invoke(this, nameSpace);
    }

    public bool Check(out string[] errors)
    {
        if (!TryGetValue("type", out var outType) || outType is not string type)
        {
            errors = ["AfterRuleModel expected a string type field."];
            return false;
        }
        errors = Models[type].checker.Invoke(this);
        return errors.Length == 0;
    }
}