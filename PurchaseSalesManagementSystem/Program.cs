using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Åö Session ÇégÇ§ÇΩÇﬂÇ…ïKóv
builder.Services.AddSession();

builder.Services.AddSingleton<CreateConnection>();

builder.Services.AddScoped<Repository_Login>();
builder.Services.AddTransient<Repository_Menu>();
builder.Services.AddScoped<Repository_PurchaseOrder>();
builder.Services.AddScoped<Repository_POSeizo>();
builder.Services.AddScoped<Repository_OrderForExport>();
builder.Services.AddScoped<Repository_InvConv>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default", 
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
