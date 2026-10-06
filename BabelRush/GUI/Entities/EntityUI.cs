using System;
using System.Collections.Generic;

using BabelRushR.Game.Binding;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;

namespace BabelRushR.Game.GUI.Entities;

public partial class EntityUI : Node2D
{
    private ProgressBar HPBar => field ??= GetNode<ProgressBar>("ProgressBar");

    private readonly List<IBindHandle> _binds = [];

    public EntityViewModel? ViewModel
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

    public double PositionX
    {
        get => Position.X;
        set => Position = Position with { X = (float)value };
    }

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    private void Rebind()
    {
        Unbind();
        if (ViewModel is not { } viewModel) return;

        // MaxHP 必须先于 HP：Range 会把 Value 钳进 [MinValue, MaxValue]，而改动 MaxValue 不会重新推一次 Value，
        // 反过来的话血量超过初始上限时进度条会停错位置。
        _binds.AddRange(
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.MaxHP, this, ui => ui.HPBar.MaxValue
            ),
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.HP, this, ui => ui.HPBar.Value
            ),
            DataBinding.Binder.BindPropertyOneWay(
                viewModel, vm => vm.Position, this, ui => ui.PositionX
            )
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }
}