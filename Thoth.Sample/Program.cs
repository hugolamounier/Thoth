using Thoth.Core;
using Thoth.Dashboard;
using Thoth.SQLServer;
using Thoth.Tests.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddThoth(options => { options.ConnectionString = builder.Configuration.GetConnectionString("SqlContext"); })
    .UseSqlServer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseThothDashboard(options =>
{
    options.RoutePrefix = "/thoth";
    if (args.Any(x => x.Contains("UseThothAuthorization")))
        options.Authorization = new[] {new ThothAuthorizationFilter()};
});

app.Run();

public abstract partial class Program
{
}