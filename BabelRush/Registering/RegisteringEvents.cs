using BabelRush.Data;

using KirisameLib.Data.Registering;
using KirisameLib.Event;

namespace BabelRush.Registering;

public abstract record RegisteringEvent : BaseEvent;

//
public abstract record RegisterStartEvent : RegisteringEvent;

public sealed record CommonRegisterStartEvent : RegisterStartEvent;

public sealed record LocalRegisterStartEvent(string Local) : RegisterStartEvent;

//
public abstract record RegisterDoneEvent : RegisteringEvent;

public sealed record CommonRegisterDoneEvent : RegisterDoneEvent;

public sealed record LocalRegisterDoneEvent(string Local) : RegisterDoneEvent;

//
public abstract record ManualRegisterEvent : RegisteringEvent;

public abstract record CommonManualRegisterEvent(string Root, string Path) : ManualRegisterEvent;

public sealed record CommonManualRegisterEvent<T>(string Root, string Path, IRegTarget<RegKey, T> Reg) : CommonManualRegisterEvent(Root, Path);

public abstract record LocalManualRegisterEvent(string Local, string Root, string Path) : ManualRegisterEvent;

public sealed record LocalManualRegisterEvent<T>(string Local, string Root, string Path, IRegTarget<RegKey, T> Reg) : LocalManualRegisterEvent(Local, Root, Path);