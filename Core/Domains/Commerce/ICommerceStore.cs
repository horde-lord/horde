namespace Horde.Core.Domains.Commerce
{
    public interface ICommerceStore
    {
        Task<List<Product>> FindProducts(string search);
    }
}
