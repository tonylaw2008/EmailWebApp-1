using EamilWebApp;
using EamilWebApp.Models;
using MailEnhanceService;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var authenticUserList = configuration.GetSection("AuthenticUser").Get<List<AuthenticUserModel>>();
builder.Services.AddSingleton<IList<AuthenticUserModel>>(authenticUserList);
// 提供公司用戶認證服務列表
PubBusiness pubBusiness = new PubBusiness(authenticUserList);
builder.Services.AddSingleton<IPubBusiness, PubBusiness>();  // 注意：由於 authenticUserList 已經注入，所以可以這樣寫，PubBusiness的構造函數自動獲取。

// 默認發送目標郵箱地址
var mailToDefault = configuration.GetSection("MailToDefault").Get<string>()??string.Empty;
builder.Services.AddSingleton<string>(mailToDefault);

builder.Logging.AddLog4Net("log4net.config"); // 指定配置文件路径

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
 
app.UseLog4net();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
