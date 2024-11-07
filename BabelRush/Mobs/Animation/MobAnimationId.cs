using System.Collections.Generic;
using System.Collections.Immutable;

using Godot;

using KirisameLib.Core.Extensions;

namespace BabelRush.Mobs.Animation;

public class MobAnimationId
{
    #region Factory

    private MobAnimationId(IEnumerable<string> stateParts, IEnumerable<string> actionParts, string fullName)
    {
        FullName = fullName;
        NameId = new(fullName);
        StateParts = [..stateParts];
        ActionParts = [..actionParts];
    }


    private static readonly Dictionary<string, MobAnimationId> Cache = [];

    public static MobAnimationId Get(IList<string> stateParts, IList<string> actionParts)
    {
        var key = actionParts.Count == 0
            ? stateParts.Join('.') : $"{stateParts.Join('.')}:{actionParts.Join('.')}";
        if (Cache.TryGetValue(key, out var result)) return result;
        result = new MobAnimationId(stateParts, actionParts, key);
        Cache.Add(key, result);
        return result;
    }

    public static MobAnimationId Get(string state, string action)
    {
        var key = action == "" ? state : $"{state}:{action}";
        if (Cache.TryGetValue(key, out var result)) return result;
        result = new(state.Split('.'), action.Split('.'), key);
        Cache.Add(key, result);
        return result;
    }

    public static MobAnimationId Get(string fullName)
    {
        if (Cache.TryGetValue(fullName, out var result)) return result;
        var (state, action) = fullName.Split(':', 2) switch
        {
            // ReSharper disable ConvertTypeCheckPatternToNullCheck
            [string s]           => (s, ""),
            [string s, string a] => (s, a),
            _                    => ("", "")
            // ReSharper restore ConvertTypeCheckPatternToNullCheck
        };
        result = new MobAnimationId(state.Split('.'), action.Split('.'), fullName);
        Cache.Add(fullName, result);
        return result;
    }

    public static MobAnimationId Default => Get("Idle");

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

    public static implicit operator MobAnimationId(string fullName) => Get(fullName);

    public static implicit operator string(MobAnimationId id) => id.FullName;

    public static implicit operator StringName(MobAnimationId id) => id.NameId;

    #endregion


    #region Backoff

    public IEnumerable<MobAnimationId> Backoff()
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