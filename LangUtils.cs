using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HNZ.Utils
{
    // utils without dependency to game
    public static class LangUtils
    {
        public static string SeqToString<T>(this IEnumerable<T> self)
        {
            return $"[{string.Join(", ", self)}]";
        }

        public static string DicToString<K, V>(this IDictionary<K, V> self)
        {
            return $"{{{string.Join(", ", self.Select(p => $"{p.Key}: {p.Value}"))}}}";
        }

        public static void DequeueAll<T>(this ConcurrentQueue<T> self, ICollection<T> other = null)
        {
            T element;
            while (self.TryDequeue(out element))
            {
                other?.Add(element);
            }
        }

        public static double ElapsedMilliseconds(this Stopwatch self)
        {
            return (double)self.ElapsedTicks / Stopwatch.Frequency;
        }

        public static bool AddRange<T>(this ISet<T> self, IEnumerable<T> others)
        {
            var added = false;
            foreach (var other in others)
            {
                added |= self.Add(other);
            }

            return added;
        }

        public static void Increment<K>(this IDictionary<K, int> self, K key, int increment)
        {
            int value;
            self.TryGetValue(key, out value);
            value += increment;
            self[key] = value;
        }
    }
}