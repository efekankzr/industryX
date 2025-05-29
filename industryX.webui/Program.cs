using System.Text;
using IndustryX.Application.Interfaces;
using IndustryX.Application.Services;
using IndustryX.Application.Services.Implementations;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using IndustryX.Infrastructure.Interfaces;
using IndustryX.Infrastructure.Services;
using IndustryX.Infrastructure.Settings;
using IndustryX.Persistence.Contexts;
using IndustryX.Persistence.Repositories;
using IndustryX.Services.Abstract;
using IndustryX.Services.Interfaces;
using IndustryX.WebUI.Services.Abstract;
using IndustryX.WebUI.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// DI: Services
// ----------------------
builder.Services.Configure<IyzicoOptions>(builder.Configuration.GetSection("IyzicoOptions"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IIyzicoService, IyzicoService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILaborCostService, LaborCostService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductStockService, ProductStockService>();
builder.Services.AddScoped<IProductTransferService, ProductTransferService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IRawMaterialStockService, RawMaterialStockService>();
builder.Services.AddScoped<ISalesProductService, SalesProductService>();
builder.Services.AddScoped<ISalesProductStockService, SalesProductStockService>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// ----------------------
// DbContext
// ----------------------
builder.Services.AddDbContext<IndustryXDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// ----------------------
// Identity
// ----------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<IndustryXDbContext>()
.AddDefaultTokenProviders();

// ----------------------
// Authentication (JWT + Cookie)
// ----------------------

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = ".industryx.Security.Cookie",
        SameSite = SameSiteMode.Strict
    };
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true
        };
    });

// ----------------------
// MVC + Razor
// ----------------------
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// ----------------------
// App
// ----------------------
var app = builder.Build();

// ----------------------
// Seed (Roles & Admin)
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var db = services.GetRequiredService<IndustryXDbContext>();
    db.Database.Migrate();

    await DataSeeder.SeedRolesAsync(services, config);
    await DataSeeder.SeedAdminUserAsync(services, config);
}

// ----------------------
// Middleware Pipeline
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// Route Map
// ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
