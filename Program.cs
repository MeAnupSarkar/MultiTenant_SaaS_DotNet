
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SaaS.WebApp.Data;
using SaaS.WebApp.Infrastructure.Extensions;
using SaaS.WebApp.Infrastruture.Interfaces;
using SaaS.WebApp.Model.Config;
using SaaS.WebApp.Models;
using SaaS.WebApp.Services;
 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multitenant.Api", Version = "v1" });
});




var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));


builder.Services.AddDbContext<SharedCatalogDbContext>(options =>  options.UseSqlServer(builder.Configuration.GetConnectionString("TenantSharedCatalogDbConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

 

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews(options =>
{
    // Enable Antiforgery feature by default on all controller actions
    // options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//    options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie();

builder.Services.AddTransient<ITenantService, TenantService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(nameof(TenantSettings)));
await builder.Services.AddAndMigrateTenantDatabasesAsync(builder.Configuration);


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".SaaS.WebApp";
    options.IdleTimeout = TimeSpan.FromSeconds(60 * 60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(eb =>
{
    eb.MapRazorPages();
    eb.MapControllerRoute("default", "{controller=home}/{action=index}/{id?}");

});



app.Run();
