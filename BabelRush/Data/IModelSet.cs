using System.Collections.Generic;

namespace BabelRush.Data;

public interface IModelSet<out TModel>
{
    IReadOnlyCollection<TModel> CheckAll(out string[] errors);
}