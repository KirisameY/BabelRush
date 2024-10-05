using System;
using System.Collections.Frozen;
using System.Linq;
using System.Reflection;

using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public static class RegisterMapHelper
{
    public static void RegisterMapInAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
                            .Where(type => type.GetCustomAttribute<RegisterContainerAttribute>() != null);
        foreach (var type in types)
            RegisterMapInClass(type);
    }

    public static void RegisterMapInClass(Type containerClass)
    {
        var properties =
            containerClass.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
                          .Where(property => property.GetCustomAttribute<RegistrationMapAttribute>() != null);
        foreach (var property in properties)
        {
            if (!property.CanRead || property.CanWrite)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterMapHelper),
                           $"Property {property.Name} should be readonly, registration skipped.");
                continue;
            }
            if (property.PropertyType == typeof(FrozenDictionary<string, Registration.ParseAndRegisterDelegate>))
            {
                var dict = (FrozenDictionary<string, Registration.ParseAndRegisterDelegate>)
                    property.GetGetMethod()!.Invoke(null, null)!;
                foreach (var pair in dict)
                    Registration.RegisterMap(pair.Key, pair.Value);
            }
            else if (property.PropertyType == typeof(FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate>))
            {
                var dict = (FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate>)
                    property.GetGetMethod()!.Invoke(null, null)!;
                foreach (var pair in dict)
                    Registration.RegisterMap(pair.Key, pair.Value);
            }
            else
            {
                Logger.Log(LogLevel.Error, nameof(RegisterMapHelper),
                           $"unexpected type of property {property.Name} , registration skipped.");
            }
        }
    }

    private static Logger Logger { get; } = LogManager.GetLogger(nameof(RegisterMapHelper));
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;