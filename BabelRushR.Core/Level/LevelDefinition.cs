using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.Level;

[DataRegisterInitialize]
public class LevelDefinition
{
    public static ILogger Logger => field ??= Logging.Logger(typeof(LevelDefinition));

    public static ILevelDefinition Fallback => PlaceHolderLevelDefinition.Instance;

    public static IEnumerableRegistry<ILevelDefinition> Registry { get; } =
        new RegistryBuilder<ILevelDefinition>()
           .WithFallback(RegistryCreateUtils.CreateFallback(Fallback, Logger, "LevelDefinition"))
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}