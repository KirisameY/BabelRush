using System.Reflection;

using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Level;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Test;

internal static class TestGamePlay
{
    private static readonly ConstructorInfo Constructor =
        typeof(CommonGamePlay).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                              .SingleOrDefault(ctor => ctor.GetParameters().Length == 0)
        ?? throw new InvalidOperationException($"CommonGamePlay 的無參建構子不見了，請更新 {nameof(TestGamePlay)}。");

    private static readonly PropertyInfo RandomsProperty = Property(nameof(CommonGamePlay.Randoms));
    private static readonly PropertyInfo SceneProperty = Property(nameof(CommonGamePlay.Scene));
    private static readonly PropertyInfo PlayerStateProperty = Property(nameof(CommonGamePlay.PlayerState));

    /// <summary>
    ///     手動裝配一局：與 <see cref="CommonGamePlay.Create"/> 不同，這裡不放入玩家實體、也不註冊自動發現的事件處理器。
    /// </summary>
    /// <remarks>
    ///     <see cref="CommonGamePlay"/> 的建構子是 private、屬性又是 init-only，正常途徑只能走
    ///     <see cref="CommonGamePlay.Create"/>。測試需要「不塞玩家實體」的最小裝配，因此用反射繞過建構子並寫入
    ///     required 屬性——不要為了測試把生產程式碼的可見性放寬。
    /// </remarks>
    public static CommonGamePlay Assemble(ulong? seed = null, IScene? scene = null, IPlayerState? playerState = null, ILevel? level = null, GameRandoms? randoms = null)
    {
        var gamePlay = (CommonGamePlay)Constructor.Invoke(null);

        RandomsProperty.SetValue(gamePlay, randoms ?? new GameRandoms(seed ?? (ulong)Random.Shared.NextInt64()));
        SceneProperty.SetValue(gamePlay, scene ?? new CommonScene());
        PlayerStateProperty.SetValue(gamePlay, playerState ?? new CommonPlayerState(new TestEntity(maxHP: 10)));

        gamePlay.SetLevel(level is null ? PlaceHolderLevelDefinition.Instance : new TestLevelDefinition(level));
        return gamePlay;
    }

    private static PropertyInfo Property(string name) =>
        typeof(CommonGamePlay).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
        ?? throw new InvalidOperationException($"CommonGamePlay.{name} 不再是公開的實例屬性，請更新 {nameof(TestGamePlay)}。");
}
