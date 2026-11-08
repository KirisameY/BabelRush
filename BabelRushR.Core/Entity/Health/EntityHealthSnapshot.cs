namespace BabelRushR.Core.Entity.Health;

public readonly record struct EntityHealthSnapshot(int MaxHP, int MaxShield, int HP, int Shield, int Block);

public static class EntityHealthSnapshottingExtensions
{
    extension(IEntityHealth source)
    {
        public EntityHealthSnapshot Snapshot() => new(
            source.MaxHP.Value,
            source.MaxShield.Value,
            source.HP,
            source.Shield,
            source.Block
        );
    }
}