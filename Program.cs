using Amazon.DynamoDBv2;
using lab3app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(Amazon.RegionEndpoint.USEast1));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext for SQL Server
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection2RDS")));

// Configure login session setting
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseSession();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
