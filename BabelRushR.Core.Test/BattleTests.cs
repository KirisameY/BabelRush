using BabelRushR.Core.Battle;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Test;

public class BattleTests
{
    private static CommonGamePlay NewGamePlay() => CommonGamePlay.Create(new TestEntity(maxHP: 10));

    /// <summary>加入一个已在场景里的实体，供参战用。</summary>
    private static TestEntity AddEntity(CommonGamePlay gamePlay, int maxHP = 5)
    {
        var entity = new TestEntity(maxHP);
        gamePlay.Scene.AddEntity(entity);
        return entity;
    }

    /// <summary>只收战斗事件，便于断言事件的发生与顺序。</summary>
    private static List<BattleEvent> RecordBattleEvents(CommonGamePlay gamePlay)
    {
        var events = new List<BattleEvent>();
        gamePlay.EventBus.Subscribe<BattleEvent>(events.Add);
        return events;
    }


    #region State & Camps

    [Fact]
    public void A_New_GamePlay_Is_Not_In_Battle_And_Has_Empty_Camps()
    {
        var battle = NewGamePlay().Battle;

        Assert.False(battle.IsInBattle);
        Assert.Empty(battle.Allies);
        Assert.Empty(battle.Enemies);
    }

    [Fact]
    public void Battle_Is_One_Cached_Instance_Per_GamePlay()
    {
        var gamePlay = NewGamePlay();

        Assert.Same(gamePlay.Battle, gamePlay.Battle);
        Assert.NotSame(gamePlay.Battle, NewGamePlay().Battle);
    }

    [Fact]
    public void Joining_An_Ally_Does_Not_Start_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var events = RecordBattleEvents(gamePlay);
        var ally = AddEntity(gamePlay);

        Assert.True(battle.TryJoinBattle(ally, BattleSide.Ally));

