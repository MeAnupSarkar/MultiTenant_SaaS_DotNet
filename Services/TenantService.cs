using Microsoft.Extensions.Options;
using SaaS.WebApp.Model.Config;
using SaaS.WebApp.Infrastruture.Interfaces;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using SaaS.WebApp.Models;
using SaaS.WebApp.Data;

namespace SaaS.WebApp.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantSettings _tenantSettings;
        private HttpContext _httpContext;
        private Tenant _currentTenant;
        private readonly ApplicationDbContext _context;



        public TenantService(IOptions<TenantSettings> tenantSettings, IHttpContextAccessor contextAccessor, ApplicationDbContext _context)
        {
            _tenantSettings = tenantSettings.Value;
            _httpContext = contextAccessor.HttpContext;
            this._context = _context;

            //this._userManager = _userManager;

            if (_httpContext != null)
            {
                if (_httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
                {
                    SetTenant(tenantId);
                }
                else
                {
                    try
                    {
                        //if()
                        // throw new Exception("Invalid Tenant!");
                        var tid = _httpContext.Session.GetString("TenantId");
                        if (tid is not null)
                        {
                            // Debug.WriteLine("Authenticated User");

                            SetTenant(tid);

                        }
                        else
                        {
                            Debug.WriteLine("TenantId Not Found!");
                        }

                    }catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }


                }
            }

        }

        private void SetTenant(string tenantId)
        {
            _currentTenant = _context.Tenants.Where(a => a.TID == tenantId).FirstOrDefault();

            if (_currentTenant == null) throw new Exception("Invalid Tenant!");

            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                SetDefaultConnectionStringToCurrentTenant();
            }
        }
        private void SetDefaultConnectionStringToCurrentTenant()
        {
            _currentTenant.ConnectionString = _tenantSettings.Defaults.ConnectionString;
        }
        public string GetConnectionString()
        {
            return _currentTenant?.ConnectionString;
        }


        public string GetDatabaseProvider()
        {
            return _tenantSettings.Defaults?.DBProvider;
        }





        public Tenant GetTenant()
        {
            return _currentTenant;
        }
    }
}
