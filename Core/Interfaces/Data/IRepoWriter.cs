using System.Data;

namespace Core.Interfaces.Data
{
    public interface IRepoWriter : IRepoReader
    {
        /// <summary>
        /// Execute summary update and bulk inserts. Only for admin tasks
        /// </summary>
        /// <param name="sql">dapper supported sql</param>
        /// <param name="parameters">sql parameters</param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object parameters = null);

        /// <summary>
        /// Execute synchronous bulk inserts and updates. Only for admin tasks. Supports transaction
        /// </summary>
        /// <param name="sql">dapper supported sql</param>
        /// <param name="parameters">sql parameters</param>
        /// <returns></returns>
        int Execute(string sql, object parameters = null, IDbConnection connection = null, IDbTransaction transaction = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// Runs a scalar async query on ADO. This is only for specialized cases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<T> ExecuteScalarTextQuery<T>(string sql);

        /// <summary>
        /// Executes a sql procedure which returns a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<T> ExecuteProcedureWithResult<T>(string sql, object parameters = null);

        /// <summary>
        /// Returns dynamically casted result from a stored procedure
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<dynamic> ExecuteProcedureWithDynamicResult(string sql,
            object parameters = null);
        IDbConnection GetConnection();
    }
}
