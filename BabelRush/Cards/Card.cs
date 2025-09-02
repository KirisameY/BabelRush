using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;
using BabelRush.Utils;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Logging;

namespace BabelRush.Cards;

public abstract partial class Card
{
    public abstract CardType Type { get; }
    public abstract int Cost { get; }
    public abstract IList<ActionInstance> Actions { get; }
    public abstract IList<Feature> Features { get; }
    public abstract bool TargetSelected();

    [LogToSync]
    public abstract SyncTask<bool> UseAsync(Mob user);

    public static Card Default { get; } = new CommonCard(CardType.Default);


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(Card));
}