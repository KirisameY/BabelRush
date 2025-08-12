using System;
using System.Collections.Generic;
using System.Linq;

using KirisameLib.Randomization;

namespace BabelRush.Utils;

public static class RandomUtils
{
    public static TResult RandomItemWithWeight<TSource, TResult>(this RandomBelt random, ICollection<TSource> items, Func<TSource, double> weightSelector,
                                                                 Func<TSource, TResult> resultSelector, double? weightSum = null)
    {
        weightSum ??= items.Sum(weightSelector);
        var randomValue = random.NextDouble(weightSum.Value);
        foreach (var item in items)
        {
            randomValue -= weightSelector.Invoke(item);
            if (randomValue <= 0) return resultSelector.Invoke(item);
        }
        return resultSelector.Invoke(items.Last());
    }

    public static T RandomItemWithWeight<T>(this RandomBelt random, ICollection<T> items, Func<T, double> weightSelector, double? weightSum = null) =>
        random.RandomItemWithWeight(items, weightSelector, t => t, weightSum);

    public static T RandomItemWithWeight<T>(this RandomBelt random, ICollection<(T item, double weight)> items, double? weightSum = null) =>
        random.RandomItemWithWeight(items, t => t.weight, t => t.item, weightSum);
}