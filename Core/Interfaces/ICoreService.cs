using Autofac;
using Core.Domains;
using Core.Interfaces.Data;

namespace Core.Interfaces
{
    public interface ICoreService
    {
        ILifetimeScope Scope { get; }
        T Get<T>() => Scope.Resolve<T>();
        T Get<T>(string name) => Scope.ResolveNamed<T>(name) ?? throw new NullReferenceException($"Could not resolve type {typeof(T).Name} with name {name}");
        T Get<T>(object key) => Scope.ResolveKeyed<T>(key) ?? throw new NullReferenceException($"Could not resolve type {typeof(T).Name} with key {key.ToString()}");

        List<IEntityContextRepository<IEntityContext>> GetRepositories()
        {
            return Scope.Resolve<IEnumerable<IEntityContextRepository<IEntityContext>>>().ToList();
        }
        IEntityContextRepository<IEntityContext> GetRepository(ContextNames name)
        {

            return GetRepositories().LastOrDefault(r => r.Name == name);
        }

        public virtual IEntityContextRepository<IEntityContext> GetRepository<T>() where T : BaseEntity, new()
        {

            return GetRepositories().LastOrDefault(r => r.Name == new T().Context);
        }

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
        /// <summary>
        /// Saves entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task Save<T>(T entity) where T : BaseEntity, new()
        {
            var repo = GetRepository(new T().Context);
            repo.Upsert(entity);
            await repo.SaveChanges();
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
            repo.UpsertRange(entities);
            await repo.SaveChanges();
        }
        public IRepoReader Reader => Scope.Resolve<IRepoReader>();
        public IRepoWriter Writer => Scope.Resolve<IRepoWriter>();
    }
}
