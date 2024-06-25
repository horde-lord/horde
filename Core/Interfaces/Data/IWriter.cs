using Horde.Core.Domains;

namespace Horde.Core.Interfaces.Data
{
    public interface IWriter<TEntity>
        where TEntity : BaseEntity
    {
        TEntity Insert(TEntity entity);
        List<TEntity> InsertRange(List<TEntity> entities);
    }
}
