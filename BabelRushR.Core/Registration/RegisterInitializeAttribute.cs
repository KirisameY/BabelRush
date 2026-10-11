using JetBrains.Annotations;

namespace BabelRushR.Core.Registration;

[AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
public class RegisterInitializeAttribute : Attribute;