using System.Collections.Generic;

namespace BabelRush.Data;

public interface IData<out TSelf> where TSelf : IData<TSelf>
{
    public static abstract TSelf FromEntry(IDictionary<string, object> entry);
}