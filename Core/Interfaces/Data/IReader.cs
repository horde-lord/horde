using Core.Domains;

namespace Core.Interfaces.Data
{
    public interface IReader<TEntity>
        where TEntity : BaseEntity
    {
        bool Exists(string key);
    }
}
