using Horde.Core.Interfaces.Data;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Examples.Data
{
    public class Dap : IDisposable, IRepoReader, IRepoWriter
    {
        private readonly string _connectionString;
        //private readonly IConfiguration _configuration;

        public Dap(IConfiguration configuration)
        {
            //this._configuration = Configuration.ApplicationSettings;
            _connectionString = configuration.GetConnectionString("HordeDb");
        }


        #region Write Repository Implementation
        

        /// <summary>
        /// Upserts entity and returns new and old values
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <returns>key is new, value is old. In case of insert, value is null</returns>
        //public async Task<KeyValuePair<BaseEntity, BaseEntity>> UpsertAndRetreiveEntity(BaseEntity baseEntity)
        //{
        //    BaseEntity last = null;

        //    if(baseEntity.Id > 0)
        //    {
        //        var sql = "select top 1 * from " + TableResolver.GetTableName(baseEntity.GetType()) + " where id=@id";
        //        using (var db = new SqlConnection(_connectionString))
        //        {
        //            var entity = await db.QueryFirstOrDefaultAsync(sql, new { id = baseEntity.Id });
        //            last = (BaseEntity)_mapper.Map(entity, typeof(object), baseEntity.GetType());
        //        }
        //    }
        //    var output = await UpsertEntity(baseEntity);

        //    if (output > 0)
        //    {
        //        baseEntity.Id = output.Value;

        //    }

        //    return new KeyValuePair<BaseEntity, BaseEntity>(baseEntity, last);

        //}

        /// <summary>
        /// Execute summary update and bulk inserts. Only for admin tasks
        /// </summary>
        /// <param name="sql">dapper supported sql</param>
        /// <param name="parameters">sql parameters</param>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteAsync(sql, parameters);
            }
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString); 
        }
        

        /// <summary>
        /// Execute summary update and bulk inserts. Only for admin tasks
        /// </summary>
        /// <param name="sql">dapper supported sql</param>
        /// <param name="parameters">sql parameters</param>
        /// <returns></returns>
        public int Execute(string sql, object parameters = null, IDbConnection connection = null, IDbTransaction transaction = null, CommandType commandType = CommandType.Text)
        {
            int output = 0;
            if (connection != null)
            {
                output = connection.Execute(sql, parameters, transaction, null, commandType);
            }
            using (var _db = new SqlConnection(_connectionString))
            {

                output = _db.Execute(sql, parameters, null, null, commandType);
            }
            Log.Information("Executed {sql} with params {parameters}", sql, parameters);
            return output;
        }

        public IEnumerable<T> ExecuteProcedureWithResult<T>(string sql, object parameters = null)
        {
            using (var _db = new SqlConnection(_connectionString))
            {
                var output = _db.Query<T>(sql, parameters, commandType: CommandType.StoredProcedure);
                return output;
            }
        }

        public IEnumerable<dynamic> ExecuteProcedureWithDynamicResult(string sql, object parameters = null)
        {
            using (var _db = new SqlConnection(_connectionString))
            {
                var output = _db.Query(sql, parameters, null, true, null, CommandType.StoredProcedure);
                return output;
            }
        }

        #endregion

        #region Read Repository Implementation

        private DapperParameterizedModel GetFilteredSql(string sql, DynamicParameters parameters, Dictionary<string, string> filters)
        {
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (!sql.Contains(" where "))
                        sql += $" where {filter.Key} = @{filter.Key}";
                    else
                        sql += $" and {filter.Key} = @{filter.Key}";
                    parameters.Add(filter.Key, filter.Value);
                }
            }


            return new DapperParameterizedModel { Sql = sql, Parameters = parameters };
        }

        private DapperParameterizedModel GetSortedFilteredSql(string sql, DynamicParameters parameters, Dictionary<string, string> filters, KeyValuePair<string, bool>? sortAscending)
        {
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (!sql.Contains(" where "))
                        sql += $" where {filter.Key} = @{filter.Key}";
                    else
                        sql += $" and {filter.Key} = @{filter.Key}";
                    parameters.Add(filter.Key, filter.Value);
                }
            }

            if (sortAscending != null)
            {
                sql += $" order by {sortAscending.Value.Key} ";
                if (sortAscending.Value.Value == false)
                {
                    sql += "desc";
                }
            }

            return new DapperParameterizedModel { Sql = sql, Parameters = parameters };
        }

        public async Task<T> ExecuteScalarTextQuery<T>(string sql)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                var command = db.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                var output = (T)(await command.ExecuteScalarAsync());
                db.Close();
                return output;
            }
        }


        public IEnumerable<T> GetUsingExistingConnection<T>(string sql, IDbConnection dbConnection, object param = null)
        {
            return dbConnection.Query<T>(sql, param);
        }
        public IEnumerable<T> Get<T>(string sql, object param = null, Dictionary<string, string> filters = null,
            KeyValuePair<string, bool>? sortAscending = null, Type[] types = null, Func<object[], T> mapFunction = null,
            Func<List<T>, List<T>> reduceFunction = null, int? pageSize = null, int? pageNumber = null)
        {
            DapperParameterizedModel dapperParameterizedModel;
            DynamicParameters parameters = new DynamicParameters(param);

            dapperParameterizedModel = GetSortedFilteredSql(sql, parameters, filters, sortAscending);

            dapperParameterizedModel = GetPaginatedSql(dapperParameterizedModel.Sql, dapperParameterizedModel.Parameters, pageSize, pageNumber);

            using (var db = new SqlConnection(_connectionString))
            {
                try
                {
                    if (mapFunction == null)
                        return db.Query<T>(dapperParameterizedModel.Sql, dapperParameterizedModel.Parameters);
                    else
                    {
                        var list = db.Query<T>(dapperParameterizedModel.Sql, types, mapFunction, dapperParameterizedModel.Parameters).ToList();
                        if (reduceFunction != null)
                            list = reduceFunction(list);
                        return list;
                    }
                }
                catch(Exception x)
                {
                    Log.Error("Could not run {s} due to {x}. Params {@p}", sql, x.Message, param??"");
                    throw;
                }
            }
        }



        private DapperParameterizedModel GetPaginatedSql(string sql, DynamicParameters parameters, int? pageSize, int? pageNumber)
        {
            if (pageSize.HasValue && pageNumber.HasValue)
            {
                sql += $" OFFSET @pageSize * (@pageNumber - 1) ROWS FETCH NEXT @pageSize ROWS ONLY";
                parameters.Add("pageSize", pageSize.Value);
                parameters.Add("pageNumber", pageNumber.Value);
            }
            return new DapperParameterizedModel { Sql = sql, Parameters = parameters };
        }

        public Dictionary<Type, List<object>> GetBatch(string sql, object param, Type[] typesToRead)
        {
            Dictionary<Type, List<object>> output = new Dictionary<Type, List<object>>();
            using (var db = new SqlConnection(_connectionString))
            {
                var data = db.QueryMultiple(sql, param);
                foreach (var type in typesToRead)
                {
                    output.Add(type, data.Read(type).AsList());       
                }
            }

            return output;
        }

        public async Task<Dictionary<Type, List<object>>> GetBatchAsync(string sql, object param, Type[] typesToRead)
        {
            Dictionary<Type, List<object>> output = new Dictionary<Type, List<object>>();
            using (var db = new SqlConnection(_connectionString))
            {
                var data = await db.QueryMultipleAsync(sql, param);
                foreach (var type in typesToRead)
                {
                    output.Add(type, data.Read(type).AsList());
                }
            }
            return output;
        }


        public async Task<IEnumerable<T>> GetAsync<T>(string sql, object param = null, Dictionary<string, string> filters = null, KeyValuePair<string, bool>? sortAscending = null, Type[] types = null, Func<object[], T> mapFunction = null, Func<List<T>, List<T>> reduceFunction = null, int? pageSize = null, int? pageNumber = null)
        {
            DapperParameterizedModel dapperParameterizedModel;
            DynamicParameters parameters = new DynamicParameters(param);
            dapperParameterizedModel = GetSortedFilteredSql(sql, parameters, filters, sortAscending);

            dapperParameterizedModel = GetPaginatedSql(dapperParameterizedModel.Sql, dapperParameterizedModel.Parameters, pageSize, pageNumber);

            using (var db = new SqlConnection(_connectionString))
            {
                if (mapFunction == null)
                    return await db.QueryAsync<T>(dapperParameterizedModel.Sql, dapperParameterizedModel.Parameters);
                else
                {
                    var list = await db.QueryAsync<T>(dapperParameterizedModel.Sql, types, mapFunction, dapperParameterizedModel.Parameters);
                    if (reduceFunction != null)
                        list = reduceFunction(list.ToList());
                    return list;
                }
            }
        }




        #endregion

        public void Dispose()
        {

        }

       
    }

    internal class DapperParameterizedModel
    {
        public string Sql { get; set; }
        public DynamicParameters Parameters { get; set; }
    }
}
