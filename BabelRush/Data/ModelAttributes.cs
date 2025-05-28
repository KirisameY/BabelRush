using System;

namespace BabelRush.Data;

#pragma warning disable CS9113

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModelAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModelSetAttribute(string sortName) : ModelAttribute;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ModelSetAttribute<TModel>(string sortName) : ModelAttribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NecessaryPropertyAttribute : Attribute;