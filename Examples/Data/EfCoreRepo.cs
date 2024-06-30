using Autofac;
using Horde.Core.Domains;
using Horde.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

namespace Examples.Data
{
    public class EfCoreRepo<TContext> : IEntityContextRepository<TContext>
        where TContext : DbContext, IEntityContext
    {
        private readonly TContext _db;
        
        //private readonly IReadEntityRepository _readRepository;
        private IDbContextTransaction _transaction;

        public EfCoreRepo(TContext context
            //, IReadEntityRepository readEntityRepository
            )
        {
            _db = context;
            
            //_readRepository = readEntityRepository;

        }
        
        public ContextNames Name { get => _db.Name; }

        public IQueryable<TQuery> GetQueryable<TQuery>(List<string>? includes = null) where TQuery : BaseEntity
        {
            var query = _db.Set<TQuery>().AsQueryable();
            if (includes != null)
            {
                includes.ForEach(i => query = query.Include(i));
            }
            
            return query;
        }

        public bool IsAlreadyAttached<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            return _db.Entry(entity).State != EntityState.Detached;
        }



        public TEntity? Find<TEntity>(int id)
            where TEntity : BaseEntity
        {

            return _db.Find<TEntity>(id);
        }

        public IQueryable<TQuery> GetQueryable<TQuery>(params string[] includes) where TQuery : BaseEntity
        {
            var query = _db.Set<TQuery>().AsQueryable();
            if (includes?.Count() > 0)
            {
                includes.ToList().ForEach(i => query = query.Include(i));
            }
            return query;
        }
        public IQueryable<TQuery> GetNoTrackingQueryable<TQuery>(params string[] includes) where TQuery : BaseEntity
        {
            var query = _db.Set<TQuery>().AsQueryable();
            if (includes?.Count() > 0)
            {
                includes.ToList().ForEach(i => query = query.Include(i));
            }
            
            return query.AsNoTracking();
        }
        public IQueryable<TQuery> GetNoTrackingQueryable<TQuery>(List<string> includes = null) where TQuery : BaseEntity
        {
            var query = _db.Set<TQuery>().AsQueryable();
            if (includes != null)
            {
                includes.ForEach(i => query = query.Include(i));
            }
            
            return query.AsNoTracking();
        }


        public async Task SaveChanges()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while making changes to database");
                throw;
            }
            
        }

        public void Upsert<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            //if (_db.Entry(entity)?.State == EntityState.Modified)
            //    return;
            //if (_db.Entry(entity)?.State == EntityState.Added)
            //    return;
            
            _db.Update(entity);


            //_contextService.Context.AddDirtyContext(_db);
        }
        public Guid BeginTransaction()
        {
            _transaction = _db.Database.BeginTransaction();
            return _transaction.TransactionId;
        }

        public void CommitTransaction(Guid transactionId)
        {
            _transaction.Commit();
            _transaction.Dispose();
        }

        public void RollbackTransaction(Guid transactionId)
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
        

        public void UpsertRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            _db.UpdateRange(entities);
            //_contextService.Context.AddDirtyContext(_db);
        }

        public void CreateDatabase()
        {
            _db.CreateDatabase();
        }

        public void CreateSchemaObjects()
        {
            _db.CreateSchemaObjects();
        }

        public void DeleteDatabase()
        {
            _db.DeleteDatabase();
        }

        public void ClearTracker()
        {
            _db.ChangeTracker.Clear();
        }

        public IQueryable<TEntity> GetNoTrackingQueryableWithNoFilters<TEntity>(params string[] includes) where TEntity : BaseEntity
        {
            var query = GetNoTrackingQueryable<TEntity>(includes);
            return query.IgnoreQueryFilters();
        }
    }
}
