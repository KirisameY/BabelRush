using System;
using System.Collections.Generic;

using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering;

public static class ManualRegistrant
{
    #region Common

    private static readonly List<Action> CommonTargets = [];

    internal static void PublishCommonEvents() => CommonTargets.ForEach(static a => a.Invoke());


    public static ManualCommonRegistrant<TItem> Common<TItem>(string root, string path) => new(root, path);

    public readonly struct ManualCommonRegistrant<TItem>(string root, string path) : IRegistrant<TItem>
    {
        public void AcceptTarget(IRegTarget<TItem> target)
        {
            var (root1, path1) = (root, path);
            CommonTargets.Add(() => Game.LoadEventBus.Publish(new CommonManualRegisterEvent<TItem>(root1, path1, target)));
        }
    }

    #endregion


    #region I18n

    // ReSharper disable InconsistentNaming

    private static readonly List<Action<string>> I18nTargets = [];

    internal static void PublishI18nEvents(string local) => I18nTargets.ForEach(a => a.Invoke(local));


    public static ManualI18nRegistrant<TItem> I18n<TItem>(string root, string path) => new(root, path);

    public readonly struct ManualI18nRegistrant<TItem>(string root, string path) : II18nRegistrant<TItem>
    {
        public void AcceptTarget(II18nRegTarget<TItem> target)
        {
            var (root1, path1) = (root, path);
            I18nTargets.Add(local =>
            {
                List<(string Lang, IRegTarget<TItem> target)> targets = [];
                target.UpdateLocal(local, l => new ManualL10nRegistrant<TItem>(targets, l));
                foreach (var (local1, target1) in targets)
                {
                    Game.LoadEventBus.Publish(new LocalManualRegisterEvent<TItem>(local1, root1, path1, target1));
                }
            });
        }
    }

    private readonly struct ManualL10nRegistrant<TItem>(
        List<(string lang, IRegTarget<TItem>)> list, string lang)
        : IRegistrant<TItem>
    {
        public void AcceptTarget(IRegTarget<TItem> target) => list.Add((lang, target));
    }

    // ReSharper restore InconsistentNaming

    #endregion
}