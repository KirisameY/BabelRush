using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Text;

using BabelRushR.Core.Common;

using KirisameY.Randomizer;
using KirisameY.Randomizer.RandomGenerators;
using KirisameY.Registration.Data;

namespace BabelRushR.Core.GamePlay;

public sealed class GameRandoms(ulong seed)
{
    [Immutable] public ulong Seed => seed;

    private readonly Dictionary<RegKey, IRandomizer> _randomizers = [];

    private readonly ArrayBufferWriter<byte> _buffer = new();

    public IRandomizer GetRandomizer(RegKey scope) => _randomizers.GetValueOrDefault(scope) ?? ResetRandomizer(scope);

    public IRandomizer ResetRandomizer(RegKey scope)
    {
        ulong scopedSeed;
        lock (_buffer)
        {
            _buffer.Clear();

            BinaryPrimitives.WriteUInt64BigEndian(_buffer.GetSpan(8), seed);
            _buffer.Advance(8);

            var scopeName = scope.FullName;
            var strMax = Encoding.Unicode.GetMaxByteCount(scopeName.Length);
            var strLength = Encoding.Unicode.GetBytes(scopeName, _buffer.GetSpan(strMax));
            _buffer.Advance(strLength);

            scopedSeed = XxHash64.HashToUInt64(_buffer.WrittenSpan);
        }
        var result = _randomizers[scope] = new XorShiftStarGenerator(scopedSeed);
        return result;
    }
}