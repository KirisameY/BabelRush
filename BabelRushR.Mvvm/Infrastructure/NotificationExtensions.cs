using System.ComponentModel;

namespace BabelRushR.Mvvm.Infrastructure;

/// <summary>
///     <see cref="INotifyPropertyChanged"/> 的订阅辅助：把"订阅 + 退订"收敛成一个
///     <see cref="IDisposable"/>，便于交给 <see cref="ViewModelBase.Track"/> 统一管理。
/// </summary>
public static class NotificationExtensions
{
    extension(INotifyPropertyChanged source)
    {
        /// <summary>
        ///     订阅属性变化。
        /// </summary>
        /// <returns>
        ///     撤销该订阅的 <see cref="IDisposable"/>。
        /// </returns>
        public IDisposable Subscribe(PropertyChangedEventHandler handler)
        {
            source.PropertyChanged += handler;
            return new ActionDisposable(() => source.PropertyChanged -= handler);
        }
    }
}
