using BabelRushR.Core.GamePlay;
using BabelRushR.Game.GUI;
using BabelRushR.Mvvm.ViewModels;

using Godot;

namespace BabelRushR.Game.Test;

/// <summary>
///     开发期的启动场景：就地开一局游戏、包成 VM 交给 <see cref="GamePlayUI"/>，并负责它的释放。
/// </summary>
public partial class TestGamePlay : Node
{
    private GamePlayViewModel? _viewModel;

    public override void _Ready()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 30));

        _viewModel = new GamePlayViewModel(gamePlay);
        GetNode<GamePlayUI>("GamePlayUI").ViewModel = _viewModel;

        // 走在 VM 建好之后：这条走的是集合绑定的增量 Add 分支，而不是绑定时的那次全量同步。
        gamePlay.Scene.AddEntity(new TestEntity(maxHP: 12) { Position = 200 });
    }

    public override void _ExitTree() => _viewModel?.Dispose();
}
