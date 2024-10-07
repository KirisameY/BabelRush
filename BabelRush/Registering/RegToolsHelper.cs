using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public static class RegToolsHelper
{
    public static void RegisterRegToolsInAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
                            .Where(type => type.GetCustomAttribute<RegisterContainerAttribute>() != null);
        foreach (var type in types)
            RegisterRegToolsInClass(type);
    }

    public static void RegisterRegToolsInClass(Type containerClass)
    {
        var properties =
            containerClass.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
                          .Where(property => property.GetCustomAttribute<RegistrationMapAttribute>() != null);
        foreach (var property in properties)
        {
            if (!property.CanRead || property.CanWrite)
            {
                Logger.Log(LogLevel.Error, nameof(RegToolsHelper),
                           $"Property {property.Name} should be readonly, registration skipped.");
                continue;
            }
            if (typeof(IEnumerable<AssetRegTool>).IsAssignableFrom(property.PropertyType))
            {
                var enumerable = property.GetValue(null) as IEnumerable<AssetRegTool>;
                foreach (var tool in enumerable!)
                    Registration.RegisterMap(tool.Path, tool);
                Logger.Log(LogLevel.Debug, nameof(RegToolsHelper), $"Registered asset reg tools in {property.Name}");
            }
            else
            {
                Logger.Log(LogLevel.Error, nameof(RegToolsHelper),
                           $"unexpected type of property: {property.Name} , registration skipped.");
            }
        }
    }

    private static Logger Logger { get; } = LogManager.GetLogger(nameof(RegToolsHelper));
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterContainerAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class RegistrationMapAttribute : Attribute;