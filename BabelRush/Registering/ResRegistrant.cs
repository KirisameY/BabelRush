using System;

using BabelRush.Data;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class ResRegistrant : Registrant<ResSourceInfo>
{
    //Getter
    private ResRegistrant(Registrant<ResSourceInfo> registrant)
    {
        _registrant = registrant;
    }

    public static ResRegistrant Get<TModel, TTarget>(CommonRegister<TTarget> register) where TModel : IModel<ResSourceInfo, TTarget>
    {
        return new(new CommonRegistrant<ResSourceInfo, TModel, TTarget>(register));
    }

    public static ResRegistrant Get<TModel, TTarget>(LocalizedRegister<TTarget> register) where TModel : IModel<ResSourceInfo, TTarget>
    {
        return new(new LocalizedRegistrant<ResSourceInfo, TModel, TTarget>(register, () => FileLoader.CurrentLocal));
    }


    //Implement
    private readonly Registrant<ResSourceInfo> _registrant;

    public override (string id, Func<bool> register)[] Parse(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        return _registrant.Parse(source, out errorMessages);
    }
}