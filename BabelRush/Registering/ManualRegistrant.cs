using System.Collections.Generic;
using System.Linq;

using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering;

public static class ManualRegistrant<TItem> //todo:这里是泛型的话外面没法统一获取了，类型问题得想办法啊！
{
    #region Common

    private static readonly Dictionary<(string Root, string Path), IRegTarget<TItem>> CommonTargets = [];

    internal static IEnumerable<(string Root, string Path, IRegTarget<TItem>)> GetCommonTargets() =>
        CommonTargets.Select(p => (p.Key.Root, p.Key.Path, p.Value));


    public static IRegistrant<TItem> Common(string root, string path) => new ManualCommonRegistrant(root, path);

    private readonly struct ManualCommonRegistrant(string root, string path) : IRegistrant<TItem>
    {
        public void AcceptTarget(IRegTarget<TItem> target) => CommonTargets.Add((root, path), target);
    }

    #endregion


    #region I18n

    // ReSharper disable InconsistentNaming

    private static readonly Dictionary<(string Root, string Path), II18nRegTarget<TItem>> I18nTargets = [];

    internal static IEnumerable<(string lang, string Root, string Path, IRegTarget<TItem>)> GetI8nTargets(string local)
    {
        List<(string lang, string Root, string Path, IRegTarget<TItem>)> result = [];
        foreach (var ((root, path), target) in I18nTargets)
        {
            target.UpdateLocal(local, l => new ManualL10nRegistrant(result, l, root, path));
        }

        return result.AsReadOnly();
    }


    public static II18nRegistrant<TItem> I18n(string root, string path) => new ManualI18nRegistrant(root, path);

    private readonly struct ManualI18nRegistrant(string root, string path) : II18nRegistrant<TItem>
    {
        public void AcceptRegister(II18nRegTarget<TItem> register) => I18nTargets.Add((root, path), register);
    }

    private readonly struct ManualL10nRegistrant(
        List<(string lang, string Root, string Path, IRegTarget<TItem>)> list, string lang, string root, string path)
        : IRegistrant<TItem>
    {
        public void AcceptTarget(IRegTarget<TItem> target) => list.Add((lang, root, path, target));
    }

    // ReSharper restore InconsistentNaming

    #endregion
}