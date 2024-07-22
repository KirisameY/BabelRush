using Godot;

using KirisameLib.Logging;

namespace BabelRush.Gui.Mob;

public partial class MobInterface : Node
{
    private Mobs.Mob? _mob;
    public Mobs.Mob Mob
    {
        get
        {
            if (_mob is not null) return _mob;
            Logger.Log(LogLevel.Error, "GettingMob", $"MobInterface {this} has no mob instance reference");
            return Mobs.Mob.Default;
        }
        private set => _mob = value;
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(MobInterface));
}