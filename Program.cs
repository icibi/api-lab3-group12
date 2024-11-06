using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using lab3app.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext for SQL Server
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection2RDS")));

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

app.Run();
