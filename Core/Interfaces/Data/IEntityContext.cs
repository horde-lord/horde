namespace Horde.Core.Interfaces.Data
{
    public interface IEntityContext
    {
        ContextNames Name { get; }
        Task SaveChanges();
        /// <summary>
        /// Use this only on testing!
        /// </summary>
        void CreateDatabase();
        /// <summary>
        /// Use this only on testing
        /// </summary>
        void CreateSchemaObjects();
        void DeleteDatabase();
    }
}
