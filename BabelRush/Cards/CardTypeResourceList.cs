using System.Collections;
using System.Collections.Generic;

using Godot;
using Godot.Collections;

using JetBrains.Annotations;

namespace BabelRush.Cards;

[GlobalClass]
public partial class CardTypeResourceList : Resource, IReadOnlyList<CardTypeResource>
{
    [Export]
    private Array<CardTypeResource> Array { get; set; } = [];

    [MustDisposeResource]
    public IEnumerator<CardTypeResource> GetEnumerator()
    {
        return Array.GetEnumerator();
    }

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Array).GetEnumerator();
    }

    public int Count => Array.Count;
    public CardTypeResource this[int index] => Array[index];
}