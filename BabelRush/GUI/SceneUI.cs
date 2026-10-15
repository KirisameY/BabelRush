using System.Collections.Generic;

using BabelRushR.Game.Binding;
using BabelRushR.Game.GUI.Entities;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;
using KirisameY.GenericUtils;

namespace BabelRushR.Game.GUI;

/// <summary>
///     场景的视图节点：以 <see cref="ICollection{T}"/> 的身份承载场景里的实体节点，
///     内容由 <see cref="SceneViewModel.Entities"/> 经数据绑定驱动。
/// </summary>
public partial class SceneUI : Node2D, ISimpleCollectionObserver<EntityViewModel>
{
    private readonly Dictionary<EntityViewModel, EntityUI> _views = [];
    private readonly List<IBindHandle> _binds = [];

    public SceneViewModel? ViewModel
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

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();


    private void Rebind()
    {
        Unbind();
        if (ViewModel is not { } viewModel) return;

        // 集合绑定在构造时就会做一次全量同步（先 Clear 再逐个 Add），上一批实体节点在这里被清掉。
        _binds.Add(
            DataBinding.Binder.BindCollection(
                viewModel, vm => vm.Entities,
                this, ui => ui, TypeA.Of<EntityViewModel>()
            )
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }

    #region Collection Observe

    public void Add(EntityViewModel item)
    {
        if (_views.ContainsKey(item)) return;

        var view = EntityUI.New();
        view.ViewModel = item;
        AddChild(view);
        _views.Add(item, view);
    }

    public void Remove(EntityViewModel item)
    {
        if (!_views.Remove(item, out var view)) return;
        Detach(view);
    }

    public void Clear()
    {
        foreach (var view in _views.Values) Detach(view);
        _views.Clear();
    }


    /// <summary>
    ///     把实体节点从本节点上摘下并释放。解绑由节点自己的 <c>_ExitTree</c> 完成，这里不代劳。
    /// </summary>
    private void Detach(EntityUI view)
    {
        RemoveChild(view);
        view.QueueFree();
    }

    #endregion
}