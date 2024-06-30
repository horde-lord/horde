using Autofac;
using Horde.Core.Domains;
using Horde.Core.Interfaces;
using Horde.Core.Interfaces.Data;
using Horde.Core.Utilities;
using Serilog;
using System.Globalization;
using System.Runtime.CompilerServices;
using Horde.Core.Interfaces.Comm;
using shortid.Configuration;
using shortid;
using Horde.Core.Domains.Admin.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Mapster;
using CsvHelper.Configuration;
using CsvHelper;
using Horde.Core.Domains.Admin;

namespace Horde.Core.Services
{
    public class BaseService : ICoreService
    {
        private readonly ContextNames _defaultContext;
        public BaseService(ILifetimeScope scope, ContextNames name = ContextNames.World)
        {
            Scope = scope;
            Configuration = scope.Resolve<IConfiguration>();
            Http = scope.Resolve<IHttpClientFactory>().CreateClient();
            
            _tenantManager = scope.Resolve<TenantManager>();
            _defaultContext = name;
        }

        public static ILifetimeScope RootScope { get; set; }


        public Tenant Partner => _tenantManager.GetTenant();



        public static void ThrowIfNull<T>(T t, string message = "",
            [CallerArgumentExpression("t")] string? parameterName = null,
            [CallerMemberName] string? member = null)
        {
            if (t == null)
            {
                if (string.IsNullOrEmpty(message))
                    message = $"Parameter {parameterName} of type {typeof(T).Name} called by {member} cannot be null";
                Log.Error(message);

                throw new ArgumentException(message);
            }
        }
        public static void ThrowWarningIfNull<T>(T t, string message = "")
        {
            if (t == null)
            {
                if (string.IsNullOrEmpty(message))
                    message = $"Entity {typeof(T).Name} cannot be null";
                Log.Warning(message);

                throw new Warning(message);
            }
        }
        public ILifetimeScope Scope { get; }
        public IConfiguration Configuration { get; }

        public HttpClient Http { get; }

        private readonly TenantManager _tenantManager;
        protected readonly string LOGO = "https://www.tribalarena.com/img/Tribal%20Arena%20Visual%20Identity-13.png";

        /// <summary>
        /// Resolves T from inner scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() => Scope.Resolve<T>();
        public T GetTenanted<T>(int? partnerId, bool refreshCache = false) => GetTenantScope(partnerId ?? 1,refreshCache).Resolve<T>();
        public T GetNew<T>() => Scope.GetChild(Partner.Id).Resolve<T>();
        //public ILifetimeScope RootScope => Scope.;
        public T Get<T>(string name) => Scope.ResolveNamed<T>(name) ?? throw new NullReferenceException($"Could not resolve type {typeof(T).Name} with name {name}");
        public T Get<T>(object key) => Scope.ResolveKeyed<T>(key) ?? throw new NullReferenceException($"Could not resolve type {typeof(T).Name} with key {key.ToString()}");

        public async Task RegisterConsumer(Func<BaseEvent, Task> consumer, string name, params string[] topics)
        {

            await Get<IMessenger>().RegisterConsumer(consumer: consumer, @group: name, topics: topics);
        }

        public string GenerateCode(int length = 8)
        {
            return ShortId.Generate(new GenerationOptions(useSpecialCharacters: false, length: length));
        }

        private List<IEntityContextRepository<IEntityContext>> GetRepositories()
        {
            return Scope.Resolve<IEnumerable<IEntityContextRepository<IEntityContext>>>().ToList();
        }
        public virtual IEntityContextRepository<IEntityContext> GetRepository(ContextNames name)
        {

            return GetRepositories().LastOrDefault(r => r.Name == name);
        }



        public virtual IEntityContextRepository<IEntityContext> GetRepository()
        {

            return GetRepositories().LastOrDefault(r => r.Name == _defaultContext);
        }
        public virtual IEntityContextRepository<IEntityContext> GetRepository<T>() where T : BaseEntity, new()
        {

            return GetRepositories().LastOrDefault(r => r.Name == new T().Context);
        }




        private string GetScopedKey<T>(string key) => $"{typeof(T).Name}_{key}";

        
        /// <summary>
        /// Gets a non tracking queryable for T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includes">Navigation properties to load</param>
        /// <returns></returns>
        public virtual IQueryable<T> _<T>(params string[] includes) where T : BaseEntity, new()
        {

            return GetRepository(new T().Context).GetNoTrackingQueryable<T>(includes);
        }
        /// <summary>
        /// Gets unfiltered data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual IQueryable<T> __<T>(params string[] includes) where T : BaseEntity, new()
        {
            return GetRepository(new T().Context).GetNoTrackingQueryableWithNoFilters<T>(includes);
        }

        public virtual IQueryable<TMap> _<T, TMap>(params string[] includes) where T : BaseEntity, new()
        {
            return _<T>(includes).ProjectToType<TMap>();
        }
        public virtual TMap _<T, TMap>(int id, params string[] includes) where T : BaseEntity, new()
        {
            return _<T>(id, includes).Adapt<TMap>();
        }

