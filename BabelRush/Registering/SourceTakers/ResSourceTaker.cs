using System;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public class ResSourceTaker(ISourceTaker<ResSourceInfo> innerRegistrant) : ISourceTaker<ResSourceInfo>
{
    //Getter

    public static ResSourceTaker Get<TModel, TTarget>(RegisterItem<TTarget> register) where TModel : IModel<ResSourceInfo, TTarget>
    {
        return new(new CommonSourceTaker<ResSourceInfo, TModel, TTarget>(register));
    }

    public static ResSourceTaker Get<TModel, TTarget>(RegisterLocalItem<TTarget> register) where TModel : IModel<ResSourceInfo, TTarget>
    {
        return new(new LocalizedSourceTaker<ResSourceInfo, TModel, TTarget>(register, () => FileLoader.CurrentLocal));
    }


    //Implement

    public (string id, Func<bool> register)[] Take(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        return innerRegistrant.Take(source, out errorMessages);
    }
}