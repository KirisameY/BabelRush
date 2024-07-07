using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public class CommonCard(ICardType type) : ICard
{
    public ICardType Type { get; } = type;
    public IList<IAction> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();

    public void Use(IEnumerable<IMob> targets)
    {
        foreach (var action in Actions)
        {
            throw new NotImplementedException();
            //应该要直接向游戏获取玩家还是让卡牌储存它的所有者呢？
        }
    }
}