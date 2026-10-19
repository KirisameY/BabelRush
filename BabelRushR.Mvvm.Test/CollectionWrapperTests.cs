using System.Collections;

using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     <see cref="CollectionWrapper"/> 把源集合翻译成 VM 集合时的契约。
/// </summary>
/// <remarks>
///     具体 VM 的测试只保留各自的属性映射与生命周期；增删替换移动、目标释放和订阅撤销集中在这里钉住。
/// </remarks>
public class CollectionWrapperTests
{
    private static NotifiableList<int> NewList(params int[] values)
    {
        var list = new NotifiableList<int>();
        list.AddRange(values);
        return list;
    }

    private static int[] Values(IEnumerable<Target> targets) => [.. targets.Select(target => target.Value)];

    private static int[] SortedValues(IEnumerable<Target> targets) =>
        [.. targets.Select(target => target.Value).Order()];

    /// <summary>记录自己被释放了几次；<see cref="CollectionWrapper"/> 只应释放实现了 <see cref="IDisposable"/> 的目标。</summary>
    private sealed class Target(int value) : IDisposable
    {
        public int Value => value;
        public int DisposeCount { get; private set; }
        public bool IsDisposed => DisposeCount > 0;

        public void Dispose() => DisposeCount++;
    }

    // ── List：有索引、有顺序 ──────────────────────────────────────────────

