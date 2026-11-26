using JetBrains.Annotations;

namespace BabelRushR.Core.GamePlay;

[AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
public sealed class AutoGamePlayEventHandlerAttribute : Attribute; // todo: 这东西也要改名
