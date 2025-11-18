using RestX.UI.Services.Interfaces;
using RestX.UI.Services.Implementations;
using RestX.UI.Services.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Add Authentication and Authorization services
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Owner", policy =>
        policy.RequireRole("Owner"));
    options.AddPolicy("Staff", policy =>
        policy.RequireRole("Staff"));
    options.AddPolicy("Customer", policy =>
        policy.RequireRole("Customer"));
});

// Session for storing JWT tokens
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register AuthTokenHandler
builder.Services.AddTransient<AuthTokenHandler>();

// HTTP Client services for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7294/"); // RestX.API URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthTokenHandler>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register services
//builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHomeUIService, HomeUIService>();
builder.Services.AddScoped<IMenuUIService, MenuUIService>();
builder.Services.AddScoped<ICartUIService, CartUIService>();
builder.Services.AddScoped<IOwnerUIService, OwnerUIService>();
builder.Services.AddScoped<IStaffUIService, StaffUIService>();
builder.Services.AddScoped<IOrderUIService, OrderUIService>();
builder.Services.AddScoped<ICustomerUIService, CustomerUIService>();
builder.Services.AddScoped<IDishManagementUIService, DishManagementUIService>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();