    [Fact]
    public void List_Maps_Items_Already_In_The_Source()
    {
        var source = NewList(1, 2, 3);

        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });

        Assert.Equal(new[] { 1, 2, 3 }, Values(output));
    }

    [Fact]
    public void List_Add_And_Insert_Use_Source_Indexes()
    {
        var source = NewList(1);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var first = Assert.Single(output);

        source.Insert(0, 2);
        source.Add(3);

        Assert.Equal(new[] { 2, 1, 3 }, Values(output));
        Assert.Same(first, output[1]);
    }

    [Fact]
    public void List_Batch_Add_Keeps_Source_Order()
    {
        var source = NewList(1);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });

        source.AddRange([2, 3]);

        Assert.Equal(new[] { 1, 2, 3 }, Values(output));
    }

    [Fact]
    public void List_Remove_Removes_And_Disposes_The_Wrapper()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var removed = output[0];

        source.RemoveAt(0);

        Assert.Equal(new[] { 2 }, Values(output));
        Assert.Equal(1, removed.DisposeCount);
    }

    [Fact]
    public void List_Replace_Swaps_And_Disposes_The_Old_Wrapper()
    {
        var source = NewList(1);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var old = Assert.Single(output);

        source[0] = 5;

        var replacement = Assert.Single(output);
        Assert.Equal(5, replacement.Value);
        Assert.NotSame(old, replacement);
        Assert.Equal(1, old.DisposeCount);
    }

    [Fact]
    public void List_Clear_Removes_And_Disposes_Every_Wrapper()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var targets = output.ToArray();

        source.Clear();

        Assert.Empty(output);
        Assert.All(targets, target => Assert.Equal(1, target.DisposeCount));
    }

    [Fact]
    public void List_Move_Reorders_Without_Rebuilding_Wrappers()
    {
        var source = NewList(1, 2, 3);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var first = output[0];
        var second = output[1];
        var third = output[2];

        source.Move(0, 2);

        Assert.Same(second, output[0]);
        Assert.Same(third, output[1]);
        Assert.Same(first, output[2]);
        Assert.All(output, target => Assert.False(target.IsDisposed));
    }

    [Fact]
    public void List_Reset_Rebuilds_All_Wrappers_And_Disposes_The_Old()
    {
        var source = new ResettableList<int>(1, 2);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var old = output.ToArray();

        source.Reset(3, 4);

        Assert.Equal(new[] { 3, 4 }, Values(output));
        Assert.All(old, target => Assert.Equal(1, target.DisposeCount));
    }

    [Fact]
    public void List_Removed_Wrapper_Is_Disposed_After_The_Update_Notification()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.List(source, value => new Target(value), _ => { });
        var removed = output[0];
        bool? disposedWhenNotified = null;
        output.ListUpdated += (_, _) => disposedWhenNotified = removed.IsDisposed;

        source.RemoveAt(0);

        Assert.Equal(false, disposedWhenNotified);
        Assert.True(removed.IsDisposed);
    }

    [Fact]
    public void List_Disposing_The_Tracker_Unsubscribes_And_Disposes_All_Wrappers()
    {
        var source = NewList(1, 2);
        var tracked = new List<IDisposable>();
        var output = CollectionWrapper.List(source, value => new Target(value), tracked.Add);
        var targets = output.ToArray();

        var subscription = Assert.Single(tracked);
        subscription.Dispose();

        Assert.Empty(output);
        Assert.All(targets, target => Assert.Equal(1, target.DisposeCount));

        source.Add(3);
        Assert.Empty(output);
    }

    [Fact]
    public void List_Does_Not_Require_Targets_To_Be_Disposable()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.List(source, value => $"#{value}", _ => { });

        Assert.Equal(new[] { "#1", "#2" }, output);
        source.RemoveAt(0);
        source.Clear();

        Assert.Empty(output);
    }

    // ── Collection：无序、不暴露索引 ──────────────────────────────────────

    [Fact]
    public void Collection_Maps_Items_Already_In_The_Source()
    {
        var source = NewList(1, 2, 3);

        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });

        Assert.Equal(new[] { 1, 2, 3 }, SortedValues(output));
    }

    [Fact]
    public void Collection_Add_Keeps_Existing_Wrappers()
    {
        var source = NewList(1);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var first = Assert.Single(output);

        source.Add(2);

        Assert.Contains(first, output);
        Assert.Equal(new[] { 1, 2 }, SortedValues(output));
    }

    [Fact]
    public void Collection_Remove_Removes_And_Disposes_The_Wrapper()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var removed = output.Single(target => target.Value == 1);

        source.Remove(1);

        Assert.Equal(new[] { 2 }, SortedValues(output));
        Assert.Equal(1, removed.DisposeCount);
    }

    [Fact]
    public void Collection_Replace_Swaps_And_Disposes_The_Old_Wrapper()
    {
        var source = NewList(1);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var old = Assert.Single(output);

        source[0] = 5;

        var replacement = Assert.Single(output);
        Assert.Equal(5, replacement.Value);
        Assert.NotSame(old, replacement);
        Assert.Equal(1, old.DisposeCount);
    }

    [Fact]
    public void Collection_Clear_Removes_And_Disposes_Every_Wrapper()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var targets = output.ToArray();

        source.Clear();

        Assert.Empty(output);
        Assert.All(targets, target => Assert.Equal(1, target.DisposeCount));
    }

    [Fact]
    public void Collection_Ignores_Moves()
    {
        var source = NewList(1, 2, 3);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var targets = output.ToArray();

        source.Move(0, 2);

        Assert.Equal(3, output.Count);
        Assert.All(targets, target => Assert.Contains(target, output));
        Assert.All(targets, target => Assert.False(target.IsDisposed));
    }

    [Fact]
    public void Collection_Reset_Rebuilds_All_Wrappers_And_Disposes_The_Old()
    {
        var source = new ResettableCollection<int>(1, 2);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var old = output.ToArray();

        source.Reset(3, 4);

        Assert.Equal(new[] { 3, 4 }, SortedValues(output));
        Assert.All(old, target => Assert.Equal(1, target.DisposeCount));
    }

    [Fact]
    public void Collection_Removed_Wrapper_Is_Disposed_After_The_Update_Notification()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.Collection(source, value => new Target(value), _ => { });
        var removed = output.Single(target => target.Value == 1);
        bool? disposedWhenNotified = null;
        output.CollectionUpdated += (_, _) => disposedWhenNotified = removed.IsDisposed;

        source.Remove(1);

        Assert.Equal(false, disposedWhenNotified);
        Assert.True(removed.IsDisposed);
    }

    [Fact]
    public void Collection_Disposing_The_Tracker_Unsubscribes_And_Disposes_All_Wrappers()
    {
        var source = NewList(1, 2);
        var tracked = new List<IDisposable>();
        var output = CollectionWrapper.Collection(source, value => new Target(value), tracked.Add);
        var targets = output.ToArray();

        var subscription = Assert.Single(tracked);
        subscription.Dispose();

        Assert.Empty(output);
        Assert.All(targets, target => Assert.Equal(1, target.DisposeCount));

        source.Add(3);
        Assert.Empty(output);
    }

    [Fact]
    public void Collection_Does_Not_Require_Targets_To_Be_Disposable()
    {
        var source = NewList(1, 2);
        var output = CollectionWrapper.Collection(source, value => $"#{value}", _ => { });

        Assert.Equal(new[] { "#1", "#2" }, output.Order());
        source.Remove(1);
        source.Clear();

        Assert.Empty(output);
    }

    // ── Reset 源：NotifiableList 的公开操作不会产生 Reset，只能手造 ─────────

    private sealed class ResettableList<T>(params T[] items) : IReadOnlyNotifiableList<T>
    {
        private readonly List<T> _items = [.. items];

        public event EventHandler<ListUpdateEventArgs<T>>? ListUpdated;
        public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated;

        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Reset(params T[] items)
        {
            _items.Clear();
            _items.AddRange(items);
            var args = new ResetEventArgs(_items);
            ListUpdated?.Invoke(this, args);
            CollectionUpdated?.Invoke(this, args);
        }

        private sealed class ResetEventArgs(IReadOnlyList<T> view)
            : ListUpdateEventArgs<T>(view), IListResetEventArgs<T>;
    }

    private sealed class ResettableCollection<T>(params T[] items) : IReadOnlyNotifiableCollection<T>
    {
        private readonly List<T> _items = [.. items];

        public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated;

        public int Count => _items.Count;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Reset(params T[] items)
        {
            _items.Clear();
            _items.AddRange(items);
            CollectionUpdated?.Invoke(this, new ResetEventArgs(_items));
        }

        private sealed class ResetEventArgs(IReadOnlyCollection<T> view)
            : CollectionUpdateEventArgs<T>(view), ICollectionResetEventArgs<T>;
    }
}
