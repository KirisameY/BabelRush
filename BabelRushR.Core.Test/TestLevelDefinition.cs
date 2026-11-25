using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

/// <summary>
///     把現成的 <see cref="ILevel"/> 包成 <see cref="ILevelDefinition"/>。
///     用於只關心「最終拿到哪個關卡」的測試；需要斷言定義收到的 GamePlay 時用委派建構子。
/// </summary>
internal sealed class TestLevelDefinition(RegKey id, Func<IGamePlay, ILevel> generate) : ILevelDefinition
{
    public TestLevelDefinition(ILevel level) : this(RegKey.From("test", "level"), _ => level) { }

    public RegKey ID => id;

    public ILevel GenerateLevel(IGamePlay gamePlay) => generate(gamePlay);
}
