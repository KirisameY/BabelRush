using System;
using System.Collections.Generic;
using System.IO;

using BabelRushR.Core.Battle;
using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Card.Types;
using BabelRushR.Core.Contents;
using BabelRushR.Core.GamePlay;
using BabelRushR.Game.Bootstrap;
using BabelRushR.Game.GUI;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.Registration.Data;

using Zio;
using Zio.FileSystems;

namespace BabelRushR.Game.Test;

/// <summary>
///     开发期的启动场景：就地开一局游戏、包成 VM 交给 <see cref="GamePlayUI"/>，并负责它的释放。
/// </summary>
/// <remarks>
///     启动时先加载 Contents 下的资源包，让卡牌等数据进入注册表；右上角的控件只在本测试场景里直接操作 Core：
///     按输入的 id 加一张牌、清空手牌。这样可以手动验证空手牌、按 id 加入、清空和扇面重排，而不必预置固定牌。
/// </remarks>
public partial class TestGamePlay : Node
{
    private GamePlayViewModel? _viewModel;
    private CommonGamePlay? _gamePlay;

    private LineEdit CardIdInput => field ??= GetNode<LineEdit>("TestControls/Buttons/CardIdInput");
    private Button AddCardButton => field ??= GetNode<Button>("TestControls/Buttons/AddCardButton");
    private Button ClearHandButton => field ??= GetNode<Button>("TestControls/Buttons/ClearHandButton");

    public override void _Ready()
    {
        LoadContents();

        _gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 30));

        _viewModel                                  = new GamePlayViewModel(_gamePlay);
        GetNode<GamePlayUI>("GamePlayUI").ViewModel = _viewModel;

        AddCardButton.Pressed   += AddCard;
        ClearHandButton.Pressed += ClearHand;

        // 走在 VM 建好之后：这条走的是集合绑定的增量 Add 分支，而不是绑定时的那次全量同步。
        var enemy = new TestEntity(maxHP: 12) { Position = 200 };
        _gamePlay.Scene.AddEntity(enemy);
        _gamePlay.Battle.TryJoinBattle(enemy, BattleSide.Enemy);
    }

    public override void _ExitTree()
    {
        _viewModel?.Dispose();
        _gamePlay?.Dispose();
    }

    private void AddCard()
    {
        if (_gamePlay is not { } gamePlay) return;

        var text = CardIdInput.Text.Trim();
        if (text.Length == 0) return;

        // 既能输入 "babelrush:strike"，也能只输入 "strike"：裸 key 按本测试包的命名空间补齐。
        var id = RegKey.FromFull(text) ?? RegKey.From("babelrush", text);
        if (!CardType.Registry.TryGetValue(id, out var cardType))
        {
            GD.PushWarning($"Card id not found: {id}");
            cardType = PlaceHolderCardType.Instance;
        }

        gamePlay.PlayerState.CardDeck.AddCard(cardType.CreateCard(), CardPile.Hand);
    }

    private void ClearHand()
    {
        var c = _gamePlay?.PlayerState.CardDeck.HandPile.Count ?? 0;
        for (int i = c - 1; i >= 0; i--)
        {
            _gamePlay?.PlayerState.CardDeck.RemoveCard(CardPile.Hand, i, out _);
        }
    }

    /// <summary>
    ///     加载 <see cref="ContentsInfo.RelativePackPath"/> 下的资源包；必须在创建 GamePlay 之前完成，
    ///     因为卡牌注册表是在数据加载器的释放阶段冻结的。
    /// </summary>
    private static void LoadContents()
    {
        using var fs = new PhysicalFileSystem();
        var contentsPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), ContentsInfo.RelativePackPath)
        );
        var contentsDir = new DirectoryEntry(fs, fs.ConvertPathFromInternal(contentsPath));
        ContentsLoaderManager.LoadPacks(contentsDir, new Progress<IReadOnlyList<string>>());
    }
}