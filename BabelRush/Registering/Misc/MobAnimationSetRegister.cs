using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Mobs.Animation;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;
using KirisameLib.Extensions;

namespace BabelRush.Registering.Misc;

internal sealed class MobAnimationSetRegister : IRegister<MobAnimationSet>, II18nRegTarget<MobAnimationModel>
{
    public MobAnimationSetRegister(string path)
    {
        ModelReg = SimpleRegisterCreate.Res<MobAnimationModel, MobAnimationModel>(path, MobAnimationModel.Default);
        MakeFileRegistrant.ForLocalRes<MobAnimationModel, MobAnimationModel>(path).AcceptRegister(this);
    }

    #region Fields

    private I18nRegister<MobAnimationModel> ModelReg { get; }
    private Dictionary<string, MobAnimationSet> FinalReg { get; } = new();
    private static readonly IRegisterDoneEventSource RegisterDoneEventSource = RegisterEventSource.LocalRegisterDone;

    #endregion


    #region Registering

    public void UpdateLocal(string local, Func<string, IRegistrant<MobAnimationModel>> registrantCreator)
    {
        ModelReg.UpdateLocal(local, registrantCreator);
        RegisterDoneEventSource.RegisterDone += () =>
        {
            FinalReg.Clear();
            var groups = ModelReg.Values.GroupBy(model => model.MobId);
            foreach (var group in groups)
            {
                var builder = new MobAnimationSetBuilder(group.Key);
                foreach (var model in group) builder.AddAnimation(model);
                FinalReg[group.Key] = builder.Build();
            }
        };
    }

    #endregion


    public MobAnimationSet GetItem(string id) => FinalReg.GetOrDefault(id, MobAnimationSet.Default)!;

    public bool ItemRegistered(string id) => FinalReg.ContainsKey(id);
}