        Assert.False(battle.IsInBattle);
        Assert.Equal(new IEntity[] { ally }, battle.Allies);
        Assert.IsType<BattleUnitJoinedEvent>(Assert.Single(events));
    }

    [Fact]
    public void The_First_Enemy_Starts_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var events = RecordBattleEvents(gamePlay);
        var enemy = AddEntity(gamePlay);

        Assert.True(battle.TryJoinBattle(enemy, BattleSide.Enemy));

        Assert.True(battle.IsInBattle);
        Assert.Equal(new IEntity[] { enemy }, battle.Enemies);

        // 先落定状态，再按「成员变化 → 战斗开始」报事件。
        Assert.Collection(events,
            e => Assert.IsType<BattleUnitJoinedEvent>(e),
            e => Assert.IsType<BattleStartedEvent>(e));
    }

    [Fact]
    public void Another_Enemy_Joins_Without_Restarting_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        battle.TryJoinBattle(AddEntity(gamePlay), BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        Assert.True(battle.TryJoinBattle(AddEntity(gamePlay), BattleSide.Enemy));

        Assert.True(battle.IsInBattle);
        Assert.IsType<BattleUnitJoinedEvent>(Assert.Single(events));
    }

    [Fact]
    public void The_Last_Enemy_Leaving_Ends_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        Assert.True(battle.TryLeaveBattle(enemy));

        Assert.False(battle.IsInBattle);
        Assert.Empty(battle.Enemies);
        Assert.Collection(events,
            e => Assert.IsType<BattleUnitLeftEvent>(e),
            e => Assert.IsType<BattleEndedEvent>(e));
    }

    [Fact]
    public void An_Ally_Leaving_Does_Not_End_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        Assert.True(battle.TryLeaveBattle(ally));

        Assert.True(battle.IsInBattle);
        Assert.Empty(battle.Allies);
        Assert.Equal(new IEntity[] { enemy }, battle.Enemies);
        Assert.IsType<BattleUnitLeftEvent>(Assert.Single(events));
    }

    [Fact]
    public void Camps_Keep_Insertion_Order()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var allyA = AddEntity(gamePlay);
        var allyB = AddEntity(gamePlay);
        var enemyA = AddEntity(gamePlay);
        var enemyB = AddEntity(gamePlay);

        battle.TryJoinBattle(allyA, BattleSide.Ally);
        battle.TryJoinBattle(allyB, BattleSide.Ally);
        battle.TryJoinBattle(enemyA, BattleSide.Enemy);
        battle.TryJoinBattle(enemyB, BattleSide.Enemy);

        Assert.Equal(new IEntity[] { allyA, allyB }, battle.Allies);
        Assert.Equal(new IEntity[] { enemyA, enemyB }, battle.Enemies);
    }

    #endregion


    #region Notify & Ordering

    [Fact]
    public void IsInBattle_Raises_PropertyChanged_When_It_Flips()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var changed = new List<string?>();
        battle.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        var first = AddEntity(gamePlay);
        var second = AddEntity(gamePlay);
        battle.TryJoinBattle(first, BattleSide.Enemy);
        Assert.Single(changed);
        Assert.Equal(nameof(IBattle.IsInBattle), changed[0]);

        battle.TryJoinBattle(second, BattleSide.Enemy); // 敌方 1 → 2，状态没有翻转
        Assert.Single(changed);

        battle.TryLeaveBattle(second); // 2 → 1，仍没有翻转
        Assert.Single(changed);

        battle.TryLeaveBattle(first);
        Assert.Equal(2, changed.Count);
        Assert.Equal(nameof(IBattle.IsInBattle), changed[1]);
    }

    [Fact]
    public void Camp_Contents_Are_Settled_Before_The_Joined_Event()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var enemy = AddEntity(gamePlay);
        var inCampWhenNotified = false;
        var inBattleWhenNotified = false;
        gamePlay.EventBus.Subscribe<BattleUnitJoinedEvent>(e =>
        {
            inCampWhenNotified    = battle.Enemies.Contains(e.Unit);
            inBattleWhenNotified = battle.IsInBattle;
        });

        battle.TryJoinBattle(enemy, BattleSide.Enemy);

        Assert.True(inCampWhenNotified);
        Assert.True(inBattleWhenNotified);
    }

    [Fact]
    public void IsInBattle_Is_Already_Updated_When_A_Membership_Event_Arrives()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var enemy = AddEntity(gamePlay);
        var joinedSeen = new List<bool>();
        var leftSeen = new List<bool>();
        gamePlay.EventBus.Subscribe<BattleUnitJoinedEvent>(_ => joinedSeen.Add(battle.IsInBattle));
        gamePlay.EventBus.Subscribe<BattleUnitLeftEvent>(_ => leftSeen.Add(battle.IsInBattle));

        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        battle.TryLeaveBattle(enemy);

        Assert.True(Assert.Single(joinedSeen));
        Assert.False(Assert.Single(leftSeen));
    }

    [Fact]
    public void Camps_Can_Change_Outside_A_Battle_Without_State_Events()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var events = RecordBattleEvents(gamePlay);
        var ally = AddEntity(gamePlay);

        Assert.True(battle.TryJoinBattle(ally, BattleSide.Ally));
        Assert.True(battle.TryLeaveBattle(ally));

        Assert.False(battle.IsInBattle);
        Assert.Collection(events,
            e => Assert.IsType<BattleUnitJoinedEvent>(e),
            e => Assert.IsType<BattleUnitLeftEvent>(e));
    }

    #endregion


    #region Rejection

    [Fact]
    public void Joining_The_Same_Unit_Twice_Returns_False_Without_Publishing()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        Assert.False(battle.TryJoinBattle(enemy, BattleSide.Enemy));

        Assert.Single(battle.Enemies);
        Assert.Empty(events);
    }

    [Fact]
    public void Joining_A_Unit_That_Is_Already_On_The_Other_Side_Returns_False()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var unit = AddEntity(gamePlay);
        battle.TryJoinBattle(unit, BattleSide.Ally);
        var events = RecordBattleEvents(gamePlay);

        Assert.False(battle.TryJoinBattle(unit, BattleSide.Enemy));

        Assert.Equal(new IEntity[] { unit }, battle.Allies);
        Assert.Empty(battle.Enemies);
        Assert.False(battle.IsInBattle);
        Assert.Empty(events);
    }

    [Fact]
    public void Joining_A_Unit_That_Is_Not_In_The_Scene_Returns_False()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var events = RecordBattleEvents(gamePlay);
        var outsider = new TestEntity(maxHP: 5);

        Assert.False(battle.TryJoinBattle(outsider, BattleSide.Enemy));

        Assert.False(battle.IsInBattle);
        Assert.Equal(BattleSide.None, battle.GetSideOf(outsider));
        Assert.Empty(events);
    }

    [Fact]
    public void Joining_With_No_Side_Returns_False()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var unit = AddEntity(gamePlay);
        var events = RecordBattleEvents(gamePlay);

        Assert.False(battle.TryJoinBattle(unit, BattleSide.None));

        Assert.Equal(BattleSide.None, battle.GetSideOf(unit));
        Assert.Empty(battle.Allies);
        Assert.Empty(battle.Enemies);
        Assert.Empty(events);
    }

    [Fact]
    public void Leaving_A_Unit_That_Is_Not_In_The_Battle_Returns_False()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var outsider = AddEntity(gamePlay);
        var events = RecordBattleEvents(gamePlay);

        Assert.False(battle.TryLeaveBattle(outsider));

        Assert.Empty(events);
    }

    #endregion


    #region Leaving The Scene

    [Fact]
    public void A_Unit_Removed_From_The_Scene_Leaves_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        var events = RecordBattleEvents(gamePlay);

        Assert.True(gamePlay.Scene.RemoveEntity(ally));

        Assert.Empty(battle.Allies);
        Assert.Equal(BattleSide.None, battle.GetSideOf(ally));
        Assert.IsType<BattleUnitLeftEvent>(Assert.Single(events));
    }

    [Fact]
    public void Leaving_The_Battle_Keeps_The_Unit_In_The_Scene()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);

        Assert.True(battle.TryLeaveBattle(ally));

        Assert.Contains(ally, gamePlay.Scene.Entities);
    }

    [Fact]
    public void A_Dying_Enemy_Leaves_The_Battle_And_Ends_It_When_Last()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        enemy.HP = 0;
        gamePlay.Update(0.016);

        Assert.DoesNotContain(enemy, gamePlay.Scene.Entities);
        Assert.False(battle.IsInBattle);
        Assert.Empty(battle.Enemies);
        Assert.Collection(events,
            e => Assert.IsType<BattleUnitLeftEvent>(e),
            e => Assert.IsType<BattleEndedEvent>(e));
    }

    [Fact]
    public void A_Dying_Ally_Leaves_The_Battle_Without_Ending_It()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        ally.HP = 0;
        gamePlay.Update(0.016);

        Assert.True(battle.IsInBattle);
        Assert.Empty(battle.Allies);
        Assert.IsType<BattleUnitLeftEvent>(Assert.Single(events));
    }

    #endregion


    #region Scene Replacement

    [Fact]
    public void Replacing_The_Scene_Evacuates_Every_Unit_And_Ends_The_Battle()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        var events = RecordBattleEvents(gamePlay);

        gamePlay.Scene = new CommonScene();

        Assert.False(battle.IsInBattle);
        Assert.Empty(battle.Allies);
        Assert.Empty(battle.Enemies);
        Assert.Equal(BattleSide.None, battle.GetSideOf(ally));
        Assert.Equal(BattleSide.None, battle.GetSideOf(enemy));

        // 先我方后敌方；最后一个敌人离开时补上战斗结束。
        Assert.Collection(events,
            e => Assert.Equal(BattleSide.Ally, Assert.IsType<BattleUnitLeftEvent>(e).Side),
            e => Assert.Equal(BattleSide.Enemy, Assert.IsType<BattleUnitLeftEvent>(e).Side),
            e => Assert.IsType<BattleEndedEvent>(e));
    }

    [Fact]
    public void Replacing_The_Scene_With_No_Enemies_Publishes_No_Ended_Event()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        var events = RecordBattleEvents(gamePlay);

        gamePlay.Scene = new CommonScene();

        Assert.Empty(battle.Allies);
        Assert.False(battle.IsInBattle);
        Assert.IsType<BattleUnitLeftEvent>(Assert.Single(events));
    }

    #endregion


    #region Queries

    [Fact]
    public void GetSideOf_Returns_The_Side_And_None_For_Non_Participants()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        var outsider = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);

        Assert.Equal(BattleSide.Ally, battle.GetSideOf(ally));
        Assert.Equal(BattleSide.Enemy, battle.GetSideOf(enemy));
        Assert.Equal(BattleSide.None, battle.GetSideOf(outsider));
    }

    [Fact]
    public void GetSideEntities_Returns_Each_Sides_Units()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);

        Assert.Equal(new IEntity[] { ally }, battle.GetSideEntities(BattleSide.Ally));
        Assert.Equal(new IEntity[] { enemy }, battle.GetSideEntities(BattleSide.Enemy));
        Assert.Null(battle.GetSideEntities(BattleSide.None));
    }

    [Fact]
    public void Opponent_Side_Leads_To_The_Other_Camp()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var ally = AddEntity(gamePlay);
        var enemy = AddEntity(gamePlay);
        battle.TryJoinBattle(ally, BattleSide.Ally);
        battle.TryJoinBattle(enemy, BattleSide.Enemy);

        // 目标选择预期的用法：先定位自己所在阵营，再取其对阵方的列表。
        Assert.Equal(new IEntity[] { enemy },
            battle.GetSideEntities(battle.GetSideOf(ally).Opponent));
        Assert.Equal(new IEntity[] { ally },
            battle.GetSideEntities(battle.GetSideOf(enemy).Opponent));
    }

    [Fact]
    public void Battle_Events_Reach_GamePlayEvent_Subscribers_Through_Base_Dispatch()
    {
        var gamePlay = NewGamePlay();
        var battle = gamePlay.Battle;
        var events = new List<GamePlayEvent>();
        gamePlay.EventBus.Subscribe<GamePlayEvent>(events.Add);
        var enemy = AddEntity(gamePlay);

        battle.TryJoinBattle(enemy, BattleSide.Enemy);
        battle.TryLeaveBattle(enemy);

        Assert.Contains(events, e => e is BattleUnitJoinedEvent);
        Assert.Contains(events, e => e is BattleStartedEvent);
        Assert.Contains(events, e => e is BattleUnitLeftEvent);
        Assert.Contains(events, e => e is BattleEndedEvent);
    }

    #endregion
}
