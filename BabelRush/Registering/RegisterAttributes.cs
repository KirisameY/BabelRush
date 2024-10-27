using System;
using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Model;
using KirisameLib.Data.Register;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[Obsolete]
[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;

#region RegisterAttributes

//base
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public abstract class RegisterAttribute(string path) : Attribute
{
    public string Path { get; } = path;
}

public abstract class DefaultRegisterAttribute<TRegistrant, TSource, TTarget>(string path)
    : RegisterAttribute(path)
    where TRegistrant : Registrant<TSource>
{
    public abstract TRegistrant GetRegistrant(CommonRegister<TTarget> register);
}

public abstract class LocalRegisterAttribute<TRegistrant, TSource, TTarget>(string path)
    : RegisterAttribute(path)
    where TRegistrant : Registrant<TSource>
{
    public abstract TRegistrant GetRegistrant(LocalizedRegister<TTarget> register);
}

//impl
public class DataRegisterAttribute<TModel, TTarget>(string path, params string[] waitFor)
    : DefaultRegisterAttribute<DataRegistrant, byte[], TTarget>(path)
    where TModel : IDataModel<TTarget>
{
    public override DataRegistrant GetRegistrant(CommonRegister<TTarget> register) =>
        DataRegistrant.Get<TModel, TTarget>(register, waitFor);
}

public class ResDefaultRegisterAttribute<TModel, TTarget>(string path)
    : DefaultRegisterAttribute<ResRegistrant, ResSourceInfo, TTarget>(path)
    where TModel : IResModel<TTarget>
{
    public override ResRegistrant GetRegistrant(CommonRegister<TTarget> register) =>
        ResRegistrant.Get<TModel, TTarget>(register);
}

public class ResLocalRegisterAttribute<TModel, TTarget>(string path)
    : LocalRegisterAttribute<ResRegistrant, ResSourceInfo, TTarget>(path)
    where TModel : IResModel<TTarget>
{
    public override ResRegistrant GetRegistrant(LocalizedRegister<TTarget> register) =>
        ResRegistrant.Get<TModel, TTarget>(register);
}

public class LangRegisterAttribute<TModel, TTarget>(string path)
    : LocalRegisterAttribute<LangRegistrant, IDictionary<string, object>, TTarget>(path)
    where TModel : ILangModel<TTarget>
{
    public override LangRegistrant GetRegistrant(LocalizedRegister<TTarget> register) =>
        LangRegistrant.Get<TModel, TTarget>(register);
}

#endregion