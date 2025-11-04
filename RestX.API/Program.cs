//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using QRCoder;
//using RestX.API.Data.Contexts;
//using RestX.API.Data.Repository.Implementations;
//using RestX.API.Data.Repository.Interfaces;
//using RestX.API.Hubs;
//using RestX.API.Models.Configuration;
//using RestX.API.Services.Implementations;
//using RestX.API.Services.Interfaces;
//using RestX.API.Extensions;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// API Controllers Configuration với JSON cycle handling
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        // Fix JSON serialization cycles
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//        options.JsonSerializerOptions.WriteIndented = true; // For better readability in development
//    });
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
//    { 
//        Title = "RestX API", 
//        Version = "v1",
//        Description = "Restaurant Management System API"
//    });

//    // Add JWT Authentication to Swagger
////    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
////    {
////        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
////        Name = "Authorization",
////        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
////        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
////        Scheme = "Bearer"
////    });

////    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
////    {
////        {
////            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
////            {
////                Reference = new Microsoft.OpenApi.Models.OpenApiReference
////                {
////                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
////                    Id = "Bearer"
////                }
////            },
////            new string[] {}
////        }
////    });
////});

////// JWT Configuration
////builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
////var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

////// JWT Authentication
////builder.Services.AddAuthentication(options =>
////{
////    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
////    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
////    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
////})
////.AddJwtBearer(options =>
////{
////    options.SaveToken = true;
////    options.RequireHttpsMetadata = false; // Set to true in production
////    options.TokenValidationParameters = new TokenValidationParameters
////    {
////        ValidateIssuer = true,
////        ValidateAudience = true,
////        ValidateLifetime = true,
////        ValidateIssuerSigningKey = true,
////        ValidIssuer = jwtSettings?.Issuer,
////        ValidAudience = jwtSettings?.Audience,
////        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),
////        ClockSkew = TimeSpan.Zero
////    };

////    // Handle JWT in SignalR
////    options.Events = new JwtBearerEvents
////    {
////        OnMessageReceived = context =>
////        {
////            var accessToken = context.Request.Query["access_token"];
////            var path = context.HttpContext.Request.Path;
////            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/signalrServer"))
////            {
////                context.Token = accessToken;
////            }
////            return Task.CompletedTask;
////        }
////    };
////});

////builder.Services.AddAuthorization();

//// HTTP Context
//builder.Services.AddHttpContextAccessor();

//// Session (optional for API, mainly for SignalR)
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(60);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});
//builder.Services.AddSignalR();

//builder.Services.AddScoped<IRepository, EntityFrameworkRepository<RestXRestaurantManagementContext>>();
//builder.Services.AddScoped<IReadOnlyRepository, EntityFrameworkReadOnlyRepository<RestXRestaurantManagementContext>>();
//builder.Services.AddScoped<IAuthCustomerService, AuthCustomerService>();
//builder.Services.AddScoped<IExceptionHandler, ExceptionHandler>();
//builder.Services.AddScoped<ICustomerService, CustomerService>();
//builder.Services.AddScoped<IOwnerService, OwnerService>();
//builder.Services.AddScoped<IDishService, DishService>();
//builder.Services.AddScoped<IHomeService, HomeService>();
//builder.Services.AddScoped<ITableService, TableService>();
//builder.Services.AddScoped<IMenuService, MenuService>();
//builder.Services.AddScoped<ICategoryService, CategoryService>();
//builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<ILoginService, LoginService>();
//builder.Services.AddScoped<IStaffService, StaffService>();
//builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
//builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
//builder.Services.AddScoped<IIngredientImportService, IngredientImportService>();
//builder.Services.AddScoped<IDashboardService, DashboardService>();
//builder.Services.AddScoped<IDishManagementService, DishManagementService>();
//builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<ICustomerService, CustomerService>();
//builder.Services.AddScoped<QRCodeGenerator>();
//builder.Services.AddScoped<IAiService, AiService>();
//builder.Services.AddHttpClient<IAiService, AiService>();

