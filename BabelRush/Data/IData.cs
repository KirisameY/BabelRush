namespace BabelRush.Data;

public interface IData<out TModel>
{
    TModel ToModel();
}

public interface ISavableData<in TModel, out TSelf> where TSelf : ISavableData<TModel, TSelf>
{
    static abstract TSelf FromModel(TModel model);
}