        public virtual T _<T>(Func<T, T> selector, int id) where T : BaseEntity, new()
        {
            if (selector == null)
                selector = (t) => t;
            return GetRepository(new T().Context).GetNoTrackingQueryable<T>().AsEnumerable().Select(selector).SingleOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Gets list of entities having the provided ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual List<T> _<T>(IEnumerable<int> ids, params string[] includes) where T : BaseEntity, new()
        {
            return GetRepository(new T().Context).GetNoTrackingQueryable<T>(includes)
                .Where(t => ids.Contains(t.Id)).ToList();
        }

        public virtual List<T> __<T>(IEnumerable<int> ids, params string[] includes) where T : BaseEntity, new()
        {
            return GetRepository(new T().Context).GetNoTrackingQueryableWithNoFilters<T>(includes)
                .Where(t => ids.Contains(t.Id)).ToList();
        }

        /// <summary>
        /// Loads non tracking entity T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Id">Id of entity</param>
        /// <param name="includes">Navigation properties to load</param>
        /// <returns>T or Null</returns>
        public virtual T _<T>(int Id, params string[] includes) where T : BaseEntity, new()
        {
            return GetRepository(new T().Context).GetNoTrackingQueryable<T>(includes)
                .SingleOrDefault(t => t.Id == Id);
        }

        public virtual T __<T>(int Id, params string[] includes) where T : BaseEntity, new()
        {
            return GetRepository(new T().Context).GetNoTrackingQueryableWithNoFilters<T>(includes)
                .SingleOrDefault(t => t.Id == Id);
            
        }

        /// <summary>
        /// Finds all elements in first set which do not intersect in second. Compared by Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public virtual List<T> Disjoint<T>(IEnumerable<T> first, IEnumerable<T> second) where T : BaseEntity
        {
            return first.ExceptBy(second.Select(t => t.Id), t => t.Id).ToList();
        }

        /// <summary>
        /// Finds all elements in first set which do not intersect in second. Compared by func of key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual List<T> Disjoint<T, TKey>(IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> f)
        {
            return first.ExceptBy(second.Select(f), f).ToList();
        }
        /// <summary>
        /// Saves entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task Save<T>(T entity) where T : BaseEntity, new()
        {
            var repo = GetRepository(new T().Context);
            if (entity.PartnerId < 1)
            {
                if (Partner.Id < 1)
                    throw new Exception($"Tenant is not set correctly at Save. Current value {Partner.Id}");
                entity.PartnerId = Partner.Id;
            }

            repo.Upsert(entity);
            await repo.SaveChanges();
            //Log.Debug("Saved {e}:{i}", typeof(T).Name, entity.Id);
        }
        /// <summary>
        /// Saves list of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task Save<T>(List<T> entities) where T : BaseEntity, new()
        {

            var repo = GetRepository(new T().Context);
            var partnerId = _tenantManager.GetTenant().Id;
            if (!(partnerId > 0))
                throw new Exception("PartnerId is not set in Save operation");
            foreach (var entity in entities)
            {
                if (entity.PartnerId < 1)
                    entity.PartnerId = partnerId;
                //repo.Upsert(entity);
            }
            repo.UpsertRange(entities);
            await repo.SaveChanges();
        }

        public IRepoReader Reader => Scope.Resolve<IRepoReader>();
        public IRepoWriter Writer => Scope.Resolve<IRepoWriter>();


        public List<TEntity> ImportFromCsv<TEntity>(string path)
        {
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            //configuration.HeaderValidated = null;
            //configuration.MissingFieldFound = null;
            //configuration.
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                //csv.Configuration.MissingFieldFound = new MissingFieldFound(target);
                var records = csv.GetRecords<TEntity>();
                return records.ToList();
            }
        }

        public List<dynamic> ImportFromGenericCsv(string path)
        {
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            configuration.HeaderValidated = null;
            configuration.MissingFieldFound = null;
            configuration.BadDataFound = null;
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, configuration))
            {

                //csv.Configuration.MissingFieldFound = new MissingFieldFound(target);
                var records = csv.GetRecords<dynamic>();
                return records.ToList();
            }
        }

        public void ExportToCsv(List<dynamic> records, string path)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                //csv.Configuration.MissingFieldFound = new MissingFieldFound(target);
                csv.WriteRecords(records);
            }
        }

        public void ExportToCsv<T>(List<T> records, string path) where T : new()
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                //csv.Configuration.MissingFieldFound = new MissingFieldFound(target);
                csv.WriteRecords(records);
            }
        }

        public string GetAssetValue(AssetType type)
        {

           return Partner.GetAsset(type);
        }

        public ILifetimeScope GetTenantScope(int? partnerId,bool refreshCache = false)
        {
            return Scope.GetChild(partnerId ?? 1,refreshCache);
        }
        public bool IsNullOrEmpty<T>(IEnumerable<T> list)
        {
            if (list == null || list.Count() == 0)
                return true;
            return false;
        }
    }



}
