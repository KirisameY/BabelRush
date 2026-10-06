using System.Collections.Generic;

using BabelRushR.Game.Binding;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;

namespace BabelRushR.Game.GUI;

public partial class GamePlayUI : Node
{
    private SceneUI SceneUI => field ??= GetNode<SceneUI>("Scene");

    private Label TimeLabel => field ??= GetNode<Label>("HUD/TimeLabel");

    private Label DeltaTimeLabel => field ??= GetNode<Label>("HUD/DeltaTimeLabel");

    private readonly List<IBindHandle> _binds = [];

    public GamePlayViewModel? ViewModel
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (IsInsideTree()) Rebind(); else Unbind();
        }
    }

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    public override void _Process(double delta)
    {
        ViewModel?.Update(delta);
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
            )
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }
}