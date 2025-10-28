using DotNetMapsFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Read HTTPS configuration from appsettings
var useHttps = builder.Configuration.GetValue<bool>("Server:UseHttps");
var httpPort = builder.Configuration.GetValue<int>("Server:HttpPort", 4200);
var httpsPort = builder.Configuration.GetValue<int>("Server:HttpsPort", 7225);

// Configure URLs based on HTTPS setting
if (useHttps)
{
    builder.WebHost.UseUrls($"https://localhost:{httpsPort}", $"http://localhost:{httpPort}");
}
else
{
    builder.WebHost.UseUrls($"http://localhost:{httpPort}");
}

// Configure logging with custom timestamp format
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "simple";
}).AddSimpleConsole(options =>
{
    options.TimestampFormat = "dd.MM.yyyy HH:mm:ss ";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPointOfInterestService, PointOfInterestService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    // Only use HSTS if HTTPS is enabled
    if (useHttps)
    {
        app.UseHsts();
    }
}

// Only use HTTPS redirection if HTTPS is enabled
if (useHttps)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

// Make Program class accessible for testing
public partial class Program 
{
    protected Program() { }
}
