namespace BabelRush.Mobs.Actions;

public class CommonMobActionStrategizer(CommonMobActionStrategy strategy, Mob mob) : MobActionStrategizer(strategy, mob)
{
    private string _state = "default";

    public override MobAction? GetNextAction()
    {
        var template = strategy.GetNext(_state);
        if (template is null) return null;
        if (template is { ConvertState: { } state }) _state = state;
        return template.NewInstance(Mob);
    }
}