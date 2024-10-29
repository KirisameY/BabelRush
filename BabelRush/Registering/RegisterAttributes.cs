using System;

using BabelRush.Data;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Model;
using KirisameLib.Data.Register;

namespace BabelRush.Registering;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[Obsolete]
[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;

#region RegisterAttributes

//base
[AttributeUsage(AttributeTargets.Field)]
public abstract class RegisterAttribute(string path) : Attribute;

//impl
public sealed class DataRegisterAttribute<TModel, TTarget>(string path, params string[] waitFor) : RegisterAttribute(path)
    where TModel : IDataModel<TTarget>;

public sealed class ResRegisterAttribute<TModel, TTarget>(string path) : RegisterAttribute(path)
    where TModel : IResModel<TTarget>;

public sealed class LangRegisterAttribute<TModel, TTarget>(string path) : RegisterAttribute(path)
    where TModel : ILangModel<TTarget>;

#endregion