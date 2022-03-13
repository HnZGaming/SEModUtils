using System;
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

        public static bool TryGetFirstValue<T>(this IReadOnlyList<T> self, out T value)
        {
            if (self.Count > 0)
            {
                value = self[0];
                return true;
            }

            value = default(T);
            return false;
        }

        public static TimeSpan TimeSpanSinceMidnight(this DateTime self)
        {
            return new TimeSpan(0, self.Hour, self.Minute, self.Second, self.Millisecond);
        }

        public static string HoursToString(double hours)
        {
            var d = Math.Floor(hours / 24);
            var rh = Math.Floor((hours / 24 - d) * 24);
            if (d >= 1f) return $"{d} days {rh} hours";

            var h = Math.Floor(hours);
            var rm = Math.Floor((hours - h) * 60);
            if (hours >= 1f) return $"{h} hours {rm} minutes";

            var m = Math.Floor(hours * 60);
            var rs = Math.Floor((hours * 60 - m) * 60);
            if (m >= 1f) return $"{m} minutes {rs} seconds";

            var s = Math.Floor(hours * 60 * 60);
            return $"{s} seconds";
        }

        public static bool RunOnce(ref bool called)
        {
            if (!called)
            {
                called = true;
                return true;
            }

            return false;
        }
    }
}