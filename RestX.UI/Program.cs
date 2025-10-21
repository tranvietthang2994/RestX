using RestX.UI.Services.Interfaces;
using RestX.UI.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Session for storing JWT tokens
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HTTP Client services for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5000/"); // RestX.API URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
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