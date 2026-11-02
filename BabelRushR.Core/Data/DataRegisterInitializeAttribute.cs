using JetBrains.Annotations;

namespace BabelRushR.Core.Data;

[AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
public class DataRegisterInitializeAttribute : Attribute;