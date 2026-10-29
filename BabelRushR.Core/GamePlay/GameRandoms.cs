using KirisameY.Randomizer;
using KirisameY.Randomizer.RandomGenerators;

namespace BabelRushR.Core.GamePlay;

internal class GameRandoms(ulong seed)
{
    public ulong Seed => seed;

    private readonly Dictionary<string, IRandomizer> _randomizers = [];

    public IRandomizer GetRandomizer(string scopeName)
    {
        if (!_randomizers.TryGetValue(scopeName, out var result))
        {
            var l = HashCode.Combine(unchecked((int)(seed & int.MaxValue)), scopeName);
            var h = HashCode.Combine(unchecked((int)(seed >>> 32)), scopeName);
            var scopeSeed = ((ulong)h << 32) + (ulong)l;
            _randomizers[scopeName] = result = new XorShiftStarGenerator(scopeSeed);
        }

        return result;
    }
}