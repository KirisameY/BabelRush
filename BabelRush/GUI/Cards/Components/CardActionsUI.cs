using System.Collections.Generic;

using BabelRushR.Game.Binding;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;
using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Game.GUI.Cards.Components;

public partial class CardActionsUI : Control, ISimpleListObserver<EntityActionViewModel>
{
    #region Lifetime

    private CardActionsUI() { }

    #endregion


    #region Binding

    private readonly List<IBindHandle> _binds = [];

    public IReadOnlyNotifiableList<EntityActionViewModel>? ViewModel
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
            DataBinding.Binder.BindCollection(ViewModel, this, TypeA.Of<EntityActionViewModel>())
        );
    }

    private void Unbind()
    {
        _binds.ForEach(b => b.Dispose());
        _binds.Clear();
    }

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    #endregion


    #region List

    private readonly List<CardActionUI> _list = [];

    public int Find(EntityActionViewModel item) => _list.FindIndex(n => n.ViewModel == item);

    public void Add(EntityActionViewModel item)
    {
        var node = CardActionUI.New();
        node.ViewModel = item;
        _list.Add(node);
        AddChild(node);
    }

    public void Insert(int index, EntityActionViewModel item)
    {
        var node = CardActionUI.New();
        node.ViewModel = item;
        _list.Insert(index, node);
        AddChild(node);
        MoveChild(node, index);
    }

    public void RemoveAt(int index)
    {
        var node = _list[index];
        _list.RemoveAt(index);
        node.QueueFree();
    }

    public void Replace(int index, EntityActionViewModel newItem)
    {
        var old = _list[index];
        var node = CardActionUI.New();
        node.ViewModel = newItem;
        _list[index]   = node;
        Detach(old);
        AddChild(node);
    }

    public void Clear()
    {
        foreach (var node in _list)
        {
            Detach(node);
        }
        _list.Clear();
    }

    private void Detach(CardActionUI view)
    {
        RemoveChild(view);
        view.QueueFree();
    }

    #endregion
}