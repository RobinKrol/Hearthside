using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сериализуемый словарь для сохранения через JsonUtility.
/// Использование: SerializableDictionary&lt;string, int&gt;
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

    public TValue this[TKey key]
    {
        get => dict[key];
        set { dict[key] = value; }
    }

    public bool ContainsKey(TKey key) => dict.ContainsKey(key);

    public void Add(TKey key, TValue value) => dict.Add(key, value);

    public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

    public IEnumerable<TKey> Keys => dict.Keys;
    public IEnumerable<TValue> Values => dict.Values;

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            dict[keys[i]] = values[i];
    }
}
