var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews()
.AddRazorRuntimeCompilation();
// Add IHttpContextAccessor and session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register SessionManager as scoped so you can inject it
builder.Services.AddScoped<OnlineExamSystem.Common.SessionManager>();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// session must be before authorization or components that rely on it
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=UserLogin}/{id?}");

app.Run();
