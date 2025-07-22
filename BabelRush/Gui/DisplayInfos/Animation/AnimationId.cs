using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Godot;

using KirisameLib.Extensions;

namespace BabelRush.Gui.DisplayInfos.Animation;

public class AnimationId
{
    #region Factory

    private AnimationId(IEnumerable<string> stateParts, IEnumerable<string> actionParts, string fullName)
    {
        FullName = fullName;
        NameId = new(fullName);
        StateParts = [..stateParts];
        ActionParts = [..actionParts];
    }


    private static readonly Dictionary<string, AnimationId> Cache = [];

    public static AnimationId Get(IList<string> stateParts, IList<string> actionParts)
    {
        var key = actionParts is [] ? stateParts.Join('.') : $"{stateParts.Join('.')}${actionParts.Join('.')}";
        if (Cache.TryGetValue(key, out var result)) return result;
        result = new AnimationId(stateParts, actionParts, key);
        Cache.Add(key, result);
        return result;
    }

    public static AnimationId Get(string state, string action)
    {
        var fullName = action == "" ? state : $"{state}${action}";
        if (Cache.TryGetValue(fullName, out var result)) return result;
        result = new(state.Split('.'), action.Split('.'), fullName);
        Cache.Add(fullName, result);
        return result;
    }

    public static AnimationId Get(string fullName)
    {
        if (Cache.TryGetValue(fullName, out var result)) return result;
        var (state, action) = fullName.Split('$', 2) switch
        {
            // ReSharper disable ConvertTypeCheckPatternToNullCheck
            [string s]           => (s.Split('.'), []),
            [string s, string a] => (s.Split('.'), a.Split('.')),
            _                    => ([], [])
            // ReSharper restore ConvertTypeCheckPatternToNullCheck
        };
        result = new AnimationId(state, action, fullName);
        Cache.Add(fullName, result);
        return result;
    }

    public static AnimationId Default => Get("idle");

    #endregion


    #region Data

    public ImmutableArray<string> StateParts { get; }
    public ImmutableArray<string> ActionParts { get; }
    public string FullName { get; }
    public StringName NameId { get; }

    public bool IsAction => !ActionParts.IsDefaultOrEmpty;

    #endregion


    #region Convert

    public override string ToString() => FullName;

    [return: NotNullIfNotNull("fullName")]
    public static implicit operator AnimationId?(string? fullName) => fullName is null ? null : Get(fullName);

    [return: NotNullIfNotNull("id")]
    public static implicit operator string?(AnimationId? id) => id?.FullName;

    [return: NotNullIfNotNull("id")]
    public static implicit operator StringName?(AnimationId? id) => id?.NameId;

    #endregion


    #region Backoff

    public IEnumerable<AnimationId> Backoff()
    {
        if (IsAction)
        {
            for (int s = StateParts.Length; s > 0; s--)
            {
                for (int a = ActionParts.Length; a > 0; a--)
                {
                    yield return Get(StateParts[..s], ActionParts[..a]);
                }
            }
        }
        else
        {
            for (int s = StateParts.Length; s > 0; s--)
            {
                yield return Get(StateParts[..s], []);
            }
        }
    }

    #endregion
}