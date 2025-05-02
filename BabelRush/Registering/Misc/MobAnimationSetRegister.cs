using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Mobs.Animation;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;
using KirisameLib.Extensions;

namespace BabelRush.Registering.Misc;

internal sealed class MobAnimationSetRegister : IRegister<RegKey, MobAnimationSet>, II18nRegTarget<MobAnimationModel>
{
    public MobAnimationSetRegister(string path)
    {
        // ModelReg = SimpleRegisterCreate.Res<MobAnimationModel, MobAnimationModel>(path, MobAnimationModel.Default);
        ModelReg = new I18nRegisterBuilder<MobAnimationModel>()
                  .WithFallback(new RegisterBuilder<RegKey, MobAnimationModel>()
                               .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
                               .AddRegistrant(MakeRegistrant.ForCommonRes<MobAnimationModel, MobAnimationModel>(path))
                               .WithFallback(MobAnimationModel.Default)
                               .Build())
                  .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
                  .Build();
        MakeRegistrant.ForLocalRes<MobAnimationModel, MobAnimationModel>(path).AcceptTarget(this);
    }


    #region Fields

    private I18nRegister<MobAnimationModel> ModelReg { get; }
    private Dictionary<RegKey, MobAnimationSet> FinalReg { get; } = new();

    private bool _isRegistering = false;

    #endregion


    #region Registering

    public void UpdateLocal(string local, Func<string, IRegistrant<RegKey, MobAnimationModel>> registrantCreator)
    {
        ModelReg.UpdateLocal(local, registrantCreator);

        if (_isRegistering) return;
        _isRegistering = true;
        Game.LoadEventBus.Subscribe<LocalRegisterDoneEvent>(_ =>
        {
            FinalReg.Clear();
            var groups = ModelReg.Values.GroupBy(model => model.SetId); //todo: 改RegKey的后续处理
            foreach (var group in groups)
            {
                var builder = new MobAnimationSetBuilder(group.Key);
                group.ForEach(model => builder.AddAnimation(model));
                FinalReg[group.Key] = builder.Build();
            }
            _isRegistering = false;
        });
    }

    #endregion


    public MobAnimationSet this[RegKey id] => GetItem(id);

    public MobAnimationSet GetItem(RegKey id) => FinalReg.GetOrDefault(id, MobAnimationSet.Default)!;

    public bool ItemRegistered(RegKey id) => FinalReg.ContainsKey(id);
}