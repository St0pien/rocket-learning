using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Probability
{
    public class RandomPool<T>
    {
        private struct Entry
        {
            public double Limit;
            public T value;
        }

        private List<Entry> entries;
        public int Size { get { return entries.Count; } }

        public RandomPool(IEnumerable<(T value, float weight)> values)
        {
            var exp = values.Select(x => (x.value, Math.Exp(x.weight)));
            var sum = exp.Sum(x => x.Item2);
            var x = 0.0;
            entries = new List<Entry>();
            foreach (var e in exp)
            {
                x += e.Item2 / sum;
                entries.Add(new Entry()
                {
                    Limit = x,
                    value = e.value
                });
            }
        }

        public T Get()
        {
            var rnd = UnityEngine.Random.Range(0f, 1f);
            var index = entries.Count - 1;
            int i = 0;
            while (index > 0 && rnd > entries[index].Limit)
            {
                if (i++ > 1000)
                {
                    throw new Exception("buruuh");
                }
                index--;
            }
            ;
            return entries[index].value;
        }
    }
}
