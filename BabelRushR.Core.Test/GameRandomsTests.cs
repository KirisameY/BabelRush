using BabelRushR.Core.GamePlay;

using KirisameY.Randomizer;
using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class GameRandomsTests
{
    private static readonly RegKey Damage = RegKey.From("test", "damage");

    private static RegKey Key(string key) => RegKey.From("test", key);

    private static ulong[] Sequence(IRandomizer randomizer, int count = 8) =>
        [..Enumerable.Range(0, count).Select(_ => randomizer.NextU64())];

    [Fact]
    public void Seed_Is_Exposed_As_Given()
    {
        Assert.Equal(42ul, new GameRandoms(42).Seed);
        Assert.Equal(0ul, new GameRandoms(0).Seed);
    }

    [Fact]
    public void Same_Seed_And_Scope_Produce_The_Same_Sequence()
    {
        var first = new GameRandoms(1234).GetRandomizer(Damage);
        var second = new GameRandoms(1234).GetRandomizer(Damage);

        Assert.Equal(Sequence(first), Sequence(second));
    }

    [Fact]
    public void Same_Seed_Reproduces_Every_Scope()
    {
        var first = new GameRandoms(2026);
        var second = new GameRandoms(2026);

        foreach (var scope in new[] { Damage, Key("shuffle"), RegKey.From("other", "damage") })
        {
            Assert.Equal(Sequence(first.GetRandomizer(scope)), Sequence(second.GetRandomizer(scope)));
        }
    }

    [Fact]
    public void Different_Seeds_Produce_Different_Sequences()
    {
        var first = new GameRandoms(1).GetRandomizer(Damage);
        var second = new GameRandoms(2).GetRandomizer(Damage);

        Assert.NotEqual(Sequence(first), Sequence(second));
    }

    [Fact]
    public void Different_Scopes_Produce_Different_Sequences()
    {
        // 同一命名空間的不同 key、以及不同命名空間的同名 key 都該是不同的流。
        var damage = Sequence(new GameRandoms(2026).GetRandomizer(Damage));
        var shuffle = Sequence(new GameRandoms(2026).GetRandomizer(Key("shuffle")));
        var foreign = Sequence(new GameRandoms(2026).GetRandomizer(RegKey.From("other", "damage")));

        Assert.NotEqual(damage, shuffle);
        Assert.NotEqual(damage, foreign);
    }

    [Fact]
    public void GetRandomizer_Returns_The_Cached_Instance_For_An_Equal_Key()
    {
        var randoms = new GameRandoms(7);
        var first = randoms.GetRandomizer(Damage);

        Assert.Same(first, randoms.GetRandomizer(Damage));
        // RegKey 是值相等的；用另一個實例查找也該命中同一個隨機器。
        Assert.Same(first, randoms.GetRandomizer(RegKey.From("test", "damage")));
    }

    [Fact]
    public void Different_Scopes_Get_Different_Randomizers()
    {
        var randoms = new GameRandoms(7);

        Assert.NotSame(randoms.GetRandomizer(Key("a")), randoms.GetRandomizer(Key("b")));
        Assert.NotSame(randoms.GetRandomizer(Key("a")), randoms.GetRandomizer(RegKey.From("other", "a")));
    }

    [Fact]
    public void ResetRandomizer_Returns_A_New_Randomizer_At_The_Start_Of_The_Scope_Sequence()
    {
        var randoms = new GameRandoms(99);
        var first = randoms.GetRandomizer(Damage);
        _ = first.NextU64(); // 把它推離起點，若重置沒重算種子就會露餡

        var reset = randoms.ResetRandomizer(Damage);

        Assert.NotSame(first, reset);
        Assert.Same(reset, randoms.GetRandomizer(Damage));
        Assert.Equal(Sequence(new GameRandoms(99).GetRandomizer(Damage)), Sequence(reset));
    }

    [Fact]
    public void ResetRandomizer_On_A_Fresh_Scope_Matches_A_Lazily_Created_One()
    {
        var randoms = new GameRandoms(99);

        var reset = randoms.ResetRandomizer(Key("fresh"));

        Assert.Equal(Sequence(new GameRandoms(99).GetRandomizer(Key("fresh"))), Sequence(reset));
    }

    [Fact]
    public void ResetRandomizer_Leaves_Other_Scopes_Alone()
    {
        var randoms = new GameRandoms(99);
        var damage = randoms.GetRandomizer(Damage);
        var twin = new GameRandoms(99).GetRandomizer(Damage);

        randoms.ResetRandomizer(Key("shuffle"));

        Assert.Same(damage, randoms.GetRandomizer(Damage));
        Assert.Equal(Sequence(twin), Sequence(damage));
    }

    [Fact]
    public void Seed_Stays_The_Same_After_Reset()
    {
        var randoms = new GameRandoms(99);

        randoms.ResetRandomizer(Damage);

        Assert.Equal(99ul, randoms.Seed);
    }
}
