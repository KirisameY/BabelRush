using System.Reflection;

using BabelRushR.Core.Common.Serializing;

using Tomlyn;

namespace BabelRushR.Core.Test;

internal static class SerializingOptionsTestAccessor
{
    private static readonly FieldInfo InitializedField = typeof(SerializingOptions)
        .GetField("_initialized", BindingFlags.Static | BindingFlags.NonPublic)
     ?? throw new MissingFieldException(typeof(SerializingOptions).FullName, "_initialized");

    private static readonly MethodInfo DefaultSetter = (typeof(SerializingOptions)
        .GetProperty(nameof(SerializingOptions.Default), BindingFlags.Static | BindingFlags.Public)
     ?? throw new MissingMemberException(typeof(SerializingOptions).FullName, nameof(SerializingOptions.Default)))
       .GetSetMethod(nonPublic: true)
     ?? throw new MissingMethodException(typeof(SerializingOptions).FullName, "set_Default");

    private static readonly FieldInfo DefaultNamespaceCacheField = typeof(SerializingOptions)
        .GetField("_withDefaultNsCache", BindingFlags.Static | BindingFlags.NonPublic)
     ?? throw new MissingFieldException(typeof(SerializingOptions).FullName, "_withDefaultNsCache");

    // 生产里 CollectOptions 只跑一次；测试在同一进程里反复加载，必须把一次性状态和派生缓存一起复位。
    // 私有状态用反射复位，不为测试把字段提升到 internal。
    public static void Reset()
    {
        InitializedField.SetValue(null, false);
        DefaultSetter.Invoke(null, [null]);

        // 缓存的 options 引用上一轮 Default 的转换器实例，不复位会让后续用例拿到过期配置。
        ((Dictionary<string, TomlSerializerOptions>)DefaultNamespaceCacheField.GetValue(null)!).Clear();
    }

    public static void ResetAndInitialize()
    {
        Reset();
        SerializingOptions.CollectOptions();
    }
}
