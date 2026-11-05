using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

using Microsoft.Extensions.Logging;

using Zio;

using ZLogger;

namespace BabelRushR.Core.Data;

public static class RegistryCreatUtils
{
    public static Func<RegKey, T> CreateFallback<T>(T value, ILogger logger, string registryName) =>
        CreateFallback(_ => value, logger, registryName);

    public static Func<RegKey, T> CreateFallback<T>(Func<RegKey, T> getter, ILogger logger, string registryName) => key =>
    {
        logger.ZLogWarning($"{registryName} {key} not found in registry, fallback value used.");
        return getter.Invoke(key);
    };

    public static IRegistrant<T> DataRegistrant<T>(UPath path, Func<T, RegKey> idGetter) =>
        DataRegistrationManager.Attach(new CommonTomlDataRegistrant<T>(path, idGetter));

    public static IRegistrant<T> ListDataRegistrant<T>(UPath path, string rootKey, Func<T, RegKey> idGetter) =>
        DataRegistrationManager.Attach(new ListedTomlDataRegistrant<T>(path, rootKey, idGetter));
}