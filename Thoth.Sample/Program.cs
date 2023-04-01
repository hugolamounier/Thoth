using Thoth.Core;
using Thoth.Dashboard;
using Thoth.SQLServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddThoth(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext");
})
    .UseSqlServer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseThothDashboard();
app.Run();

public partial class Program { }