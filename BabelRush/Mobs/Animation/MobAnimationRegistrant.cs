using System;

using BabelRush.Data;

using KirisameLib.Data.Model;
using KirisameLib.Data.Registration;

namespace BabelRush.Mobs.Animation;

public class MobAnimationRegistrant : IRegistrant<ResSourceInfo>
{
    public virtual (string id, Func<bool> register)[] Parse(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        throw new NotImplementedException();
    }
}