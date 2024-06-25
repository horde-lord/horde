namespace Horde.Core.Utilities
{
    public static class EnumerableExtensions
    {

        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IList<T> source, int chunkSize)
        {
            for (int i = 0; i < source.Count; i += chunkSize)
                yield return source.Skip(i).Take(chunkSize);
        }


        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));
        public static IEnumerable<T> DuplicateBy<T>(this IEnumerable<T> self, Func<T, object> predicate, 
            Func<T, bool> filter = null)
        {
            if(filter is null)
                return self.GroupBy(predicate).Where(s => s.Count() > 1).SelectMany(x => x);
            else
                return self.GroupBy(predicate).Where(s => s.Count() > 1).SelectMany(x => x).Where(filter);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
                return true;
            return false;
        }

        

        public static bool Empty<T>(this IEnumerable<T> list)
        {
            return list.Count() == 0;
        }

        public static void AddIfNotNull<T>(this IList<T> list, params T[] items)
        {
            foreach(var item in items)
            {
                if (item != null)
                    list.Add(item);
            }
        }
    }
}
