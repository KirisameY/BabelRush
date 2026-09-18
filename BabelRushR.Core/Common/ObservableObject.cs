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
    /// 生成一个把 <see cref="PropertyChanged"/> 发到指定属性名上的处理器，供 Numeric 的
    /// <c>WithUpdateHandler</c> 挂接。<paramref name="propertyName"/> 默认取调用方成员名。
    /// </summary>
    protected EventHandler PropertyChangedHandler([CallerMemberName] string? propertyName = null)
        => (_, _) => OnPropertyChanged(propertyName);

    /// <summary>
    /// 值发生实际变化时写入并通知。
    /// </summary>
    /// <returns>
    /// 若发生变化则为 <c>true</c>，否则为 <c>false</c>.
    /// </returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
