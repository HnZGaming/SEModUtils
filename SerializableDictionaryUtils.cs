using VRage.Serialization;

namespace HNZ.Utils
{
    public static class SerializableDictionaryUtils
    {
        public static bool TryGetValue<K, V>(this SerializableDictionary<K, V> self, K key, out V value)
        {
            return self.Dictionary.TryGetValue(key, out value);
        }

        public static void Clear<K, V>(this SerializableDictionary<K, V> self)
        {
            self.Dictionary.Clear();
        }
    }
}