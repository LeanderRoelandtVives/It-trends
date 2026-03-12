using RestaurantAi.Model;
using RestaurantAi.Mvc.Handlers;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<JwtHandler>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("RestaurantApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7179");
})
.AddHttpMessageHandler<JwtHandler>();

builder.Services.AddSession();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();