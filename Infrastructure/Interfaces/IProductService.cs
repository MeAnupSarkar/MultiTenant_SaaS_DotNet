using SaaS.WebApp.Model.Entity.Tables;

namespace SaaS.WebApp.Infrastruture.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateAsync(string name, string description, int rate);
        Task<Product> GetByIdAsync(int id);

        Task<Product> UpdateAsync(string name, string description, int rate);


        Task<bool> DeleteByIdAsync(int id);

        Task<IReadOnlyList<Product>> GetAllAsync();
    }
}
