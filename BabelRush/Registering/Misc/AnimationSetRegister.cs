using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Gui.DisplayInfos.Animation;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;
using KirisameLib.Event;
using KirisameLib.Extensions;

namespace BabelRush.Registering.Misc;

internal sealed class AnimationSetRegister : IRegister<RegKey, AnimationSet>, II18nRegTarget<AnimationModel>
{
    public AnimationSetRegister(string path)
    {
        // ModelReg = SimpleRegisterCreate.Res<MobAnimationModel, MobAnimationModel>(path, MobAnimationModel.Default);
        ModelReg = new I18nRegisterBuilder<AnimationModel>()
                  .WithFallback(new RegisterBuilder<RegKey, AnimationModel>()
                               .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
                               .AddRegistrant(MakeRegistrant.ForCommonRes<AnimationModel, AnimationModel>(path))
                               .WithFallback(AnimationModel.Default)
                               .Build())
                  .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
                  .Build();
        MakeRegistrant.ForLocalRes<AnimationModel, AnimationModel>(path).AcceptTarget(this);
    }


    #region Fields

    private I18nRegister<AnimationModel> ModelReg { get; }
    private Dictionary<RegKey, AnimationSet> FinalReg { get; } = new();

    private bool _isRegistering = false;

    #endregion


    #region Registering

    public void UpdateLocal(string local, Func<string, IRegistrant<RegKey, AnimationModel>> registrantCreator)
    {
        ModelReg.UpdateLocal(local, registrantCreator);

        if (_isRegistering) return;
        _isRegistering = true;

        Game.LoadEventBus.Subscribe<LocalRegisterDoneEvent>(_ =>
        {
            FinalReg.Clear();
            ModelReg.Values.GroupBy(model => model.SetId).ForEach(group =>
            {
                var builder = new AnimationSetBuilder(group.Key);
                group.ForEach(model => builder.AddAnimation(model));
                FinalReg[group.Key] = builder.Build();
            });
            _isRegistering = false;
        }, HandlerSubscribeFlag.OnlyOnce);
    }

    #endregion


    public AnimationSet this[RegKey id] => GetItem(id);

    public AnimationSet GetItem(RegKey id) => FinalReg.GetOrDefault(id, AnimationSet.Default)!;

    public bool ItemRegistered(RegKey id) => FinalReg.ContainsKey(id);
}