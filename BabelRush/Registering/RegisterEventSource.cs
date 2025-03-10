using System;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering;

public static class RegisterEventSource
{
    private class RegisterDoneEventSource : IRegisterDoneEventSource
    {
        public event Action? RegisterDone;

        public void Trigger()
        {
            RegisterDone?.Invoke();
            RegisterDone = null;
        }
    }

    // ReSharper disable InconsistentNaming
    private static readonly RegisterDoneEventSource _commonRegisterDone = new();
    private static readonly RegisterDoneEventSource _localRegisterDone = new();
    // ReSharper restore InconsistentNaming

    public static IRegisterDoneEventSource CommonRegisterDone => _commonRegisterDone;
    public static IRegisterDoneEventSource LocalRegisterDone => _localRegisterDone;

    internal static void TriggerCommonRegisterDone() => _commonRegisterDone.Trigger();
    internal static void TriggerLocalRegisterDone() => _localRegisterDone.Trigger();
}