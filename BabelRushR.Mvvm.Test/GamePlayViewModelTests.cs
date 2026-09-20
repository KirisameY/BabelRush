using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class GamePlayViewModelTests
{
    [Fact]
    public void Update_Advances_The_GamePlay()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        viewModel.Update(0.5);

        Assert.Equal(0.5, gamePlay.Time, 10);
        Assert.Equal(0.5, gamePlay.DeltaTime, 10);
    }

    [Fact]
    public void Player_Entity_Appears_Exactly_Once_In_The_Scene()
    {
        var player = new TestEntity(maxHP: 10);
        var gamePlay = CommonGamePlay.Create(player);
        var viewModel = new GamePlayViewModel(gamePlay);

        var playerViewModel = viewModel.Scene.Find(gamePlay.PlayerState.PCEntity);

        Assert.NotNull(playerViewModel);
        Assert.Same(playerViewModel, Assert.Single(viewModel.Scene.Entities));
    }

    [Fact]
    public void Disposing_Stops_Reflecting_The_Scene()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        viewModel.Dispose();
        gamePlay.Scene.AddEntity(new TestEntity(maxHP: 5));

        // 只剩下开局时放进场景的玩家实体
        Assert.Single(viewModel.Scene.Entities);
    }
}
