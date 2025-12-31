using Serilog;
using Zoo.Services;

// 設定 Serilog 結構化日誌記錄
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/zoo-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("啟動動物園導覽系統");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 取代內建的日誌提供者
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddRazorPages();

    // 加入記憶體快取服務
    builder.Services.AddMemoryCache();

    // 註冊 JSON 資料服務
    builder.Services.AddSingleton<IJsonDataService, JsonDataService>();

    var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}
