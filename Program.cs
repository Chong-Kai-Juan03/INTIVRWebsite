using DotNetEnv;
using CloudinaryDotNet;
using Firebase.Database;
using firstconnectfirebase.Services;

// 加载环境变量（优先从.env文件读取）
Env.Load();

var builder = WebApplication.CreateBuilder(args);

/* ========== 服务配置 ========== */
// Cloudinary 配置（建议改为从环境变量读取）
var cloudinaryAccount = new Account(
    Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "dhm7z5t7r",
    Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? "356935324743282",
    Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? "s5_N9-9NcQUPYOWhySQaF8sfOOM"
);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

// Firebase 配置（强制验证配置）
var firebaseUrl = builder.Configuration["Firebase:DatabaseUrl"]
    ?? throw new Exception("缺少 Firebase:DatabaseUrl 配置");
var firebaseSecret = builder.Configuration["Firebase:Secret"]
    ?? throw new Exception("缺少 Firebase:Secret 配置");

builder.Services.AddSingleton<FirebaseClient>(_ => new FirebaseClient(
    firebaseUrl,
    new FirebaseOptions
    {
        AuthTokenAsyncFactory = () => Task.FromResult(firebaseSecret)
    }));

// 应用服务
builder.Services.AddScoped<FirebaseService>();


builder.Services.AddControllersWithViews();

/* ========== 中间件配置 ========== */
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();