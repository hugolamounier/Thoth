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

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseThothDashboard();
app.Run();

public abstract partial class Program { }