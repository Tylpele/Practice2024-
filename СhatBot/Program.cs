using ÑhatBot.Models;
using ÑhatBot.RabbitMQ;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHostedService<PreHandler>();
builder.Services.AddHostedService<Handler>();
builder.Services.AddHostedService<PostHandler>();
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<Handler>();


builder.Logging.ClearProviders();
builder.Host.UseNLog();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.Run();

