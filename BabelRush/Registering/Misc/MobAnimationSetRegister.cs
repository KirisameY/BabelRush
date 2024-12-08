using System.Collections.Generic;

using BabelRush.I18n;
using BabelRush.Mobs.Animation;

using KirisameLib.Data.Register;
using KirisameLib.Event;

namespace BabelRush.Registering.Misc;

[EventHandlerContainer]
internal sealed partial class MobAnimationSetRegister : IRegister<MobAnimationSet>
{
    private readonly MobAnimationSet _defaultSet;

    public MobAnimationSetRegister(MobAnimationSet defaultSet)
    {
        _defaultSet = defaultSet;
        SubscribeInstanceHandler(Game.EventBus);
    }

    private Dictionary<string, List<MobAnimationModel>> BaseRegDict { get; } = new();
    private Dictionary<string, Dictionary<string, List<MobAnimationModel>>> LocalRegDict { get; } = new();
    private Dictionary<string, (bool dirty, MobAnimationSet value)> Cache { get; } = new();

    public MobAnimationSet GetItem(string id)
    {
        string local = Game.Localization.Local;

        if (Cache.TryGetValue(id, out var cache) && !cache.dirty) return cache.value;

        List<MobAnimationModel>? baseSort, localSort = null;
        if (BaseRegDict.TryGetValue(id, out baseSort) |
            (LocalRegDict.TryGetValue(local, out var localReg) && localReg.TryGetValue(id, out localSort)))
        {
            var builder = new MobAnimationSetBuilder().SetId(id);
            foreach (var entry in baseSort ?? []) builder.SetAnimation(entry);
            foreach (var entry in localSort ?? []) builder.SetAnimation(entry);
            var result = builder.Build();
            Cache[id] = (false, result);
            return result;
        }

        return _defaultSet;
    }

    public bool ItemRegistered(string id)
    {
        return BaseRegDict.ContainsKey(id) ||
            (LocalRegDict.TryGetValue(Game.Localization.Local, out var reg) && reg.ContainsKey(id));
    }

    public bool RegisterDefault(string id, MobAnimationModel animation)
    {
        if (!BaseRegDict.TryGetValue(id, out var sort))
            BaseRegDict[id] = sort = [];
        sort.Add(animation);
        return true;
    }

    public bool RegisterLocal(string local, string id, MobAnimationModel animation)
    {
        if (!LocalRegDict.TryGetValue(local, out var reg))
            LocalRegDict[local] = reg = new();
        if (!reg.TryGetValue(id, out var sort))
            reg[id] = sort = [];
        sort.Add(animation);
        return true;
    }

    //EventHandler
    [EventHandler]
    private void OnLocalChanged(LocalChangedEvent e)
    {
        foreach (var id in Cache.Keys)
        {
            Cache[id] = Cache[id] with { dirty = true };
        }
    }
}