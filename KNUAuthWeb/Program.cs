using KNUAuthMYSQLConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "auth",
    pattern: "signin",
    defaults: new { controller = "signIn", action = "Register" });
app.MapControllerRoute(
    name: "auth",
    pattern: "login",
    defaults: new { controller = "signIn", action = "Login" });

app.Run();
