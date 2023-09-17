using BlogProject.Context;
using BlogProject.NewFolder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add configurations
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddUserSecrets<Program>(); 

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext and Identity
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<BlogDbContext>();


string ApiKey = builder.Configuration["ApiKey"];
// Register Postmark email sender
builder.Services.AddSingleton<IEmailSender>(sp => new PostmarkEmailSender(ApiKey));

// Add controllers and views
builder.Services.AddControllersWithViews();

// Add ADO.NET connection
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Configure routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}");

app.Run();
