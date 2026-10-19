using System.Collections.Generic;
using System.Linq;

using BabelRushR.Game.Binding;
using BabelRushR.Game.GUI.Cards;
using BabelRushR.Mvvm.ViewModels;

using Godot;

using KirisameY.BindingBridge;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;
using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Game.GUI.Hand;

/// <summary>
///     手牌区的视图节点：维护 <see cref="CardPileViewModel.Cards"/> 与 <see cref="CardUI"/> 节点的集合映射。
/// </summary>
/// <remarks>
///     布局、Tween 与鼠标交互在 <c>HandCardsUI.Presentation.cs</c> 分部里；本文件只保留场景生命周期、
///     数据绑定和集合增删这些与其它 UI 节点一致的部分。
/// </remarks>
public partial class HandCardsUI : Control, ISimpleListObserver<CardViewModel>
{
    #region Scene & Lifetime

    private HandCardsUI() { }

    public override void _Ready() => InitializeLayout();

    public override void _EnterTree() => Rebind();

    public override void _ExitTree() => Unbind();

    public override void _Process(double delta) => DraggingProcess();

    #endregion


    #region Binding

    private readonly List<IBindHandle> _binds = [];

    public IReadOnlyNotifiableList<CardViewModel>? ViewModel
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

        _binds.Add(DataBinding.Binder.BindCollection(viewModel, this, TypeA.Of<CardViewModel>()));
    }

    private void Unbind()
    {
        _binds.ForEach(handle => handle.Dispose());
        _binds.Clear();
        Clear();
    }

    #endregion


    #region Cards

    private readonly List<CardUI> _cards = [];

    public int Find(CardViewModel item) => _cards.FindIndex(card => card.ViewModel == item);

    public void Add(CardViewModel item)
    {
        var wasLaidOut = _layoutValid;
        var card = CreateCard(item);
        _cards.Add(card);
        ApplyChildOrder();
        var laidOut = Relayout(card);
        if (wasLaidOut && laidOut) StartEntryAnimation(card);
    }

    public void Insert(int index, CardViewModel item)
    {
        var wasLaidOut = _layoutValid;
        var card = CreateCard(item);
        _cards.Insert(index, card);
        ApplyChildOrder();
        var laidOut = Relayout(card);
        if (wasLaidOut && laidOut) StartEntryAnimation(card);
    }

    public void RemoveAt(int index)
    {
        var card = _cards[index];
        _cards.RemoveAt(index);

        DetachCard(card);
        ApplyChildOrder();
        Relayout();
    }

    public void Replace(int index, CardViewModel newItem)
    {
        var card = _cards[index];
        card.ViewModel = newItem;

        var wasDragging = PrepareReplacement(card);
        Relayout();
        if (wasDragging) FinishReplacement(card);
    }

    public void Move(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex) return;

        var card = _cards[oldIndex];
        _cards.RemoveAt(oldIndex);
        _cards.Insert(newIndex, card);
        ApplyChildOrder();
        Relayout();
    }

    public void Clear()
    {
        ResetInteraction();

        var snapshot = _cards.ToArray();
        _cards.Clear();
        foreach (var card in snapshot) DetachCard(card);
    }

    private CardUI CreateCard(CardViewModel viewModel)
    {
        var card = CardUI.New();
        card.ViewModel = viewModel;
        AddChild(card);

        AttachCard(card);
        return card;
    }

    private void DetachCard(CardUI card)
    {
        DetachCardState(card);
        RemoveChild(card);
        card.QueueFree();
    }

    #endregion
}