using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using RestX.API.Data.Contexts;
using RestX.API.Data.Repository.Implementations;
using RestX.API.Data.Repository.Interfaces;
using RestX.API.Hubs;
using RestX.API.Services.Implementations;
using RestX.API.Services.Interfaces;
using RestX.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// API Controllers Configuration với JSON cycle handling
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Fix JSON serialization cycles
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // For better readability in development
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "RestX API", 
        Version = "v1",
        Description = "Restaurant Management System API"
    });
});

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Session (if needed for API)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();

builder.Services.AddScoped<IRepository, EntityFrameworkRepository<RestXRestaurantManagementContext>>();
builder.Services.AddScoped<IReadOnlyRepository, EntityFrameworkReadOnlyRepository<RestXRestaurantManagementContext>>();
builder.Services.AddScoped<IAuthCustomerService, AuthCustomerService>();
builder.Services.AddScoped<IExceptionHandler, ExceptionHandler>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IIngredientImportService, IngredientImportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDishManagementService, DishManagementService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<QRCodeGenerator>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHttpClient<IAiService, AiService>();

// Database Configuration
builder.Services.AddDbContext<RestXRestaurantManagementContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestX"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Authentication (JWT for API - will be configured later)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});

// File upload configuration
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// CORS Configuration for API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure Swagger for Development and Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestX API v1");
    c.RoutePrefix = string.Empty; // Make Swagger UI available at root
});

// Configure HTTP pipeline for API
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/api/error");
    app.UseHsts();
}

// Set up UserHelper for API
UserHelper.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// API Controllers
app.MapControllers();

// SignalR Hubs
app.MapHub<SignalrServer>("/signalrServer");
app.MapHub<TableStatusHub>("/tableStatusHub");

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();