using System;
using System.Linq;

using BabelRushR.Core.Card.Types;
using BabelRushR.Core.EntityAction.Types;
using BabelRushR.Core.GamePlay;
using BabelRushR.Game.GUI;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.Registration.Data;

namespace BabelRushR.Game.Test;

/// <summary>
///     开发期的启动场景：就地开一局游戏、包成 VM 交给 <see cref="GamePlayUI"/>，并负责它的释放。
/// </summary>
/// <remarks>
///     右上角的两个按钮只在本测试场景里直接操作 Core：一个随机加牌，一个清空手牌。
///     这样可以手动验证空手牌、增量加入、清空和扇面重排，而不必预置固定牌。
/// </remarks>
public partial class TestGamePlay : Node
{
    private GamePlayViewModel? _viewModel;
    private CommonGamePlay? _gamePlay;

    private readonly Random _random = new();
    private int _nextCardId;

    private Button AddCardButton => field ??= GetNode<Button>("TestControls/Buttons/AddCardButton");
    private Button ClearHandButton => field ??= GetNode<Button>("TestControls/Buttons/ClearHandButton");

    public override void _Ready()
    {
        _gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 30));

        _viewModel = new GamePlayViewModel(_gamePlay);
        GetNode<GamePlayUI>("GamePlayUI").ViewModel = _viewModel;

        AddCardButton.Pressed += AddRandomCard;
        ClearHandButton.Pressed += ClearHand;

        // 走在 VM 建好之后：这条走的是集合绑定的增量 Add 分支，而不是绑定时的那次全量同步。
        _gamePlay.Scene.AddEntity(new TestEntity(maxHP: 12) { Position = 200 });
    }

    public override void _ExitTree() => _viewModel?.Dispose();

    private void AddRandomCard()
    {
        if (_gamePlay is not { } gamePlay) return;

        var cost = _random.Next(0, 10);
        var actionCount = _random.Next(0, 4);
        var cardType = new RecordCardType(
            RegKey.From("babelrush.test", $"hand_{_nextCardId++}"),
            cost,
            [.. Enumerable.Range(0, actionCount).Select(i => (TestActionType, i + 1))]
        );

        gamePlay.PlayerState.CardDeck.HandPile.Add(cardType.CreateCard());
    }

    private void ClearHand() => _gamePlay?.PlayerState.CardDeck.HandPile.Clear();

    /// <summary>Action 的占位类型：目标选择与打牌都还没做，这里只关心数量。</summary>
    private static readonly IEntityActionType TestActionType =
        new ValuedDelegateEntityActionType((_, _) => { });
}
