using System;
using System.Linq;

using BabelRush.Data;
using BabelRush.Mobs.Animation;

using KirisameLib.Data.Model;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering.Misc;

internal class MobAnimationDefaultRegistrant(MobAnimationSetRegister register) : IRegistrant<ResSourceInfo>
{
    public (string id, Func<bool> register)[] Parse(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        var models = MobAnimationModel.FromSource(source, out errorMessages);
        return models.Select(model => (model.Id, GetRegister(model.Convert()))).ToArray();
    }

    private Func<bool> GetRegister(MobAnimationModel model)
    {
        return () => register.RegisterDefault(model.Id, model);
    }
}

internal class MobAnimationLocalizedRegistrant(MobAnimationSetRegister register) : IRegistrant<ResSourceInfo>
{
    public (string id, Func<bool> register)[] Parse(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        var models = MobAnimationModel.FromSource(source, out errorMessages);
        return models.Select(model => (model.Id, GetRegister(model.Convert()))).ToArray();
    }

    private Func<bool> GetRegister(MobAnimationModel model)
    {
        return () => register.RegisterLocal(FileLoader.CurrentLocal, model.Id, model);
    }
}