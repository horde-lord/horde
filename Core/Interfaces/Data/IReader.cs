using Horde.Core.Domains;

namespace Horde.Core.Interfaces.Data
{
    public interface IReader<TEntity>
        where TEntity : BaseEntity
    {
        bool Exists(string key);
    }
}
