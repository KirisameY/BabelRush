using BabelRushR.Core.Common;
using BabelRushR.Core.GamePlay;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Level;

public interface ILevelDefinition
{
    [Immutable] RegKey ID { get; }

    ILevel GenerateLevel(IGamePlay gamePlay);
}