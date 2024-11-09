using System;

namespace BabelRush.Data;

#pragma warning disable CS9113

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModelSetAttribute(string sortName) : Attribute;
