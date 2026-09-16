using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BabelRushR.Core.Common;

/// <summary>
/// Core 内所有需要 <see cref="INotifyPropertyChanged"/> 的类型的基类。
/// 只做属性变化通知，不承载任何游戏逻辑。
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// 值发生实际变化时写入并通知，返回是否发生了变化。
    /// </summary>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
