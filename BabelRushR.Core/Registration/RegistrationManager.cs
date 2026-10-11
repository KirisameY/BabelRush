using System.Reflection;
using System.Runtime.CompilerServices;

using KirisameY.Registration.Registering;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Core.Registration;

public static class RegistrationManager
{
    public static TRegistrant Attach<TRegistrant>(TRegistrant registrant)
        where TRegistrant : BabelRegistrant
    {
        RegisterStart += registrant.Work;
        return registrant;
    }

    internal static void InitializeRegisters()
    {
        // initialize registers
        var selfAsm = Assembly.GetAssembly(typeof(RegistrationManager));
        AppDomain.CurrentDomain
                 .GetAssemblies()
                 .Where(asm => asm.GetReferencedAssemblies().Contains(selfAsm!.GetName()))
                 .SelectMany(assembly => assembly.DefinedTypes)
                 .Where(type => type.GetCustomAttribute<RegisterInitializeAttribute>() is not null)
                 .ForEach(type => RuntimeHelpers.RunClassConstructor(type.TypeHandle));

        RegisterStart?.Invoke();
        RegisterDone?.Invoke();
    }

    private static event Action? RegisterStart;

    private static readonly DoneEventSource _doneSource = new();
    public static IRegisterDoneEventSource DoneSource => _doneSource;

    private static event Action? RegisterDone;

    private class DoneEventSource : IRegisterDoneEventSource
    {
        event Action? IRegisterDoneEventSource.RegisterDone
        {
            add => RegisterDone += value;
            remove => RegisterDone -= value;
        }
    }
}