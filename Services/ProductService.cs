using Microsoft.EntityFrameworkCore;
using SaaS.WebApp.Data;
using SaaS.WebApp.Infrastruture.Interfaces;
using SaaS.WebApp.Model.Entity.Tables;

namespace SaaS.WebApp.Services
{
    public class ProductService : IProductService
    {
        private readonly SharedCatalogDbContext _context;
        public ProductService(SharedCatalogDbContext context)
        {
            _context = context;
        }
        public async Task<Product> CreateAsync(string name, string description, int rate)
        {
            var product = new Product(name, description, rate);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var pro = await _context.Products.FindAsync(id);

            if (pro != null) return false;


            _context.Products.Remove(pro);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> UpdateAsync(string name, string description, int rate)
        {
            var product = new Product(name, description, rate);
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            return product;
        }
    }
}
