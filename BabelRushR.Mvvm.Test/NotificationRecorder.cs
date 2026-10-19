using System.ComponentModel;

namespace BabelRushR.Mvvm.Test;

/// <summary>订阅通知并把它们记录下来，供断言使用。</summary>
internal static class NotificationRecorder
{
    /// <summary>记录属性变化通知携带的属性名。</summary>
    public static List<string?> PropertyNames(INotifyPropertyChanged source)
    {
        var names = new List<string?>();
        source.PropertyChanged += (_, e) => names.Add(e.PropertyName);
        return names;
    }
}
