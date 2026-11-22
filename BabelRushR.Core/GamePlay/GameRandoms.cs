using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Text;

using KirisameY.Randomizer;
using KirisameY.Randomizer.RandomGenerators;

namespace BabelRushR.Core.GamePlay;

internal class GameRandoms(ulong seed)
{
    public ulong Seed => seed;

    private readonly Dictionary<string, IRandomizer> _randomizers = [];

    private readonly ArrayBufferWriter<byte> _buffer = new();

    public IRandomizer GetRandomizer(string scopeName)
    {
        if (!_randomizers.TryGetValue(scopeName, out var result))
        {
            ulong scopedSeed;
            lock (_buffer)
            {
                _buffer.Clear();

                var strMax = Encoding.Unicode.GetMaxByteCount(scopeName.Length);
                var strLength = Encoding.Unicode.GetBytes(scopeName, _buffer.GetSpan(strMax));
                _buffer.Advance(strLength);

                BinaryPrimitives.WriteUInt64BigEndian(_buffer.GetSpan(8), seed);
                _buffer.Advance(8);

                scopedSeed = XxHash64.HashToUInt64(_buffer.WrittenSpan);
            }
            _randomizers[scopeName] = result = new XorShiftStarGenerator(scopedSeed);
        }

        return result;
    }
}