//// JWT Service
//builder.Services.AddScoped<IJwtService, JwtService>();

//// Database Configuration
//builder.Services.AddDbContext<RestXRestaurantManagementContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("RestX"),
//        sqlOptions =>
//        {
//            sqlOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(30),
//                errorNumbersToAdd: null);
//        });

//    if (builder.Environment.IsDevelopment())
//    {
//        options.EnableSensitiveDataLogging();
//        options.EnableDetailedErrors();
//    }
//});

//// AutoMapper
//builder.Services.AddAutoMapper(typeof(Program));

//// Note: JWT Authentication already configured above

//// File upload configuration
//builder.Services.Configure<FormOptions>(options =>
//{
//    options.ValueLengthLimit = int.MaxValue;
//    options.MultipartBodyLengthLimit = int.MaxValue;
//    options.MultipartHeadersLengthLimit = int.MaxValue;
//});

//// CORS Configuration for API
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//var app = builder.Build();

//// Configure Swagger for Development and Production
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestX API v1");
//    c.RoutePrefix = string.Empty; // Make Swagger UI available at root
//});

//// Configure HTTP pipeline for API
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseExceptionHandler("/api/error");
//    app.UseHsts();
//}

//// Set up UserHelper for API
//UserHelper.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

//// Middleware pipeline
//app.UseHttpsRedirection();
//app.UseCors("AllowAll");
//app.UseRouting();
////app.UseSession();
////app.UseAuthentication();
////app.UseAuthorization();

//// API Controllers
//app.MapControllers();

//// SignalR Hubs
//app.MapHub<SignalrServer>("/signalrServer");
//app.MapHub<TableStatusHub>("/tableStatusHub");

//// Redirect root to Swagger UI
//app.MapGet("/", () => Results.Redirect("/swagger"));

//app.Run();



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
builder.Services.AddControllers(options =>
    {
        // Ensure RestaurantContext (OwnerId/TableId) is populated from route data for services
        options.Filters.Add<RestaurantContextFilterAttribute>();
    })
    .AddJsonOptions(options =>
    {
        // Fix JSON serialization cycles
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RestX API",
        Version = "v1",
        Description = "Restaurant Management System API (Test Mode - No Auth)"
    });
});

// 🟩 Tạm thời bỏ xác thực để test API/UI
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//builder.Services.AddAuthentication(...);
//builder.Services.AddAuthorization();

// HTTP Context
builder.Services.AddHttpContextAccessor();

// Session (tạm bỏ nếu không cần test login)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SignalR
builder.Services.AddSignalR();

// 🧩 Dependency Injection
builder.Services.AddScoped<IRepository, EntityFrameworkRepository<RestXRestaurantManagementContext>>();
builder.Services.AddScoped<IReadOnlyRepository, EntityFrameworkReadOnlyRepository<RestXRestaurantManagementContext>>();
builder.Services.AddScoped<IExceptionHandler, ExceptionHandler>();
builder.Services.AddScoped<IAuthCustomerService, AuthCustomerService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IStaffManagementService, StaffManagementService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IIngredientImportService, IngredientImportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDishManagementService, DishManagementService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<QRCodeGenerator>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHttpClient<IAiService, AiService>();

// Database
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

// File upload configuration
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// CORS
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

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestX API v1 (No Auth)");
    c.RoutePrefix = string.Empty;
});

// Error Handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/api/error");
    app.UseHsts();
}

// User Helper
UserHelper.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

// Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();

// 🟨 Comment xác thực để test
//app.UseSession();
//app.UseAuthentication();
//app.UseAuthorization();

// Controllers
app.MapControllers();

// SignalR
app.MapHub<SignalrServer>("/signalrServer");
app.MapHub<TableStatusHub>("/tableStatusHub");

// Default redirect
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
