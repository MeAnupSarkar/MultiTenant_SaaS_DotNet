using Infrastructure.Azure;
using Microsoft.AspNetCore.Mvc;
using SaaS.WebApp.Data;

namespace SaaS.WebApp.Controllers.Azure
{
    public class ShardManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly SharedCatalogDbContext _contextSharedCatalogDb;

        public ShardManagementController(ApplicationDbContext _context, SharedCatalogDbContext _contextSharedCatalogDb)
        {
            this._context = _context;
            this._contextSharedCatalogDb = _contextSharedCatalogDb;
        }


        public IActionResult Index()
        {
            //ShardingManager manager = new ShardingManager(_context, _contextSharedCatalogDb);
            //manager.init();
            return View();
        }
    }
}
