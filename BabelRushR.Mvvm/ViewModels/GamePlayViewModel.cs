using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     一局游戏的根 VM。视图只需要认识这一个对象。
/// </summary>
public sealed class GamePlayViewModel : ViewModelBase
{
    private readonly IGamePlay _gamePlay;

    public SceneViewModel Scene { get; }

    public GamePlayViewModel(IGamePlay gamePlay)
    {
        _gamePlay = gamePlay;

        Scene = new SceneViewModel(gamePlay.Scene);
        Track(Scene);
    }

    /// <summary>
    ///     由视图每帧调用，推进逻辑层。
    /// </summary>
    public void Update(double delta) => _gamePlay.Update(delta);
}
