using System.Data;

namespace Horde.Core.Interfaces.Data
{
    public interface IRepoReader
    {

        IEnumerable<T> Get<T>(string sql, object param = null, Dictionary<string, string> filters = null,
            KeyValuePair<string, bool>? sortAscending = null, Type[] types = null, Func<object[], T> mapFunction = null,
            Func<List<T>, List<T>> reduceFunction = null, int? pageSize = null, int? pageNumber = null);

        Task<IEnumerable<T>> GetAsync<T>(string sql, object param = null, Dictionary<string, string> filters = null,
            KeyValuePair<string, bool>? sortAscending = null, Type[] types = null, Func<object[], T> mapFunction = null,
            Func<List<T>, List<T>> reduceFunction = null, int? pageSize = null, int? pageNumber = null);


        Dictionary<Type, List<object>> GetBatch(string sql, object param, Type[] typesToRead);

        IEnumerable<T> GetUsingExistingConnection<T>(string sql, IDbConnection dbConnection, object param = null);
    }

}
