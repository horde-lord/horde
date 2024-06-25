using Horde.Core.Domains;

namespace Horde.Core.Interfaces.Data
{
    public interface IEntityContextRepository<out TContext> where TContext : IEntityContext
    {
        IQueryable<TEntity> GetQueryable<TEntity>(List<string> includes = null) where TEntity : BaseEntity;
        IQueryable<TEntity> GetQueryable<TEntity>(params string[] includes) where TEntity : BaseEntity;
        Task SaveChanges();
        TEntity Find<TEntity>(int id) where TEntity : BaseEntity;
        bool IsAlreadyAttached<TEntity>(TEntity entity) where TEntity : BaseEntity;
        void Upsert<TEntity>(TEntity entity) where TEntity : BaseEntity;
        void UpsertRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;
        IQueryable<TEntity> GetNoTrackingQueryable<TEntity>(List<string> includes = null) where TEntity : BaseEntity;
        IQueryable<TEntity> GetNoTrackingQueryable<TEntity>(params string[] includes) where TEntity : BaseEntity;
        IQueryable<TEntity> GetNoTrackingQueryableWithNoFilters<TEntity>(params string[] includes) where TEntity : BaseEntity;
        // void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;
        ContextNames Name { get; }
        /// <summary>
        /// Use this only on testing!
        /// </summary>
        void CreateDatabase();
        /// <summary>
        /// Use this only on testing
        /// </summary>
        void CreateSchemaObjects();

        void DeleteDatabase();
        Guid BeginTransaction();
        void CommitTransaction(Guid transactionId);
        void RollbackTransaction(Guid transactionId);
        void ClearTracker();
    }

    public enum ContextNames
    {
        Ecosystem,
        League,
        Money,
        Game,
        QuizGame,
        CashFree,
        Marketing,
        Analysis,
        Commerce,
        Partners
    }
}
