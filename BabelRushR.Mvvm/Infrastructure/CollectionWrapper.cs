using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Mvvm.Infrastructure;

public static class CollectionWrapper
{
    public static IReadOnlyNotifiableCollection<TTarget> Collection<TSource, TTarget>(
        IReadOnlyNotifiableCollection<TSource> source, Func<TSource, TTarget> wrapper, Action<IDisposable> tracker
    ) where TSource : notnull where TTarget : notnull
    {
        var dict = source.ToNotifiableDictionary(s => s, wrapper);
        var dispose = typeof(TTarget).IsAssignableTo(typeof(IDisposable));

        source.CollectionUpdated += Handler;
        tracker.Invoke(new ActionDisposable(() =>
        {
            source.CollectionUpdated -= Handler;
            var snapshot = dict.Clear();
            if (dispose) snapshot.Values.ForEach(d => ((IDisposable)d).Dispose());
        }));

        return dict.Values;

        void Handler(object? _, CollectionUpdateEventArgs<TSource> args)
        {
            IEnumerable<TTarget>? removedItems = null;

            switch (args)
            {
                case ICollectionItemAddedEventArgs<TSource> added:
                    dict.AddRange(added.AddedItems.ToDictionary(s => s, wrapper));
                    break;
                case ICollectionItemClearedEventArgs<TSource>:
                    removedItems = dict.Clear().Values;
                    break;
                case ICollectionItemRemovedEventArgs<TSource> removed:
                    removedItems = dict.RemoveRange(removed.RemovedItems).Values;
                    break;
                case ICollectionItemReplacedEventArgs<TSource> replaced:
                    removedItems = dict.RemoveRange(replaced.OldItems).Values;
                    dict.AddRange(replaced.NewItems.ToDictionary(s => s, wrapper));
                    break;
                case ICollectionResetEventArgs<TSource> reset:
                    removedItems = dict.Clear().Values;
                    dict.AddRange(reset.CollectionView.ToDictionary(s => s, wrapper));
                    break;
            }

            if (dispose) removedItems?.ForEach(d => ((IDisposable)d).Dispose());
        }
    }

    public static IReadOnlyNotifiableList<TTarget> List<TSource, TTarget>(
        IReadOnlyNotifiableList<TSource> source, Func<TSource, TTarget> wrapper, Action<IDisposable> tracker
    ) where TSource : notnull where TTarget : notnull
    {
        var list = source.Select(wrapper).ToNotifiableList();
        var dispose = typeof(TTarget).IsAssignableTo(typeof(IDisposable));

        source.ListUpdated += Handler;
        tracker.Invoke(new ActionDisposable(() =>
        {
            source.ListUpdated -= Handler;
            var snapshot = list.Clear();
            if (dispose) snapshot.ForEach(d => ((IDisposable)d).Dispose());
        }));

        return list.AsReadOnlyNotifiableList();

        void Handler(object? _, ListUpdateEventArgs<TSource> args)
        {
            IEnumerable<TTarget>? removedItems = null;

            switch (args)
            {
                case IListItemAddedEventArgs<TSource> added:
                    list.MultiInsert(added.Indexes, added.AddedItems.Select(wrapper));
                    break;
                // Cleared 派生自 Removed，必须先判
                case IListItemClearedEventArgs<TSource>:
                    removedItems = list.Clear();
                    break;
                case IListItemRemovedEventArgs<TSource> removed:
                    removedItems = list.MultiRemove(removed.Indexes);
                    break;
                case IListItemReplacedEventArgs<TSource> replaced:
                    removedItems = list.MultiReplace(replaced.Indexes, replaced.NewItems.Select(wrapper));
                    break;
                case IListItemMovedEventArgs<TSource> moved:
                    list.MultiMove(moved.OldIndexes.Zip(moved.NewIndexes));
                    break;
                case IListResetEventArgs<TSource> reset:
                    removedItems = list.Clear();
                    list.AddRange(reset.CollectionView.Select(wrapper));
                    break;
            }

            if (dispose) removedItems?.ForEach(d => ((IDisposable)d).Dispose());
        }
    }
}