namespace Horde.Core.Utilities
{
    public static class Batcher
    {
        public static List<List<T>> Batch<T>(int batchSize, List<T> source)
        {
            List<List<T>> output = new List<List<T>>();
            int totalItems = source.Count;
            int numberOfBatches = Convert.ToInt32(Math.Ceiling((decimal)(totalItems / batchSize))) + 1;
            for (int i = 0; i < numberOfBatches; i++)
            {
                var batch = source.Skip(i * batchSize).Take(batchSize).ToList();
                output.Add(batch);
            }
            return output;
        }
    }
}
