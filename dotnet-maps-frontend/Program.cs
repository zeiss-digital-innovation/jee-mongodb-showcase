using DotNetMapsFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure URL to run on localhost:4200
builder.WebHost.UseUrls("http://localhost:4200");

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

// Make Program class accessible for testing
public partial class Program 
{
    protected Program() { }
}
