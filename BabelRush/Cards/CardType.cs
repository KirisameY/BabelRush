﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Gui.DisplayInfos;

namespace BabelRush.Cards;

public class CardType(RegKey id, RegKey iconId, bool usable, int cost, IEnumerable<(ActionType, int)> actions, IEnumerable<FeatureType> features)
{
    public RegKey Id { get; } = id;
    public NameDesc NameDesc => Registers.CardRegisters.CardNameDesc.GetItem(Id);
    public SpriteInfo Icon => Registers.CardRegisters.CardIcon.GetItem(iconId);
    public bool Usable { get; } = usable;
    public int Cost { get; } = cost;

    [field: AllowNull, MaybeNull]
    public IImmutableList<(ActionType type, int value)> Actions => field ??= actions.ToImmutableList();

    [field: AllowNull, MaybeNull]
    public IImmutableList<FeatureType> Features => field ??= features.ToImmutableList();

    public Card NewInstance() => new CommonCard(this);

    public static CardType Default { get; } = new(RegKey.Default, RegKey.Default, false, 0, [], []);
}