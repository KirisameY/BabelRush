using System.Collections.Generic;

using BabelRushR.Game.Binding;
using BabelRushR.Game.GUI.Hand;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;

namespace BabelRushR.Game.GUI;

public partial class GamePlayUI : Node
{
    #region Scene & Lifetime

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    public override void _Process(double delta) => ViewModel?.Update(delta);

    #endregion


    #region Nodes & Properties

    private SceneUI SceneUI => field ??= GetNode<SceneUI>("Scene");

    private HandCardsUI HandCardsUI => field ??= GetNode<HandCardsUI>("HUD/HandCardsUI");

    private Label TimeLabel => field ??= GetNode<Label>("HUD/TimeLabel");

    private Label DeltaTimeLabel => field ??= GetNode<Label>("HUD/DeltaTimeLabel");

    #endregion


    #region Binding

    private readonly List<IBindHandle> _binds = [];

    public GamePlayViewModel? ViewModel
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (IsInsideTree()) Rebind();
            else Unbind();
        }
    }

    private void Rebind()
    {
        Unbind();
        if (ViewModel is not { } viewModel) return;

        _binds.AddRange(
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.Time, this, ui => ui.TimeLabel.Text, t => t.ToString("F2")
            ),
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.DeltaTime, this, ui => ui.DeltaTimeLabel.Text, t => t.ToString("F3")
            ),
            // Scene 被换掉时由这条自己推过去；SceneUI 会在它自己的 ViewModel setter 里重接集合绑定。
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.Scene, this, ui => ui.SceneUI.ViewModel!
            ),
            // 玩家状态整局不变，但手牌集合会持续增删；HandCardsUI 自己再对 Cards 做集合绑定。
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.CardDeck.HandPile, this, ui => ui.HandCardsUI.ViewModel!
            )
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }

    #endregion
}