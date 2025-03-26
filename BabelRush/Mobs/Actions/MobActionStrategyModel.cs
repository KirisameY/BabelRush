using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Data;

namespace BabelRush.Mobs.Actions;

public class MobActionStrategyModel : IDictionary<string, List<MobActionTemplateModel>>
{
    private IDictionary<string, List<MobActionTemplateModel>> _dictionaryImplementation = new Dictionary<string, List<MobActionTemplateModel>>();

    public bool Check(out string[] errors)
    {
        List<string> errorList = [];

        foreach (var (key, models) in _dictionaryImplementation)
        {
            foreach (var model in models)
            {
                if (!model.Check(out string[] strings)) errorList.AddRange(strings.Select(s => $"{s} in {key}"));
            }
        }

        errors = errorList.ToArray();
        return errorList.Count == 0;
    }

    public CommonMobActionStrategy Convert(string nameSpace) =>
        new(this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(m => m.Convert(nameSpace)).ToList()));


    #region Implementation

    IEnumerator<KeyValuePair<string, List<MobActionTemplateModel>>> IEnumerable<KeyValuePair<string, List<MobActionTemplateModel>>>.GetEnumerator()
    {
        return _dictionaryImplementation.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)_dictionaryImplementation).GetEnumerator();
    }

    public void Add(KeyValuePair<string, List<MobActionTemplateModel>> item)
    {
        _dictionaryImplementation.Add(item);
    }

    public void Clear()
    {
        _dictionaryImplementation.Clear();
    }

    public bool Contains(KeyValuePair<string, List<MobActionTemplateModel>> item)
    {
        return _dictionaryImplementation.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, List<MobActionTemplateModel>>[] array, int arrayIndex)
    {
        _dictionaryImplementation.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, List<MobActionTemplateModel>> item)
    {
        return _dictionaryImplementation.Remove(item);
    }

    public int Count => _dictionaryImplementation.Count;
    public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

    public void Add(string key, List<MobActionTemplateModel> value)
    {
        _dictionaryImplementation.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _dictionaryImplementation.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _dictionaryImplementation.Remove(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out List<MobActionTemplateModel> value)
    {
        return _dictionaryImplementation.TryGetValue(key, out value);
    }

    public List<MobActionTemplateModel> this[string key]
    {
        get => _dictionaryImplementation[key];
        set => _dictionaryImplementation[key] = value;
    }
    public ICollection<string> Keys => _dictionaryImplementation.Keys;
    public ICollection<List<MobActionTemplateModel>> Values => _dictionaryImplementation.Values;

    #endregion
}