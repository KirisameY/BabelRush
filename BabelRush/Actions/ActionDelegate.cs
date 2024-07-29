using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public delegate void ActionDelegate(Mob self, IReadOnlyList<Mob> targets, int value);