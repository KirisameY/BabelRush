using System;

using BabelRush.Data;

namespace BabelRush.Registering;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[Obsolete]
[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;

#region RegisterAttributes

#pragma warning disable CS9113
// ReSharper disable UnusedTypeParameter

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

// ReSharper restore UnusedTypeParameter
#pragma warning restore CS9113

#endregion