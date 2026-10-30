namespace BabelRushR.Core.Common;

public readonly ref struct FlagHolder(ref bool flag) : IDisposable
{
    private readonly ref bool _flag = ref flag;

    public void Dispose()
    {
        _flag = false;
    }
}