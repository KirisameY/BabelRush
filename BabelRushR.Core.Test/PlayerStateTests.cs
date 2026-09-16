using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Test;

public class PlayerStateTests
{
    private static CommonGamePlay NewGame(int maxAP = 3, double apRegeneration = 1.0)
        => CommonGamePlay.Create(new TestEntity(maxHP: 10), maxAP, apRegeneration);

    [Fact]
    public void Exposes_PCEntity_Config_And_Empty_Piles()
    {
        var pc = new TestEntity(maxHP: 10);
        var game = CommonGamePlay.Create(pc, maxAP: 4, apRegeneration: 2.0);

        Assert.Same(pc, game.PlayerState.PCEntity);
        Assert.Equal(4, game.PlayerState.MaxAP);
        Assert.Equal(2.0, game.PlayerState.APRegeneration, 6);
        Assert.Empty(game.PlayerState.HandPile);
        Assert.Empty(game.PlayerState.DrawPile);
        Assert.Empty(game.PlayerState.DiscardPile);
    }

    [Fact]
    public void AP_Regenerates_At_Configured_Rate()
    {
        var game = NewGame(apRegeneration: 1.0);

        game.Update(0.5);
        Assert.Equal(0, game.PlayerState.AP);
        Assert.Equal(0.5, game.PlayerState.APRegenerated, 6);

        game.Update(0.5);
        Assert.Equal(1, game.PlayerState.AP);
        Assert.Equal(0.0, game.PlayerState.APRegenerated, 6);
    }

    [Fact]
    public void One_Large_Delta_Does_Not_Overshoot_MaxAP()
    {
        var game = NewGame(maxAP: 3, apRegeneration: 1.0);

        game.Update(10.0);

        Assert.Equal(3, game.PlayerState.AP);
        Assert.Equal(1.0, game.PlayerState.APRegenerated, 6);
    }

    [Fact]
    public void Regenerated_Stays_Locked_At_One_While_AP_Is_Full()
    {
        var game = NewGame(maxAP: 3, apRegeneration: 1.0);
        game.Update(10.0);

        game.Update(0.016);

        Assert.Equal(3, game.PlayerState.AP);
        Assert.Equal(1.0, game.PlayerState.APRegenerated, 6);
    }

    [Fact]
    public void Spending_One_AP_At_Full_Is_Refunded_On_The_Next_Frame()
    {
        var game = NewGame(maxAP: 3, apRegeneration: 1.0);
        game.Update(10.0);
        Assert.Equal(3, game.PlayerState.AP);

        game.PlayerState.AP -= 1;
        Assert.Equal(2, game.PlayerState.AP);

        game.Update(1.0 / 60);

        Assert.Equal(3, game.PlayerState.AP);
        Assert.Equal(1.0, game.PlayerState.APRegenerated, 6);
    }

    [Fact]
    public void AP_Setter_Clamps_To_Range()
    {
        var game = NewGame(maxAP: 3);

        game.PlayerState.AP = 99;
        Assert.Equal(3, game.PlayerState.AP);

        game.PlayerState.AP = -5;
        Assert.Equal(0, game.PlayerState.AP);
    }

    [Fact]
    public void APChangedEvent_Is_Published_Only_When_AP_Actually_Changes()
    {
        var game = NewGame(maxAP: 3);
        var events = new List<APChangedEvent>();
        game.EventBus.Subscribe<APChangedEvent>(events.Add);

        game.Update(2.0);
        Assert.Equal(2, events.Count);
        Assert.Equal((0, 1), (events[0].OldAP, events[0].NewAP));
        Assert.Equal((1, 2), (events[1].OldAP, events[1].NewAP));

        game.PlayerState.AP = 2;
        Assert.Equal(2, events.Count);

        game.PlayerState.AP = 0;
        Assert.Equal(3, events.Count);
        Assert.Equal((2, 0), (events[2].OldAP, events[2].NewAP));
    }

    [Fact]
    public void APChangedEvent_Reaches_PlayerStateEvent_Subscribers()
    {
        var game = NewGame(maxAP: 3);
        var events = new List<PlayerStateEvent>();
        game.EventBus.Subscribe<PlayerStateEvent>(events.Add);

        game.PlayerState.AP = 1;

        Assert.IsType<APChangedEvent>(Assert.Single(events));
    }

    [Fact]
    public void APRegenerated_Notifies_PropertyChanged_Without_Publishing_An_Event()
    {
        var game = NewGame(maxAP: 3, apRegeneration: 1.0);
        var changed = new List<string?>();
        game.PlayerState.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        var events = new List<APChangedEvent>();
        game.EventBus.Subscribe<APChangedEvent>(events.Add);

        // 只累积了 0.5 点，整数费用没有变化。
        game.Update(0.5);

        Assert.Contains(nameof(IPlayerState.APRegenerated), changed);
        Assert.Empty(events);
    }

    [Fact]
    public void No_Repeated_Notification_While_Regenerated_Is_Locked()
    {
        var game = NewGame(maxAP: 3, apRegeneration: 1.0);
        game.Update(10.0);

        var changed = new List<string?>();
        game.PlayerState.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        game.Update(0.016);

        Assert.DoesNotContain(nameof(IPlayerState.APRegenerated), changed);
    }
}
