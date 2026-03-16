using Microsoft.AspNetCore.Authentication.Cookies;
using RestaurantAi.Model;
using RestaurantAi.Mvc.Handlers;
using RestaurantAi.Mvc.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtHandler>();

builder.Services.AddControllersWithViews();
// Use configured ApiBaseUrl for all named API clients to avoid port mismatches
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/";

builder.Services.AddHttpClient("RestaurantApi", client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<JwtHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie();

var googleSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
    });
}

builder.Services.AddSession();

// Register AI Service
builder.Services.AddSingleton<AIService>();

// HTTP client for backend API (ensure JwtHandler is attached so JWT from session is forwarded)
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<JwtHandler>();

// Register auth api client
builder.Services.AddScoped<RestaurantAi.Mvc.Services.AuthApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
